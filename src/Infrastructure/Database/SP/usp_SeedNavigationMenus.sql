-- Seed navigation menu master data (idempotent by MenuName)
CREATE OR ALTER PROCEDURE [dbo].[usp_SeedNavigationMenus]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Menus TABLE (
        SortOrder INT,
        MenuName NVARCHAR(255),
        ParentMenuName NVARCHAR(255) NULL,
        MenuUrl VARCHAR(255) NULL,
        IconClass VARCHAR(100) NULL,
        MenuKey VARCHAR(100) NULL
    );

    INSERT INTO @Menus (SortOrder, MenuName, ParentMenuName, MenuUrl, IconClass, MenuKey) VALUES
    (1,  N'Dashboard', NULL, N'/Admin', N'ri-dashboard-2-line', NULL),
    (2,  N'Location Management', NULL, N'#locationmanagement', N'ri-map-pin-line', N'locationmanagement'),
    (3,  N'States', N'Location Management', N'/Admin/States', NULL, NULL),
    (4,  N'Districts', N'Location Management', N'/Admin/Districts', NULL, NULL),
    (5,  N'Blocks', N'Location Management', N'/Admin/Blocks', NULL, NULL),
    (6,  N'Villages', N'Location Management', N'/Admin/Villages', NULL, NULL),
    (7,  N'Settings', NULL, N'#settinglist', N'ri-settings-3-line', N'settinglist'),
    (8,  N'Divisions', N'Settings', N'/Admin/Divisions', NULL, NULL),
    (9,  N'Institutions', N'Settings', N'/Admin/Institutions', NULL, NULL),
    (10, N'Programs', N'Settings', N'/Admin/Programs', NULL, NULL),
    (11, N'Menu Permissions', N'Settings', N'/Admin/MenuPermissions', NULL, NULL),
    (12, N'Roles', N'Settings', N'/Admin/Roles', NULL, NULL),
    (13, N'Users', NULL, N'#userslist', N'ri-user-line', N'userslist'),
    (14, N'Peoples', N'Users', N'/Admin/Peoples', NULL, NULL),
    (15, N'Kadam', NULL, N'#kadamlist', N'ri-book-line', N'kadamlist'),
    (16, N'Subjects', N'Kadam', N'/Admin/Kadam/Subjects', NULL, NULL),
    (17, N'Steps', N'Kadam', N'/Admin/Kadam/Steps', NULL, NULL),
    (18, N'Themes', N'Kadam', N'/Admin/Kadam/Themes', NULL, NULL),
    (19, N'Reports', NULL, N'#reportlist', N'ri-file-edit-line', N'reportlist'),
    (20, N'Kadam Programme Report', N'Reports', N'/Admin/Report', NULL, NULL),
    (21, N'Student Attendance Report', N'Reports', N'/Admin/AttendanceReport', NULL, NULL),
    (22, N'Students', N'Settings', N'/Admin/Students', NULL, NULL);

    ;WITH ParentMenus AS (
        SELECT m.MenuName, m.Id
        FROM MenuPermissions m
        WHERE m.IsDeleted = 0
    )
    INSERT INTO MenuPermissions (MenuName, ParentId, MenuUrl, IconClass, SortOrder, MenuKey, CurrentStatus, IsDeleted, DateCreated)
    SELECT
        src.MenuName,
        p.Id,
        src.MenuUrl,
        src.IconClass,
        src.SortOrder,
        src.MenuKey,
        1,
        0,
        GETDATE()
    FROM @Menus src
    LEFT JOIN ParentMenus p ON p.MenuName = src.ParentMenuName
    WHERE NOT EXISTS (
        SELECT 1
        FROM MenuPermissions existing
        WHERE existing.IsDeleted = 0
          AND existing.MenuName = src.MenuName
          AND ISNULL(existing.ParentId, 0) = ISNULL(p.Id, 0)
    );

    UPDATE child
    SET
        child.MenuUrl = src.MenuUrl,
        child.IconClass = src.IconClass,
        child.SortOrder = src.SortOrder,
        child.MenuKey = src.MenuKey,
        child.ParentId = parentMenu.Id
    FROM MenuPermissions child
    INNER JOIN @Menus src ON src.MenuName = child.MenuName AND child.IsDeleted = 0
    LEFT JOIN MenuPermissions parentMenu ON parentMenu.MenuName = src.ParentMenuName AND parentMenu.IsDeleted = 0
    WHERE src.ParentMenuName IS NOT NULL;
END
GO
