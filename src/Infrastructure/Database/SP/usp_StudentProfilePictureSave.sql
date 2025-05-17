Create Or Alter PROCEDURE [dbo].[usp_StudentProfilePictureSave]
    @Id INT,
    @ProfilePicturePath Varchar(255)
AS
BEGIN
     Update Students Set ProfilePicturePath = @ProfilePicturePath Where Id = @Id
End
