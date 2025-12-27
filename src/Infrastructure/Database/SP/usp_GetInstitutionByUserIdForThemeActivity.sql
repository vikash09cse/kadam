ALTER   Procedure [dbo].[usp_GetInstitutionByUserIdForThemeActivity] --usp_GetInstitutionByUserIdForThemeActivity 1
@UserId Int
As
Begin

    Declare @InstitutionIds VarChar(2000)

    Select @InstitutionIds = InstitutionIds From PeopleInstitutions Where UserId = @UserId

    Select Id , InstitutionName 
	From Institutions 
	Where IsDeleted = 0 
	And Id In (Select Item From SplitString(@InstitutionIds, ','))

	Select igs.InstitutionId, igs.GradeId As Id, g.GradeName, igs.Sections,
		Coalesce(
			Stuff((
				Select ', ' + LTrim(RTrim(s.Section)) + ':' + Cast(Count(*) As Varchar(10))
				From Students s
				Where s.InstitutionId = igs.InstitutionId 
					And s.GradeId = igs.GradeId
					And (s.IsDeleted = 0 Or s.IsDeleted Is Null)
				Group By LTrim(RTrim(s.Section))
				Order By LTrim(RTrim(s.Section))
				For Xml Path('')
			), 1, 2, '')
		, '') As StudentCount
	From InstitutionGradeSections igs
	Inner Join Grades g on g.Id = igs.GradeId
	Where igs.InstitutionId In (Select Item From SplitString(@InstitutionIds, ','))
	Order by g.Id
End