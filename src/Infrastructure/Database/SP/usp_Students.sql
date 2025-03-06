Create Or Alter Procedure [dbo].[usp_Students]
(
 @QueryType			Int,
 @Id				Int=Null,
 @StudentRegistratioNumber			Varchar(100)=Null,
 @AadhaarCardNumber			Varchar(100)=Null,
 @DeletedBy			INT = 0,
 @CreatedBy			INT = 0
 
)
AS
	If @QueryType=1 -- Check if the student registration number is already in use
		BEGIN
			Select Count(1) From Students Where IsDeleted=0 AND StudentRegistratioNumber=@StudentRegistratioNumber AND Id!=@Id
		End
    Else If @QueryType=2 -- Check if the Aadhaar card number is already in use
        BEGIN
            Select Count(1) From Students Where IsDeleted=0 AND AadhaarCardNumber=@AadhaarCardNumber AND Id!=@Id
        End
	Else If @QueryType=3 -- Delete a student
		BEGIN
			Update Students Set IsDeleted=1, DeletedBy=@DeletedBy, DeletedDate=GetDate() Where Id=@Id
		End
	Else If @QueryType=4 -- Get a student by ID
		BEGIN
			Select 
				Id, StudentId, FirstName, LastName, Gender, DateOfBirth,
				DoYouHaveAadhaarCard, AadhaarCardNumber, InstitutionId,
				SectionId, GradeId, StudentRegistratioNumber,
				ChildStatudBeforeKadamSTC, HowLongPlaningToStayThisArea,
				Class, Reasons, DropoutClass, DropoutYear, CurrentStatus,
				CreatedBy, DateCreated, ModifyBy, ModifyDate,
				DeletedBy, DeletedDate, IsDeleted
			From Students Where Id=@Id
		End
	Else If @QueryType=5 -- Get all students
		BEGIN
			Select 
				Id, StudentId, FirstName, LastName, Gender, DateOfBirth,
				DoYouHaveAadhaarCard, AadhaarCardNumber, InstitutionId,
				SectionId, GradeId, StudentRegistratioNumber,
				ChildStatudBeforeKadamSTC, HowLongPlaningToStayThisArea,
				Class, Reasons, DropoutClass, DropoutYear, CurrentStatus,
				CreatedBy, DateCreated, ModifyBy, ModifyDate,
				DeletedBy, DeletedDate, IsDeleted
			From Students Where IsDeleted=0
		End


