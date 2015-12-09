USE [DataWarehouse]
GO
/****** Object:  Table [dbo].[RedmineIssueJournal]    Script Date: 09-12-2015 16:51:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RedmineIssueJournal](
	[id] [int] NOT NULL,
	[issueid] [nchar](10) NULL,
	[userid] [int] NULL,
	[username] [nvarchar](150) NULL,
	[notes] [nvarchar](max) NULL,
	[created_on] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
