CREATE OR ALTER PROCEDURE usp_GetRolesDropDown
AS
BEGIN
    SELECT Id as [Value], RoleName as [Text] FROM Roles WHERE IsDeleted = 0
END