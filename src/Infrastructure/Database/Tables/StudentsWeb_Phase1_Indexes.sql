IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Students')
      AND name = N'IX_Students_WebList'
)
    CREATE NONCLUSTERED INDEX IX_Students_WebList
    ON dbo.Students (InstitutionId, CurrentStatus, EnrollmentDate DESC, Id DESC)
    INCLUDE (CreatedBy, StudentId, FirstName, LastName, AadhaarCardNumber,
             GradeId, Section, Age, DateEntryPoint)
    WHERE IsDeleted = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Students')
      AND name = N'IX_Students_WebList_CreatedBy'
)
    CREATE NONCLUSTERED INDEX IX_Students_WebList_CreatedBy
    ON dbo.Students (CreatedBy, EnrollmentDate DESC, Id DESC)
    INCLUDE (InstitutionId, CurrentStatus)
    WHERE IsDeleted = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentTrios')
      AND name = N'IX_StudentTrios_StudentId_IsDeleted'
)
    CREATE NONCLUSTERED INDEX IX_StudentTrios_StudentId_IsDeleted
    ON dbo.StudentTrios (StudentId, IsDeleted)
    INCLUDE (TrioId);
GO

IF EXISTS (
    SELECT StudentId
    FROM dbo.StudentTrios
    WHERE IsDeleted = 0
    GROUP BY StudentId
    HAVING COUNT(*) > 1
)
    THROW 51001, 'Duplicate active StudentTrios rows must be resolved before creating the unique index.', 1;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentTrios')
      AND name = N'UX_StudentTrios_ActiveStudent'
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_StudentTrios_ActiveStudent
    ON dbo.StudentTrios (StudentId)
    WHERE IsDeleted = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentTrios')
      AND name = N'IX_StudentTrios_ActiveTrio'
)
    CREATE NONCLUSTERED INDEX IX_StudentTrios_ActiveTrio
    ON dbo.StudentTrios (TrioId, StudentId)
    WHERE IsDeleted = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.PeopleInstitutions')
      AND name = N'IX_PeopleInstitutions_UserId'
)
    CREATE NONCLUSTERED INDEX IX_PeopleInstitutions_UserId
    ON dbo.PeopleInstitutions (UserId)
    INCLUDE (InstitutionIds);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Institutions')
      AND name = N'IX_Institutions_DivisionId_IsDeleted'
)
    CREATE NONCLUSTERED INDEX IX_Institutions_DivisionId_IsDeleted
    ON dbo.Institutions (DivisionId, IsDeleted)
    INCLUDE (InstitutionName);
GO

IF EXISTS (
    SELECT StudentId
    FROM dbo.StudentMainstreams
    GROUP BY StudentId
    HAVING COUNT(*) > 1
)
    THROW 51000, 'Duplicate StudentMainstreams rows must be resolved before creating the unique index.', 1;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentMainstreams')
      AND name = N'UX_StudentMainstreams_StudentId'
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_StudentMainstreams_StudentId
    ON dbo.StudentMainstreams (StudentId);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentBaselineDetails')
      AND name = N'IX_StudentBaselineDetails_WebEligibility'
)
    CREATE NONCLUSTERED INDEX IX_StudentBaselineDetails_WebEligibility
    ON dbo.StudentBaselineDetails (StudentId, BaselineType)
    INCLUDE (CompletedDate)
    WHERE IsDeleted = 0;
GO
