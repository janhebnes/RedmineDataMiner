USE [DataWarehouse]
GO
/****** Object:  Table [dbo].[RedmineIssue]    Script Date: 09-12-2015 16:51:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RedmineIssue](
	[id] [int] NOT NULL,
	[project_id] [int] NULL,
	[project_name] [nvarchar](250) NULL,
	[tracker_id] [int] NULL,
	[tracker_name] [nvarchar](250) NULL,
	[status_id] [int] NULL,
	[status_name] [nvarchar](250) NULL,
	[priority_id] [int] NULL,
	[priority_name] [nvarchar](250) NULL,
	[author_id] [int] NULL,
	[author_name] [nvarchar](250) NULL,
	[subject] [nvarchar](500) NULL,
	[description] [nvarchar](max) NULL,
	[start_date] [date] NULL,
	[due_date] [date] NULL,
	[done_ratio] [int] NULL,
	[estimated_hours] [decimal](18, 0) NULL,
	[created_on] [datetime] NULL,
	[updated_on] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
