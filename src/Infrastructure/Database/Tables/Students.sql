CREATE TABLE [dbo].[Students](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StudentId] [varchar](100) NOT NULL,
	[EnrollmentDate] [date] NOT NULL,
	[ProfilePicture] [varchar](155) NULL,
	[FirstName] [varchar](50) NOT NULL,
	[LastName] [varchar](50) NOT NULL,
	[GenderId] [int] NULL,
	[DateOfBirth] [date] NOT NULL,
	[Age] [int] NULL,
	[DoYouHaveAadhaarCard] [bit] NOT NULL,
	[AadhaarCardNumber] [varchar](16) NOT NULL,
	[InstitutionId] [int] NOT NULL,
	[GradeId] [int] NOT NULL,
	[Section] [varchar](25) NULL,
	[StudentRegistratioNumber] [varchar](50) NOT NULL,
	[ChildStatudBeforeKadamSTC] [int] NULL,
	[HowLongPlaningToStayThisArea] [int] NULL,
	[Class] [int] NULL,
	[ReasonId] [int] NULL,
	[DropoutClass] [varchar](25) NULL,
	[DropoutYear] [int] NULL,
	[CurrentStatus] [int] NOT NULL,
	[CreatedBy] [int] NULL,
	[DateCreated] [datetime] NULL,
	[ModifyBy] [int] NULL,
	[ModifyDate] [datetime] NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime] NULL,
	[IsDeleted] [bit] NULL,
	[ProfilePicturePath] [varchar](255) NULL,
	[PromotionDate] [datetime] NULL,
	[IsKadamPlusStudent] [bit] NOT NULL,
	[Remarks] [varchar](255) NULL,
	[InActiveReason] [varchar](100) NULL,
	[InActiveDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Students] ADD  DEFAULT (getdate()) FOR [DateCreated]
GO

ALTER TABLE [dbo].[Students] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[Students] ADD  DEFAULT ((1)) FOR [IsKadamPlusStudent]
GO


