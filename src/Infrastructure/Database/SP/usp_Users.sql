Create Or Alter Procedure [dbo].[usp_Users]
(
 @QueryType			Int,
 @Id				Int=Null,
 @Email				Varchar(100)=Null
)
AS
	If @QueryType=1
		BEGIN
			Select Count(1) From Users Where Email=@Email AND Id!=@Id
		End
	Else If @QueryType=2
		BEGIN
			Delete Users Where Id=@Id
		End
	Else If @QueryType=3
		BEGIN
			Select 
				Id, Email, UserName, PasswordHash, PasswordSalt, FirstName, LastName, 
				Phone, AlternatePhone, Gender, Grade, Section, GradeSection, DivisionId, 
				RoleId, ReporteeRoleId, UserStatus, ActivityType, IsDeleted, DateCreated, 
				CreatedBy, ModifyDate, ModifyBy, DeletedDate, DeletedBy
			From Users Where Id=@Id
		End
	Else If @QueryType=4
		BEGIN
			Select 
				Id, Email, UserName, PasswordHash, PasswordSalt, FirstName, LastName, 
				Phone, AlternatePhone, Gender, Grade, Section, GradeSection, DivisionId, 
				RoleId, ReporteeRoleId, UserStatus, ActivityType, IsDeleted, DateCreated, 
				CreatedBy, ModifyDate, ModifyBy, DeletedDate, DeletedBy
			From Users 
		End