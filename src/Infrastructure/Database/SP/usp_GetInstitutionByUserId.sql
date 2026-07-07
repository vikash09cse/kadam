ALTER   Procedure [dbo].[usp_GetInstitutionByUserId] --usp_GetInstitutionByUserId 3
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
    SELECT igs.InstitutionId, igs.GradeId As Id, g.GradeName, igs.Sections
    FROM InstitutionGradeSections igs
    Inner Join Grades g on g.Id = igs.GradeId
    Inner Join UserInstitutions ui on ui.InstitutionId = igs.InstitutionId
    Order By G.Id
End
