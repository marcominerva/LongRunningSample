CREATE TABLE [dbo].[Executions](
	[Id] [uniqueidentifier] NOT NULL,
	[JobId] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CompletedAt] [datetime] NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Exception] [nvarchar](max) NULL,
 CONSTRAINT [PK_Executions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))