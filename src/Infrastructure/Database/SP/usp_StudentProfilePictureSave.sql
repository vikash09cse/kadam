Create Or Alter PROCEDURE [dbo].[usp_StudentProfilePictureSave]
    @Id INT,
    @ProfilePicturePath Varchar(255)
AS
BEGIN
     UPDATE Students
     SET ProfilePicturePath = @ProfilePicturePath,
         DateEntryPoint = 1
     WHERE Id = @Id
End
