CREATE OR ALTER PROCEDURE dbo.usp_EnsureNavigationMenusSeeded
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.MenuPermissions
        WHERE IsDeleted = 0
          AND MenuUrl IS NOT NULL
    )
    BEGIN
        EXEC dbo.usp_SeedNavigationMenus;
    END
END
