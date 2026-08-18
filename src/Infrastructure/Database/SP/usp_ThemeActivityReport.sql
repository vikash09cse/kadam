CREATE OR ALTER PROCEDURE dbo.usp_ThemeActivityReport
    @UserId INT = NULL,
    @StateId INT = NULL,
    @DivisionId INT = NULL,
    @InstitutionId INT = NULL,
    @ThemeId INT = NULL,
    @GradeId INT = NULL,
    @Section VARCHAR(25) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @InstitutionIds VARCHAR(MAX) = NULL;
    DECLARE @FilterByInstitution BIT = 0;
    DECLARE @FilterByDivision BIT = 0;

    IF @UserId IS NOT NULL AND @UserId > 0
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM dbo.Users u
            INNER JOIN dbo.Roles r ON u.RoleId = r.Id AND r.IsDeleted = 0
            WHERE u.Id = @UserId
              AND u.IsDeleted = 0
              AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
        )
        BEGIN
            SET @FilterByInstitution = 0;
            SET @FilterByDivision = 0;
        END
        ELSE IF EXISTS (SELECT 1 FROM dbo.PeopleDivisions WHERE UserId = @UserId)
        BEGIN
            SET @FilterByDivision = 1;
        END
        ELSE
        BEGIN
            SET @FilterByInstitution = 1;
            SELECT @InstitutionIds = STRING_AGG(CAST(LTRIM(RTRIM(s.Item)) AS VARCHAR(20)), ',')
            FROM dbo.PeopleInstitutions pi
            CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') s
            WHERE pi.UserId = @UserId
              AND LTRIM(RTRIM(ISNULL(pi.InstitutionIds, ''))) <> ''
              AND TRY_CAST(LTRIM(RTRIM(s.Item)) AS INT) IS NOT NULL;
        END
    END

    SELECT
        ROW_NUMBER() OVER (ORDER BY ta.ThemeActivityDate DESC, ta.Id DESC) AS RowNumber,
        ta.Id,
        ta.ThemeActivityDate,
        ta.InstitutionId,
        i.InstitutionName,
        ta.ThemeId,
        t.ThemeName,
        ISNULL(summary.GradeSectionsText, '') AS GradeSectionsText,
        ta.TotalStudents,
        ta.StudentAttended,
        ta.DidChildrenDayHappen,
        ta.TotalParentsAttended,
        ta.StudentAttended + ISNULL(ta.TotalParentsAttended, 0) AS TotalParticipants,
        ta.DateEntryPoint,
        CASE WHEN ISNULL(ta.DateEntryPoint, 1) = 2 THEN N'Web' ELSE N'Mobile' END AS EntryPointText,
        ta.CreatedBy,
        LTRIM(RTRIM(CONCAT(ISNULL(u.FirstName, ''), ' ', ISNULL(u.LastName, '')))) AS CreatedByName
    FROM dbo.ThemeActivities ta
    INNER JOIN dbo.Institutions i ON i.Id = ta.InstitutionId AND i.IsDeleted = 0
    INNER JOIN dbo.Themes t ON t.Id = ta.ThemeId AND t.IsDeleted = 0
    LEFT JOIN dbo.Users u ON u.Id = ta.CreatedBy
    OUTER APPLY
    (
        SELECT STRING_AGG(CONVERT(NVARCHAR(MAX), pairs.GradeSection), ', ')
               WITHIN GROUP (ORDER BY pairs.GradeSection) AS GradeSectionsText
        FROM
        (
            SELECT DISTINCT
                CASE
                    WHEN tags.Section IS NULL OR LTRIM(RTRIM(tags.Section)) = ''
                    THEN g.GradeName
                    ELSE g.GradeName + ' - ' + LTRIM(RTRIM(tags.Section))
                END AS GradeSection
            FROM dbo.ThemeActivityGradeSections tags
            INNER JOIN dbo.Grades g ON g.Id = tags.GradeId
            WHERE tags.ThemeActivityId = ta.Id
        ) pairs
    ) summary
    WHERE ISNULL(ta.IsDeleted, 0) = 0
      AND (
          (@FilterByInstitution = 0 AND @FilterByDivision = 0)
          OR (
              @FilterByDivision = 1
              AND i.DivisionId IN (
                  SELECT pd.DivisionId
                  FROM dbo.PeopleDivisions pd
                  WHERE pd.UserId = @UserId
              )
          )
          OR (
              @FilterByInstitution = 1
              AND @InstitutionIds IS NOT NULL
              AND LTRIM(RTRIM(@InstitutionIds)) <> ''
              AND i.Id IN (
                  SELECT TRY_CAST(LTRIM(RTRIM(Item)) AS INT)
                  FROM dbo.SplitString(@InstitutionIds, ',')
                  WHERE TRY_CAST(LTRIM(RTRIM(Item)) AS INT) IS NOT NULL
              )
          )
      )
      AND (@StateId IS NULL OR @StateId = 0 OR i.StateId = @StateId)
      AND (@DivisionId IS NULL OR @DivisionId = 0 OR i.DivisionId = @DivisionId)
      AND (@InstitutionId IS NULL OR @InstitutionId = 0 OR ta.InstitutionId = @InstitutionId)
      AND (@ThemeId IS NULL OR @ThemeId = 0 OR ta.ThemeId = @ThemeId)
      AND (
          @GradeId IS NULL
          OR @GradeId = 0
          OR EXISTS (
              SELECT 1
              FROM dbo.ThemeActivityGradeSections tags_filter
              WHERE tags_filter.ThemeActivityId = ta.Id
                AND tags_filter.GradeId = @GradeId
          )
      )
      AND (
          @Section IS NULL
          OR LTRIM(RTRIM(@Section)) = ''
          OR EXISTS (
              SELECT 1
              FROM dbo.ThemeActivityGradeSections tags_filter
              CROSS APPLY dbo.SplitString(tags_filter.Section, ',') split
              WHERE tags_filter.ThemeActivityId = ta.Id
                AND LOWER(LTRIM(RTRIM(split.Item))) = LOWER(LTRIM(RTRIM(@Section)))
          )
      )
      AND (@FromDate IS NULL OR CAST(ta.ThemeActivityDate AS DATE) >= @FromDate)
      AND (@ToDate IS NULL OR CAST(ta.ThemeActivityDate AS DATE) <= @ToDate)
    ORDER BY ta.ThemeActivityDate DESC, ta.Id DESC;
END
GO
