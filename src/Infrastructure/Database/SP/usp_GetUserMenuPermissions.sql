CREATE OR ALTER PROCEDURE [dbo].[usp_GetUserMenuPermissions]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.Id,
        m.MenuName,
        m.ParentId,
        p.MenuName AS ParentMenuName,
        m.SortOrder,
        CASE WHEN ump.Id IS NULL THEN 0 ELSE 1 END AS IsSelected
    FROM MenuPermissions m
    LEFT JOIN MenuPermissions p ON m.ParentId = p.Id AND p.IsDeleted = 0
    LEFT JOIN UserMenuPermissions ump ON ump.MenuId = m.Id
        AND ump.UserId = @UserId
        AND ump.IsDeleted = 0
    WHERE m.IsDeleted = 0
      AND m.CurrentStatus = 1
    ORDER BY ISNULL(m.ParentId, m.Id), m.SortOrder, m.Id;
END
GO
