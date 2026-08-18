-- Latest data entry channel: 1 = Mobile, 2 = Web.
-- Existing rows are treated as Mobile because the web module did not exist.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.Students', 'DateEntryPoint') IS NULL
    ALTER TABLE dbo.Students ADD DateEntryPoint TINYINT NOT NULL
        CONSTRAINT DF_Students_DateEntryPoint DEFAULT (1) WITH VALUES;

IF COL_LENGTH('dbo.StudentFamilyDetails', 'DateEntryPoint') IS NULL
    ALTER TABLE dbo.StudentFamilyDetails ADD DateEntryPoint TINYINT NOT NULL
        CONSTRAINT DF_StudentFamilyDetails_DateEntryPoint DEFAULT (1) WITH VALUES;

IF COL_LENGTH('dbo.StudentHealths', 'DateEntryPoint') IS NULL
    ALTER TABLE dbo.StudentHealths ADD DateEntryPoint TINYINT NOT NULL
        CONSTRAINT DF_StudentHealths_DateEntryPoint DEFAULT (1) WITH VALUES;

IF COL_LENGTH('dbo.StudentDocuments', 'DateEntryPoint') IS NULL
    ALTER TABLE dbo.StudentDocuments ADD DateEntryPoint TINYINT NOT NULL
        CONSTRAINT DF_StudentDocuments_DateEntryPoint DEFAULT (1) WITH VALUES;

IF COL_LENGTH('dbo.StudentTrios', 'DateEntryPoint') IS NULL
    ALTER TABLE dbo.StudentTrios ADD DateEntryPoint TINYINT NOT NULL
        CONSTRAINT DF_StudentTrios_DateEntryPoint DEFAULT (1) WITH VALUES;

COMMIT TRANSACTION;
