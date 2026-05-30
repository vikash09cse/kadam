CREATE OR ALTER PROCEDURE dbo.usp_GetAdminDashboardCount
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FilterByUser BIT = 1;

    IF EXISTS (
        SELECT 1
        FROM dbo.Users u
        INNER JOIN dbo.Roles r ON u.RoleId = r.Id AND r.IsDeleted = 0
        WHERE u.Id = @UserId
          AND u.IsDeleted = 0
          AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
    )
        SET @FilterByUser = 0;

    SELECT
        ISNULL(SUM(CASE WHEN s.CurrentStatus = 1 THEN 1 ELSE 0 END), 0) AS ActiveCount,
        ISNULL(SUM(CASE WHEN s.CurrentStatus = 2 THEN 1 ELSE 0 END), 0) AS InactiveCount,
        ISNULL(SUM(CASE WHEN s.CurrentStatus = 3 THEN 1 ELSE 0 END), 0) AS CompletedCount
    FROM dbo.Students s
    WHERE s.IsDeleted = 0
      AND (@FilterByUser = 0 OR s.CreatedBy = @UserId);
END
