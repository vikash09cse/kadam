IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StudentDeleteLogs' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.StudentDeleteLogs
    (
        Id                INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
        StudentRecordId   INT NOT NULL,
        KadamStudentId    VARCHAR(50) NULL,
        StudentName       NVARCHAR(200) NOT NULL,
        InstitutionId     INT NULL,
        InstitutionName   NVARCHAR(200) NULL,
        DeletedBy         INT NOT NULL,
        DeletedByName     NVARCHAR(200) NULL,
        DeletedDate       DATETIME NOT NULL DEFAULT(GETUTCDATE())
    );

    CREATE NONCLUSTERED INDEX IX_StudentDeleteLogs_StudentRecordId
        ON dbo.StudentDeleteLogs (StudentRecordId);

    CREATE NONCLUSTERED INDEX IX_StudentDeleteLogs_DeletedDate
        ON dbo.StudentDeleteLogs (DeletedDate DESC);
END
