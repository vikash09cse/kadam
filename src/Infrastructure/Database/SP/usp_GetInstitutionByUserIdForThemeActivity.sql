ALTER   Procedure [dbo].[usp_GetInstitutionByUserIdForThemeActivity] --usp_GetInstitutionByUserIdForThemeActivity 1
@UserId Int
As
Begin
    SET NOCOUNT ON;

    ;WITH UserInstitutions AS (
        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(s.Item)) AS INT) AS InstitutionId
        FROM PeopleInstitutions pi
        CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') s
        WHERE pi.UserId = @UserId
          AND LTRIM(RTRIM(ISNULL(pi.InstitutionIds, ''))) <> ''
          AND TRY_CAST(LTRIM(RTRIM(s.Item)) AS INT) IS NOT NULL
    )
    SELECT i.Id, i.InstitutionName
    FROM Institutions i
    INNER JOIN UserInstitutions ui ON ui.InstitutionId = i.Id
    WHERE i.IsDeleted = 0;

    ;WITH UserInstitutions AS (
        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(s.Item)) AS INT) AS InstitutionId
        FROM PeopleInstitutions pi
        CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') s
        WHERE pi.UserId = @UserId
          AND LTRIM(RTRIM(ISNULL(pi.InstitutionIds, ''))) <> ''
          AND TRY_CAST(LTRIM(RTRIM(s.Item)) AS INT) IS NOT NULL
    )
    SELECT igs.InstitutionId, igs.GradeId As Id, g.GradeName, igs.Sections,
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
    Inner Join UserInstitutions ui on ui.InstitutionId = igs.InstitutionId
    Order by g.Id
End
