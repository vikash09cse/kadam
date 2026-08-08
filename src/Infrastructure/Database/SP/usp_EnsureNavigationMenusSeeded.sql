CREATE OR ALTER PROCEDURE dbo.usp_EnsureNavigationMenusSeeded
AS
BEGIN
    SET NOCOUNT ON;

    -- The seed procedure is idempotent and also updates menu metadata.
    -- Always execute it so newly deployed module pages are registered.
    EXEC dbo.usp_SeedNavigationMenus;
END
