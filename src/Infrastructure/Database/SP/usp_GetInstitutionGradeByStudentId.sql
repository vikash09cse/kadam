Create OR ALTER   Procedure [dbo].[usp_GetInstitutionGradeByStudentId] --usp_GetInstitutionGradeByStudentId 3
@StudentId Int
As
Begin

    Declare @InstitutionId Int

    Select @InstitutionId = InstitutionId From Students Where Id = @StudentId


	Select igs.Id, igs.InstitutionId, igs.GradeId, g.GradeName, igs.Sections
	From InstitutionGradeSections igs
	Inner Join Grades g on g.Id = igs.GradeId
	Where igs.InstitutionId = @InstitutionId

End