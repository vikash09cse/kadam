CREATE OR ALTER PROCEDURE [dbo].[usp_GetInstitutionByUserIdForThemeActivity] --usp_GetInstitutionByUserIdForThemeActivity 1
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

    ;WITH AssignedRows AS (
        SELECT
            TRY_CAST(LTRIM(RTRIM(pi.InstitutionIds)) AS INT) AS InstitutionId,
            pi.GradeAndSection
        FROM PeopleInstitutions pi
        WHERE pi.UserId = @UserId
          AND TRY_CAST(LTRIM(RTRIM(pi.InstitutionIds)) AS INT) IS NOT NULL
    ),
    FromJson AS (
        SELECT
            ar.InstitutionId,
            CAST(j.GradeId AS INT) AS GradeId,
            CAST(j.Sections AS VARCHAR(55)) AS Sections
        FROM AssignedRows ar
        CROSS APPLY OPENJSON(ar.GradeAndSection)
        WITH (
            GradeId INT '$.GradeId',
            Sections VARCHAR(55) '$.Sections'
        ) j
        WHERE ar.GradeAndSection IS NOT NULL
          AND LTRIM(RTRIM(ar.GradeAndSection)) <> ''
          AND ISJSON(ar.GradeAndSection) = 1
    ),
    FromInstitution AS (
        SELECT
            igs.InstitutionId,
            igs.GradeId,
            igs.Sections
        FROM InstitutionGradeSections igs
        INNER JOIN AssignedRows ar ON ar.InstitutionId = igs.InstitutionId
        WHERE ar.GradeAndSection IS NULL
           OR LTRIM(RTRIM(ar.GradeAndSection)) = ''
           OR ISJSON(ar.GradeAndSection) = 0
    ),
    Combined AS (
        SELECT InstitutionId, GradeId, Sections FROM FromJson
        UNION ALL
        SELECT InstitutionId, GradeId, Sections FROM FromInstitution
    )
    SELECT
        c.InstitutionId,
        c.GradeId AS Id,
        g.GradeName,
        c.Sections,
        COALESCE(
            STUFF((
                SELECT ', ' + LTRIM(RTRIM(s.Section)) + ':' + CAST(COUNT(*) AS VARCHAR(10))
                FROM Students s
                WHERE s.InstitutionId = c.InstitutionId
                    AND s.GradeId = c.GradeId
                    AND (s.IsDeleted = 0 OR s.IsDeleted IS NULL)
                GROUP BY LTRIM(RTRIM(s.Section))
                ORDER BY LTRIM(RTRIM(s.Section))
                FOR XML PATH('')
            ), 1, 2, '')
        , '') AS StudentCount
    FROM Combined c
    INNER JOIN Grades g ON g.Id = c.GradeId
    WHERE g.IsDeleted = 0
    ORDER BY g.Id;
End
GO
