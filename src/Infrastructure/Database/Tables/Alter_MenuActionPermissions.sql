IF COL_LENGTH('dbo.UserMenuPermissions', 'CanAddEdit') IS NULL
BEGIN
    ALTER TABLE dbo.UserMenuPermissions ADD CanAddEdit BIT NOT NULL CONSTRAINT DF_UserMenuPermissions_CanAddEdit DEFAULT (1);
END

IF COL_LENGTH('dbo.UserMenuPermissions', 'CanDelete') IS NULL
BEGIN
    ALTER TABLE dbo.UserMenuPermissions ADD CanDelete BIT NOT NULL CONSTRAINT DF_UserMenuPermissions_CanDelete DEFAULT (1);
END

IF COL_LENGTH('dbo.RolePermissions', 'CanAddEdit') IS NULL
BEGIN
    ALTER TABLE dbo.RolePermissions ADD CanAddEdit BIT NOT NULL CONSTRAINT DF_RolePermissions_CanAddEdit DEFAULT (1);
END

IF COL_LENGTH('dbo.RolePermissions', 'CanDelete') IS NULL
BEGIN
    ALTER TABLE dbo.RolePermissions ADD CanDelete BIT NOT NULL CONSTRAINT DF_RolePermissions_CanDelete DEFAULT (1);
END
GO
