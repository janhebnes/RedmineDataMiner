using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineDataMiner
{
    /// <summary>
    /// Handle Database operations
    /// </summary>
    /// <remarks>The default network packet size is 4,096 bytes.</remarks>
    /// <see cref="http://technet.microsoft.com/en-us/library/ms177437.aspx"/>
    public class DataStore
    {
        private StringBuilder _query = new StringBuilder();
        private int _bufferLength = 0;
        private static string ConnectionString
        {
            get
            {
                if (ConfigurationManager.ConnectionStrings["Data"] == null)
                    return string.Empty;

                return ConfigurationManager.ConnectionStrings["Data"].ConnectionString;
            }
        }

        public static bool HasConnectionString
        {
            get
            {
                return (!String.IsNullOrWhiteSpace(ConnectionString));
            }
        }

        private static byte[] ConvertToBinary(string str)
        {
            var encoding = new System.Text.UTF8Encoding();
            return encoding.GetBytes(str);
        }

        /// <summary>
        /// Handles buffering optimale query packet size for Sql Server
        /// Remember to call Flush for allowing the last queries to be executed
        /// </summary>
        /// <remarks>Remember to call Flush for allowing the last queries to be executed</remarks>
        /// <param name="query"></param>
        public void Execute(string query)
        {
            if (String.IsNullOrWhiteSpace(query)) return;

            var queryBinaryLength = ConvertToBinary(query).Length;
            if ((_bufferLength + queryBinaryLength) <= 4096)
            {
                _query.AppendLine(query);
                _bufferLength += queryBinaryLength;
                return;
            }

            this.ExecuteRaw(_query.ToString());
            _query = new StringBuilder(query);
            _bufferLength = queryBinaryLength;
        }

        public void Flush()
        {
            ExecuteRaw(_query.ToString());
            _query = new StringBuilder();
            _bufferLength = 0;
        }

        /// <summary>
        /// Execute directly to sql 
        /// </summary>
        /// <param name="query"></param>
        public void ExecuteRaw(string query)
        {
            if (String.IsNullOrWhiteSpace(query)) return;

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = query;
                try
                {
                    var result = cmd.ExecuteNonQuery();
                }
                catch (SqlException sex)
                {
                    throw new Exception(string.Format("Could not execute query: {0}", query), sex);
                }

            }
        }

    }
}
