SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.StudentBaselineDetails', 'DateEntryPoint') IS NULL
    ALTER TABLE dbo.StudentBaselineDetails
    ADD DateEntryPoint TINYINT NOT NULL
        CONSTRAINT DF_StudentBaselineDetails_DateEntryPoint DEFAULT (1);

IF COL_LENGTH('dbo.StudentProgressSteps', 'DateEntryPoint') IS NULL
    ALTER TABLE dbo.StudentProgressSteps
    ADD DateEntryPoint TINYINT NOT NULL
        CONSTRAINT DF_StudentProgressSteps_DateEntryPoint DEFAULT (1);

IF COL_LENGTH('dbo.StudentGradeTestDetails', 'DateEntryPoint') IS NULL
    ALTER TABLE dbo.StudentGradeTestDetails
    ADD DateEntryPoint TINYINT NOT NULL
        CONSTRAINT DF_StudentGradeTestDetails_DateEntryPoint DEFAULT (1);

IF COL_LENGTH('dbo.StudentGradeTestDetails', 'CurrentStatus') IS NULL
    ALTER TABLE dbo.StudentGradeTestDetails
    ADD CurrentStatus INT NOT NULL
        CONSTRAINT DF_StudentGradeTestDetails_CurrentStatus DEFAULT (1);

IF COL_LENGTH('dbo.StudentGradeTestDetails', 'ModifyBy') IS NULL
    ALTER TABLE dbo.StudentGradeTestDetails ADD ModifyBy INT NULL;

IF COL_LENGTH('dbo.StudentGradeStartAndEndDetails', 'DateEntryPoint') IS NULL
    ALTER TABLE dbo.StudentGradeStartAndEndDetails
    ADD DateEntryPoint TINYINT NOT NULL
        CONSTRAINT DF_StudentGradeStartAndEndDetails_DateEntryPoint DEFAULT (1);

IF COL_LENGTH('dbo.StudentMainstreams', 'DateEntryPoint') IS NULL
    ALTER TABLE dbo.StudentMainstreams
    ADD DateEntryPoint TINYINT NOT NULL
        CONSTRAINT DF_StudentMainstreams_DateEntryPoint DEFAULT (1);

COMMIT TRANSACTION;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

UPDATE dbo.StudentGradeTestDetails SET IsDeleted = 0 WHERE IsDeleted IS NULL;
UPDATE dbo.StudentGradeTestDetails SET CurrentStatus = 1 WHERE CurrentStatus IS NULL;
UPDATE dbo.StudentGradeTestDetails SET CreatedBy = 0 WHERE CreatedBy IS NULL;
UPDATE dbo.StudentGradeTestDetails SET DateCreated = GETDATE() WHERE DateCreated IS NULL;

COMMIT TRANSACTION;
GO

IF EXISTS (
    SELECT StudentId, BaselineType, SubjectId
    FROM dbo.StudentBaselineDetails
    WHERE IsDeleted = 0
    GROUP BY StudentId, BaselineType, SubjectId
    HAVING COUNT(*) > 1
)
    THROW 51010, 'Resolve duplicate active assessment subject rows before creating Phase 2 indexes.', 1;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentBaselineDetails')
      AND name = N'UX_StudentBaselineDetails_ActiveAssessmentSubject'
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_StudentBaselineDetails_ActiveAssessmentSubject
    ON dbo.StudentBaselineDetails (StudentId, BaselineType, SubjectId)
    WHERE IsDeleted = 0;
GO

IF EXISTS (
    SELECT StudentId
    FROM dbo.StudentGradeStartAndEndDetails
    GROUP BY StudentId
    HAVING COUNT(*) > 1
)
    THROW 51011, 'Resolve duplicate StudentGradeStartAndEndDetails rows before creating Phase 2 indexes.', 1;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentGradeStartAndEndDetails')
      AND name = N'UX_StudentGradeStartAndEndDetails_StudentId'
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_StudentGradeStartAndEndDetails_StudentId
    ON dbo.StudentGradeStartAndEndDetails (StudentId);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentProgressSteps')
      AND name = N'IX_StudentProgressSteps_StudentId_StepId'
)
    CREATE NONCLUSTERED INDEX IX_StudentProgressSteps_StudentId_StepId
    ON dbo.StudentProgressSteps (StudentId, StepId)
    INCLUDE (IsCompleted);
GO

IF EXISTS (
    SELECT StudentId, StepId
    FROM dbo.StudentProgressSteps
    GROUP BY StudentId, StepId
    HAVING COUNT(*) > 1
)
    THROW 51012, 'Resolve duplicate StudentProgressSteps rows before creating Phase 2 unique indexes.', 1;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentProgressSteps')
      AND name = N'UX_StudentProgressSteps_StudentId_StepId'
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_StudentProgressSteps_StudentId_StepId
    ON dbo.StudentProgressSteps (StudentId, StepId);
GO

IF EXISTS (
    SELECT StudentId, GradeLevelId, SubjectId
    FROM dbo.StudentGradeTestDetails
    WHERE IsDeleted = 0
    GROUP BY StudentId, GradeLevelId, SubjectId
    HAVING COUNT(*) > 1
)
    THROW 51013, 'Resolve duplicate active StudentGradeTestDetails rows before creating Phase 2 unique indexes.', 1;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentGradeTestDetails')
      AND name = N'UX_StudentGradeTestDetails_ActiveGradeSubject'
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_StudentGradeTestDetails_ActiveGradeSubject
    ON dbo.StudentGradeTestDetails (StudentId, GradeLevelId, SubjectId)
    WHERE IsDeleted = 0;
GO
