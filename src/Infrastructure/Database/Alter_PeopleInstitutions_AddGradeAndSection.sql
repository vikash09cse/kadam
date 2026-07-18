-- Add GradeAndSection JSON column to PeopleInstitutions (safe for existing data)
IF COL_LENGTH('dbo.PeopleInstitutions', 'GradeAndSection') IS NULL
BEGIN
    ALTER TABLE dbo.PeopleInstitutions
    ADD GradeAndSection NVARCHAR(MAX) NULL;
END
GO
