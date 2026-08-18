CREATE OR ALTER PROCEDURE [dbo].[usp_GetUserMenuPermissions]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @PortalType TINYINT = 1;
    DECLARE @RoleId INT = 0;

    SELECT
        @PortalType = ISNULL(r.PortalType, 1),
        @RoleId = ISNULL(u.RoleId, 0)
    FROM Users u
    INNER JOIN Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
    WHERE u.Id = @UserId AND u.IsDeleted = 0;

    SELECT
        m.Id,
        m.MenuName,
        m.ParentId,
        p.MenuName AS ParentMenuName,
        m.SortOrder,
        m.MenuUrl,
        CASE WHEN ump.Id IS NOT NULL OR rp.Id IS NOT NULL THEN 1 ELSE 0 END AS IsSelected,
        CASE WHEN rp.Id IS NOT NULL AND ump.Id IS NULL THEN 1 ELSE 0 END AS IsInherited,
        CASE
            WHEN ump.Id IS NOT NULL THEN ISNULL(ump.CanAddEdit, 0)
            WHEN rp.Id IS NOT NULL THEN ISNULL(rp.CanAddEdit, 0)
            ELSE 0
        END AS CanAddEdit,
        CASE
            WHEN ump.Id IS NOT NULL THEN ISNULL(ump.CanDelete, 0)
            WHEN rp.Id IS NOT NULL THEN ISNULL(rp.CanDelete, 0)
            ELSE 0
        END AS CanDelete
    FROM MenuPermissions m
    LEFT JOIN MenuPermissions p ON m.ParentId = p.Id AND p.IsDeleted = 0
    LEFT JOIN UserMenuPermissions ump ON ump.MenuId = m.Id
        AND ump.UserId = @UserId
        AND ump.IsDeleted = 0
    LEFT JOIN RolePermissions rp ON rp.MenuId = m.Id
        AND rp.RoleId = @RoleId
        AND rp.IsDeleted = 0
        AND rp.CurrentStatus = 1
    WHERE m.IsDeleted = 0
      AND m.CurrentStatus = 1
      AND m.PortalType = @PortalType
    ORDER BY ISNULL(m.ParentId, m.Id), m.SortOrder, m.Id;
END
GO
