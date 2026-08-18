CREATE OR ALTER Procedure [dbo].[usp_UserLoginValidate]
	@UserName			Varchar(100)
AS
BEGIN
	SELECT u.Id, u.FirstName, u.LastName, u.Email, u.RoleId, u.ReporteeRoleId,
           u.UserName, u.PasswordHash, u.PasswordSalt, ISNULL(r.PortalType, 1) PortalType
		FROM Users u
        INNER JOIN Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
		WHERE u.IsDeleted=0 and u.UserStatus=1 and u.UserName=@UserName
END