USE [DataWarehouse]
GO
/****** Object:  Table [dbo].[RedmineIssueJournalDetail]    Script Date: 09-12-2015 16:51:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RedmineIssueJournalDetail](
	[journalid] [int] NOT NULL,
	[property] [nvarchar](50) NULL,
	[name] [nvarchar](50) NULL,
	[old_value] [nvarchar](max) NULL,
	[new_value] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
