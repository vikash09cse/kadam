Create Or Alter Procedure usp_GetInstitutionByUserId --usp_GetInstitutionByUserId 3
@UserId Int
As
Begin

    Declare @InstitutionIds VarChar(2000)

    Select @InstitutionIds = InstitutionIds From PeopleInstitutions Where UserId = @UserId

    Select Id , InstitutionName 
	From Institutions 
	Where IsDeleted = 0 
	And Id In (Select Item From SplitString(@InstitutionIds, ','))

	Select igs.Id, igs.InstitutionId, igs.GradeId, g.GradeName, igs.Sections
	From InstitutionGradeSections igs
	Inner Join Grades g on g.Id = igs.GradeId
	Where igs.InstitutionId In (Select Item From SplitString(@InstitutionIds, ','))
End
