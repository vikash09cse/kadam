CREATE OR ALTER PROCEDURE usp_GetRolesDropDown
AS
BEGIN
    SELECT
        Id AS [Value],
        RoleName AS [Text],
        ISNULL(AllowMultipleDivision, 0) AS AllowMultipleDivision
    FROM Roles
    WHERE IsDeleted = 0
END
GO
