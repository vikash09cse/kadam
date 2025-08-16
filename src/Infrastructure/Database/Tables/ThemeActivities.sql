Create Table ThemeActivities
(
    Id INT PRIMARY KEY IDENTITY(1,1),
    ThemeId INT NOT NULL,
    InstitutionId INT NOT NULL,
    GradeId INT NOT NULL,
    Section Varchar(25),
    TotalStudents INT NOT NULL,
    StudentAttended INT NOT NULL,
    DidChildrenDayHappen BIt NOT NULL Default(0),
    TotalParentsAttended INT,
    CurrentStatus INT NOT NULL,
    ThemeActivityDate DATETIME,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
)