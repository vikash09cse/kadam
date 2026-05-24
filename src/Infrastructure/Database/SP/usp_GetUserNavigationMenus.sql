CREATE OR ALTER PROCEDURE [dbo].[usp_GetUserNavigationMenus]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IsAdmin BIT = 0;

    IF EXISTS (
        SELECT 1
        FROM Users u
        INNER JOIN Roles r ON u.RoleId = r.Id AND r.IsDeleted = 0
        WHERE u.Id = @UserId
          AND u.IsDeleted = 0
          AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
    )
        SET @IsAdmin = 1;

    ;WITH AllowedMenuIds AS (
        SELECT m.Id
        FROM MenuPermissions m
        WHERE m.IsDeleted = 0
          AND m.CurrentStatus = 1
          AND (
              @IsAdmin = 1
              OR m.Id IN (
                  SELECT ump.MenuId
                  FROM UserMenuPermissions ump
                  WHERE ump.UserId = @UserId AND ump.IsDeleted = 0
              )
          )
    ),
    VisibleMenuIds AS (
        SELECT Id FROM AllowedMenuIds
        UNION
        SELECT m.ParentId
        FROM MenuPermissions m
        INNER JOIN AllowedMenuIds a ON m.Id = a.Id
        WHERE m.ParentId IS NOT NULL AND m.ParentId > 0
    )
    SELECT
        m.Id,
        m.MenuName,
        m.ParentId,
        m.MenuUrl,
        m.IconClass,
        m.MenuKey,
        m.SortOrder
    FROM MenuPermissions m
    WHERE m.IsDeleted = 0
      AND m.CurrentStatus = 1
      AND m.Id IN (SELECT Id FROM VisibleMenuIds)
    ORDER BY ISNULL(m.ParentId, m.Id), m.SortOrder, m.Id;
END
GO
