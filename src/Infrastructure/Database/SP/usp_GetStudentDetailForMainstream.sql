Create Or Alter Procedure dbo.usp_GetStudentDetailForMainstream
@Id			INT
AS
	BEGIN
		Select S.Id as StudentId, FirstName,LastName, S.InstitutionId, S.InstitutionId, I.InstitutionName,
		S.GradeId, S.Section,
		I.StateId, I.DistrictId
		,I.InstitutionCode
		,G.GradeName as EnrolledGrade 
		From Students S 
			Inner JOIN Institutions I On S.InstitutionId=I.Id
			Left Join Grades G On S.GradeId=G.Id
		Where S.Id=@Id
	END
