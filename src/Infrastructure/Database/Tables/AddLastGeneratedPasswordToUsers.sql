-- Stores the last admin-generated or manually set password for display on People list (internal admin use).
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = 'LastGeneratedPassword'
)
BEGIN
    ALTER TABLE dbo.Users ADD LastGeneratedPassword NVARCHAR(200) NULL;
END
GO
