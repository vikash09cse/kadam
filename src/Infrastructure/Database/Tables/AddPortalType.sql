SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.Roles', 'PortalType') IS NULL
    ALTER TABLE dbo.Roles ADD PortalType TINYINT NOT NULL
        CONSTRAINT DF_Roles_PortalType DEFAULT (1) WITH VALUES;

IF COL_LENGTH('dbo.MenuPermissions', 'PortalType') IS NULL
    ALTER TABLE dbo.MenuPermissions ADD PortalType TINYINT NOT NULL
        CONSTRAINT DF_MenuPermissions_PortalType DEFAULT (1) WITH VALUES;

UPDATE dbo.MenuPermissions
SET MenuName = 'Student List'
WHERE MenuName = 'Student Directory'
  AND MenuUrl IN ('/Students', '/StudentPortal/Directory');

UPDATE dbo.MenuPermissions
SET MenuName = 'Student Portal',
    MenuUrl = '#studentportal',
    MenuKey = 'studentportal',
    PortalType = 2
WHERE MenuName = 'Student Operations' OR MenuKey = 'studentoperations';

UPDATE dbo.MenuPermissions
SET MenuUrl = CASE MenuName
        WHEN 'Student Dashboard' THEN '/StudentPortal/Dashboard'
        WHEN 'Student List' THEN '/StudentPortal/Directory'
        WHEN 'Student Registration' THEN '/StudentPortal/Registration'
        WHEN 'Student Health' THEN '/StudentPortal/Health'
        WHEN 'Student Documents' THEN '/StudentPortal/Documents'
    END,
    PortalType = 2
WHERE MenuName IN (
    'Student Dashboard', 'Student List', 'Student Registration',
    'Student Health', 'Student Documents'
);

DECLARE @CanonicalMenus TABLE (
    MenuName NVARCHAR(255) PRIMARY KEY,
    MenuId INT NOT NULL
);

INSERT INTO @CanonicalMenus (MenuName, MenuId)
SELECT MenuName, MIN(Id)
FROM dbo.MenuPermissions
WHERE MenuName IN (
    'Student Portal', 'Student Dashboard', 'Student List',
    'Student Registration', 'Student Health', 'Student Documents'
)
GROUP BY MenuName;

-- Preserve assignments that may point at duplicate menu rows.
MERGE dbo.UserMenuPermissions AS target
USING (
    SELECT DISTINCT ump.UserId, canonical.MenuId, ump.CreatedBy
    FROM dbo.UserMenuPermissions ump
    INNER JOIN dbo.MenuPermissions duplicateMenu ON duplicateMenu.Id = ump.MenuId
    INNER JOIN @CanonicalMenus canonical ON canonical.MenuName = duplicateMenu.MenuName
    WHERE ump.IsDeleted = 0 AND ump.MenuId <> canonical.MenuId
) AS source
ON target.UserId = source.UserId AND target.MenuId = source.MenuId
WHEN MATCHED THEN
    UPDATE SET IsDeleted = 0, CurrentStatus = 1
WHEN NOT MATCHED THEN
    INSERT (UserId, MenuId, CurrentStatus, CreatedBy, DateCreated, IsDeleted)
    VALUES (source.UserId, source.MenuId, 1, source.CreatedBy, GETDATE(), 0);

UPDATE ump
SET IsDeleted = 1, DeletedDate = GETDATE()
FROM dbo.UserMenuPermissions ump
INNER JOIN dbo.MenuPermissions duplicateMenu ON duplicateMenu.Id = ump.MenuId
INNER JOIN @CanonicalMenus canonical ON canonical.MenuName = duplicateMenu.MenuName
WHERE duplicateMenu.Id <> canonical.MenuId AND ump.IsDeleted = 0;

UPDATE duplicateMenu
SET IsDeleted = 1, CurrentStatus = 3, DeletedDate = GETDATE()
FROM dbo.MenuPermissions duplicateMenu
INNER JOIN @CanonicalMenus canonical ON canonical.MenuName = duplicateMenu.MenuName
WHERE duplicateMenu.Id <> canonical.MenuId;

DECLARE @StudentPortalMenuId INT =
    (SELECT MenuId FROM @CanonicalMenus WHERE MenuName = 'Student Portal');

UPDATE menuItem
SET ParentId = CASE WHEN menuItem.MenuName = 'Student Portal' THEN NULL ELSE @StudentPortalMenuId END,
    PortalType = 2,
    IsDeleted = 0,
    CurrentStatus = 1
FROM dbo.MenuPermissions menuItem
INNER JOIN @CanonicalMenus canonical ON canonical.MenuId = menuItem.Id;

COMMIT TRANSACTION;
