Create Table StudentGradeTestDetails
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    GradeLevelId INT NOT NULL,
    SubjectId	Int Not NUll,
	StudentAge	Int Null,
	ObtainedMarks	Decimal(18,2),
	PercentageMarks	Decimal(18,2),
	TotalMarks		Decimal(18,2),
	CompletedDate	Datetime,
	[CreatedBy] [int] NULL,
	[DateCreated] [datetime] NULL,
	[ModifyDate] [datetime] NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime] NULL,
	[IsDeleted] [bit] NULL,
)
