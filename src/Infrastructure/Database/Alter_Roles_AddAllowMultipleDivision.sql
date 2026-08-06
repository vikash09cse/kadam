-- Add AllowMultipleDivision to Roles (safe for existing data)
IF COL_LENGTH('dbo.Roles', 'AllowMultipleDivision') IS NULL
BEGIN
    ALTER TABLE dbo.Roles
    ADD AllowMultipleDivision BIT NOT NULL CONSTRAINT DF_Roles_AllowMultipleDivision DEFAULT (0);
END
GO

-- Enable for existing DO / SPM roles
UPDATE dbo.Roles
SET AllowMultipleDivision = 1
WHERE IsDeleted = 0
  AND LOWER(LTRIM(RTRIM(RoleName))) IN ('do', 'spm');
GO
