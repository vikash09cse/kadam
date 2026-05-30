CREATE OR ALTER PROCEDURE dbo.usp_IsAdminUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(
        CASE
            WHEN EXISTS (
                SELECT 1
                FROM dbo.Users u
                INNER JOIN dbo.Roles r ON u.RoleId = r.Id AND r.IsDeleted = 0
                WHERE u.Id = @UserId
                  AND u.IsDeleted = 0
                  AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
            )
            THEN 1
            ELSE 0
        END AS BIT) AS IsAdmin;
END
