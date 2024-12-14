CREATE OR ALTER Procedure [dbo].[usp_UserLoginValidate]
	@UserName			Varchar(100)
AS
BEGIN
	SELECT Id, FirstName, LastName, Email, RoleId, ReporteeRoleId, UserName, PasswordHash, PasswordSalt
		FROM Users
		WHERE IsDeleted=0 and UserStatus=1 and UserName=@UserName
END