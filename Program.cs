using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RestSharp;
using RestSharp.Deserializers;

namespace RedmineDataMiner
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Using http://restsharp.org/ for parsing feed</remarks>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                int fromDate;
                if (int.TryParse(args[0], out fromDate))
                {
                    HarvestIssues(DateTime.Now.AddDays(fromDate*-1), DateTime.Now, 0);
                }
                else
                {
                    Console.WriteLine("Invalid argument - you must provide the numeric amount of days to lookup as a command line argument");
                }
            }
            else
            {
                Console.WriteLine("Missing argument - you must provide the numeric amount of days to lookup as a command line argument");
            }

            #region Sample Implimentation
            //Redmine API ID for jhe cb8750ff44ab2f423a07f5028e30bef70818be72
            //http://redmine/issues.json
            //http://www.redmine.org/projects/redmine/wiki/Rest_Issues
            //http://www.redmine.org/projects/redmine/wiki/Rest_api#Authentication
            // "X-Redmine-API-Key" HTTP header og så sætter den der ind som std ved hvert request
            // issues.xml?created_on=%3E%3C2012-03-01|2012-03-07
            //client.Authenticator = new HttpBasicAuthenticator("xxx", "xxx");

            //All issues available from 
            //http://redmine/issues.json
            //{"issues":[{"id":9306
            //            ,"project":
            //                {"id":163,
            //                 "name":"KRR Website: kromannreumert.dk"}
            //            ,"tracker":{"id":7,"name":"Projektkrav"}
            //            ,"status":{"id":1,"name":"Modtaget (1508) "}
            //            ,"priority":{"id":4,"name":"Normal"}
            //            ,"author":{"id":30,"name":"Jan Hebnes"}
            //            ,"subject":"4.2.6.1.1  Oprette microsite"
            //            ,"description":"h1. 4.2.6.1.1  Oprette microsite og siden skal udl\u00f8be\r\n"
            //            ,"start_date":"2013-09-19"
            //            ,"done_ratio":0
            //            ,"created_on":"2013-09-19T11:30:21Z"
            //            ,"updated_on":"2013-10-01T12:06:10Z"}
            //            ,{"id":9311,"project":{"id":163,"name":"KRR Website: kromannreumert.dk"},"tracker":{"id":7,"name":"Projektkrav"},"status":{"id":1,"name":"Modtaget (1508) "},"priority":{"id":4,"name":"Normal"},"author":{"id":30,"name":"Jan Hebnes"},"subject":"4.1.1  Find relevant kontaktperson (A)","description":"h1. 4.1.1  Find relevant kontaktperson beskrivelse. \r\n","start_date":"2013-09-19","done_ratio":0,"created_on":"2013-09-19T12:26:59Z","updated_on":"2013-10-01T11:58:04Z"}

            // Documentation at http://restsharp.org/

            //var client = new RestClient("http://redmine");
            //var request = new RestRequest("issues.json?updated_on=%3E%3C{fromdate}|{todate}&limit=100&offset={offset}", Method.GET);
            //request.AddHeader("X-Redmine-API-Key", ConfigurationManager.AppSettings["X-Redmine-API-Key"]); 

            //// replaces matching token in request.Resource
            //request.AddUrlSegment("fromdate", "2013-12-01");
            //request.AddUrlSegment("todate", "2013-12-18");
            //request.AddUrlSegment("offset", "140");

            ////IRestResponse response = client.Execute(request) ;
            //// var content = response.Content; // raw content as string
            //// Console.WriteLine(content.Length);
            //IRestResponse<Issues> response2 = client.Execute<Issues>(request);
            //// Console.WriteLine(response2.Data.total_count);
            //// Console.WriteLine(response2.Data.issues.Count);
            #endregion
        }

        private static void HarvestIssues(DateTime fromDate, DateTime toDate, int offset)
        {
            var client = new RestClient(ConfigurationManager.AppSettings["X-Redmine-URL"]);
            var request = new RestRequest("issues.json?updated_on=%3E%3C{fromdate}|{todate}&limit=100&offset={offset}&status_id=*", Method.GET);
            request.AddHeader("X-Redmine-API-Key", ConfigurationManager.AppSettings["X-Redmine-API-Key"]); // User jhe API Key 
            request.AddUrlSegment("fromdate", fromDate.ToString("yyyy-MM-dd"));
            request.AddUrlSegment("todate", toDate.ToString("yyyy-MM-dd"));
            request.AddUrlSegment("offset", offset.ToString());

            Console.WriteLine("Getting issues updated after " + fromDate.ToString("yyyy-MM-dd"));

            IRestResponse<Issues> response = client.Execute<Issues>(request);
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                Console.WriteLine(response.ErrorMessage);
                if (response.ErrorException != null)
                {
                    throw response.ErrorException;
                }

                throw new Exception(response.ErrorMessage);
            }

            Console.WriteLine("Harvesting " + response.Data.total_count + " Issues");

            int count = offset;
            var store = new DataStore();
            foreach (var issue in response.Data.issues)
            {
                var query = string.Format("DELETE FROM [RedmineIssue] WHERE id = '{0}' ; "
                                          +"INSERT INTO [RedmineIssue]([id],[project_id],[project_name],[tracker_id],[tracker_name],[status_id],[status_name],[priority_id],[priority_name],[author_id],[author_name] "
                                          + ",[subject],[description],[start_date],[due_date],[done_ratio],[estimated_hours],[created_on],[updated_on]) "
                                          +"VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}') ;"
                    , issue.id, issue.project.id, SafeParse(issue.project.name), issue.tracker.id, issue.tracker.name,
                    issue.status.id, issue.status.name, issue.priority.id, issue.priority.name, issue.author.id,
                    issue.author.name, SafeParse(issue.subject), SafeParse(issue.description), issue.start_date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), issue.due_date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), issue.done_ratio, issue.estimated_hours.ToString("F", CultureInfo.InvariantCulture),
                    issue.created_on.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), issue.updated_on.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                store.Execute(query);
                Console.Write(" #" + issue.id);
                count++;
            }
            store.Flush();

            Console.WriteLine(" Harvesting Journals ");
            // Handle parsing of Journal Details
            foreach (var issue in response.Data.issues)
            {
                HarvestIssueJournals(issue.id);
                Console.Write(" #" + issue.id + "...");
            }

            // Loop through the total offset.
            if (response.Data.issues.Count + offset < response.Data.total_count)
            {
                HarvestIssues(fromDate, toDate, offset + 100);
            }
        }

        private static void HarvestIssueJournals(int id)
        {
            var client = new RestClient(ConfigurationManager.AppSettings["X-Redmine-URL"]);
            //client.AddHandler("application/xml", new DotNetXmlDeserializer());
            var request = new RestRequest(string.Format("issues/{0}.xml?include=journals", id), Method.GET);
            request.AddHeader("X-Redmine-API-Key", ConfigurationManager.AppSettings["X-Redmine-API-Key"]); // User jhe API Key 
            //request.RequestFormat = DataFormat.Xml;
            IRestResponse<IssueLight> response = client.Execute<IssueLight>(request);

            if (response.Content == null || string.IsNullOrWhiteSpace(response.Content))
            {
                return;
            }

            var d = new XmlDeserializer();
            var response2 = new RestResponse();
            //HACK: The resulting xml contains a user with two attributes inside the journal, we have not solved how to deserialize this therefore this hack, attributes does not function
            response2.Content = response.Content.Replace("><user id", " userid").Replace("/><notes>", "><notes>");

            response.Data = d.Deserialize<IssueLight>(response2);
            //var result = response.Content;

            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                Console.WriteLine(response.ErrorMessage);
                if (response.ErrorException != null)
                {
                    throw response.ErrorException;
                }

                throw new Exception(response.ErrorMessage);
            }

            if (response.Data.id != id)
            {
                throw new Exception(string.Format("HarvestIssueJournals could not parse response from http://redmine/issues/{0}.json?include=journals to an Issue object receivced:\n {1} ", id, response.Content));
            }

            if (response.Data.journals == null)
            {
                return;
            }

            var store = new DataStore();
            foreach (var journal in response.Data.journals)
            {
                var query = string.Format("DELETE FROM [RedmineIssueJournal] WHERE id = '{0}' ; DELETE FROM [RedmineIssueJournalDetail] WHERE journalid = '{0}' ; "
                                          + "INSERT INTO [RedmineIssueJournal]([id],[issueid],[userid],[username],[notes],[created_on]) "
                                          + "VALUES('{0}','{1}','{2}','{3}','{4}','{5}') ; "
                    , journal.id, response.Data.id, journal.userid, SafeParse(journal.name), SafeParse(journal.notes), journal.created_on.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                store.Execute(query);

                foreach (var detail in journal.details)
                {
                    var query2 = string.Format("INSERT INTO [RedmineIssueJournalDetail]([journalid],[property],[name],[old_value],[new_value]) VALUES('{0}','{1}','{2}','{3}','{4}') ; ",
                            journal.id, detail.property, SafeParse(detail.name), SafeParse(detail.old_value), SafeParse(detail.new_value));
                    store.Execute(query2);
                }
            }
            store.Flush();
        }

        public static string SafeParse(string input)
        {
            string output = input.Replace("\"", "\"\"");
            output = output.Replace("'", "''");
            return output;
        }

        public class Issues
        {
            public List<Issue> issues { get; set; }
            public int total_count { get; set; }
            public int offset { get; set; }
            public int limit { get; set; }
            public string type{ get; set; }
        }

        public class Issue
        {
            public int id { get; set; }
            public Project project { get; set; }
            public Tracker tracker { get; set; }
            public Status status { get; set; }
            public Priority priority { get; set; }
            public Author author { get; set; }
            public string subject { get; set; }
            public string description { get; set; }
            public DateTime start_date { get; set; }
            public DateTime due_date { get; set; }
            public int done_ratio { get; set; }
            public decimal estimated_hours { get; set; }
            public DateTime created_on { get; set; }
            public DateTime updated_on { get; set; }
            public List<Journal> journals { get; set; }
        }
        // Unable to deserialize the full issue but it works with a smaller dataset 
        public class IssueLight
        {
            public int id { get; set; }
            //public Project project { get; set; }
            //public Tracker tracker { get; set; }
            //public Status status { get; set; }
            //public Priority priority { get; set; }
            //public Author author { get; set; }
            public string subject { get; set; }
            public string description { get; set; }
            //public DateTime start_date { get; set; }
            //public DateTime due_date { get; set; }
            //public int done_ratio { get; set; }
            //public decimal estimated_hours { get; set; }
            //public DateTime created_on { get; set; }
            //public DateTime updated_on { get; set; }
            public List<Journal> journals { get; set; }
        }
        public class Project
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        public class Tracker
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        public class Status
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        public class Priority
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        public class Author
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        public class User
        {
            [XmlAttribute("id")]
            public int id { get; set; }
            [XmlAttribute("name")]
            public string name { get; set; }
        }
        //public class UserLight
        //{
        //    [XmlAttribute("id")]
        //    public int Id { get; set; }
        //    [XmlAttribute("name")]
        //    public string Name { get; set; }
        //}
        public class Journal
        {
            [XmlAttribute("id")]
            public int id { get; set; }
            public int userid { get; set; }
            public string name { get; set; }
            // Attributes on xml elements in xmlelements does not seem to deserialize... fucker..  
            //public UserLight user { get; set; }
            public string notes { get; set; }
            public DateTime created_on { get; set; }
            public List<Detail> details { get; set; }
        }
        public class Detail
        {
            public string name { get; set; }
            public string property { get; set; }
            public string old_value { get; set; }
            public string new_value { get; set; }
        }
    }
}
