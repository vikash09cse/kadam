
CREATE TABLE [dbo].[StudentMainstreams](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StudentId] [int] NOT NULL,
	[IsMainstreamInstitutionSame] [bit] NOT NULL,
	[StateId] [int] NULL,
	[DistrictId] [int] NULL,
	[MainstreamInstitutionName] [varchar](100) NULL,
	[SchoolDISECode] [varchar](100) NULL,
	[GradeId] [int] NULL,
	[Section] [varchar](25) NULL,
	[ChildSRNumber] [varchar](100) NULL,
	[MainstreamDate] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[DateCreated] [datetime] NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[StudentMainstreams] ADD  CONSTRAINT [DF_StudentMainstreams_DateCreated]  DEFAULT (getdate()) FOR [DateCreated]
GO


