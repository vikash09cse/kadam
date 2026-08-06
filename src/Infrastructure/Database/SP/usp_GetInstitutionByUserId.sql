CREATE OR ALTER PROCEDURE [dbo].[usp_GetInstitutionByUserId] --usp_GetInstitutionByUserId 3
@UserId Int
As
Begin
    SET NOCOUNT ON;

    DECLARE @HasDivisionAssignments BIT = 0;

    IF EXISTS (SELECT 1 FROM dbo.PeopleDivisions WHERE UserId = @UserId)
        SET @HasDivisionAssignments = 1;

    ;WITH UserInstitutions AS (
        SELECT DISTINCT i.Id AS InstitutionId
        FROM dbo.PeopleDivisions pd
        INNER JOIN dbo.Institutions i ON i.DivisionId = pd.DivisionId AND i.IsDeleted = 0
        WHERE pd.UserId = @UserId
          AND @HasDivisionAssignments = 1

        UNION

        SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(s.Item)) AS INT) AS InstitutionId
        FROM dbo.PeopleInstitutions pi
        CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') s
        WHERE pi.UserId = @UserId
          AND @HasDivisionAssignments = 0
          AND LTRIM(RTRIM(ISNULL(pi.InstitutionIds, ''))) <> ''
          AND TRY_CAST(LTRIM(RTRIM(s.Item)) AS INT) IS NOT NULL
    )
    SELECT i.Id, i.InstitutionName
    FROM dbo.Institutions i
    INNER JOIN UserInstitutions ui ON ui.InstitutionId = i.Id
    WHERE i.IsDeleted = 0
    ORDER BY i.InstitutionName;

    -- Prefer PeopleInstitutions.GradeAndSection JSON when present;
    -- otherwise fall back to InstitutionGradeSections (existing behavior).
    -- For division-scoped users, always use InstitutionGradeSections catalog.
    ;WITH AssignedRows AS (
        SELECT
            i.Id AS InstitutionId,
            CAST(NULL AS NVARCHAR(MAX)) AS GradeAndSection
        FROM dbo.PeopleDivisions pd
        INNER JOIN dbo.Institutions i ON i.DivisionId = pd.DivisionId AND i.IsDeleted = 0
        WHERE pd.UserId = @UserId
          AND @HasDivisionAssignments = 1

        UNION ALL

        SELECT
            TRY_CAST(LTRIM(RTRIM(pi.InstitutionIds)) AS INT) AS InstitutionId,
            pi.GradeAndSection
        FROM dbo.PeopleInstitutions pi
        WHERE pi.UserId = @UserId
          AND @HasDivisionAssignments = 0
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
        FROM dbo.InstitutionGradeSections igs
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
        c.Sections
    FROM Combined c
    INNER JOIN dbo.Grades g ON g.Id = c.GradeId
    WHERE g.IsDeleted = 0
    ORDER BY c.InstitutionId, g.Id;
End
GO
