-- Phase 3 web theme-activity reads only. Web writes are performed through EF Core.
CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetActiveThemes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.Id, t.ThemeName Text
    FROM dbo.Themes t
    WHERE t.IsDeleted = 0 AND t.CurrentStatus = 1
    ORDER BY t.ThemeName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetThemeActivityGradeSections
    @UserId INT,
    @InstitutionId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Institutions i
        WHERE i.Id = @InstitutionId AND i.IsDeleted = 0 AND i.CurrentStatus = 1
          AND
          (
              EXISTS
              (
                  SELECT 1 FROM dbo.Users u
                  INNER JOIN dbo.Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
                  WHERE u.Id = @UserId AND u.IsDeleted = 0
                    AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
              )
              OR EXISTS
              (
                  SELECT 1 FROM dbo.PeopleDivisions pd
                  WHERE pd.UserId = @UserId AND pd.DivisionId = i.DivisionId
              )
              OR EXISTS
              (
                  SELECT 1 FROM dbo.PeopleInstitutions pi
                  CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split
                  WHERE pi.UserId = @UserId
                    AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = i.Id
              )
          )
    )
        RETURN;

    DECLARE @HasAssignedGradeSections BIT =
        CASE WHEN EXISTS
        (
            SELECT 1
            FROM dbo.PeopleInstitutions pi
            CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') institutionSplit
            WHERE pi.UserId = @UserId
              AND TRY_CAST(LTRIM(RTRIM(institutionSplit.Item)) AS INT) = @InstitutionId
              AND ISJSON(pi.GradeAndSection) = 1
        ) THEN 1 ELSE 0 END;

    ;WITH Configured AS
    (
        SELECT DISTINCT
            igs.GradeId,
            LTRIM(RTRIM(sectionSplit.Item)) Section
        FROM dbo.InstitutionGradeSections igs
        CROSS APPLY dbo.SplitString(igs.Sections, ',') sectionSplit
        WHERE igs.InstitutionId = @InstitutionId
          AND NULLIF(LTRIM(RTRIM(sectionSplit.Item)), '') IS NOT NULL
    ),
    Assigned AS
    (
        SELECT DISTINCT
            assignment.GradeId,
            LTRIM(RTRIM(sectionSplit.Item)) Section
        FROM dbo.PeopleInstitutions pi
        CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') institutionSplit
        CROSS APPLY OPENJSON(CASE WHEN ISJSON(pi.GradeAndSection) = 1 THEN pi.GradeAndSection ELSE '[]' END)
        WITH (GradeId INT '$.GradeId', Sections NVARCHAR(1000) '$.Sections') assignment
        CROSS APPLY dbo.SplitString(assignment.Sections, ',') sectionSplit
        WHERE pi.UserId = @UserId
          AND TRY_CAST(LTRIM(RTRIM(institutionSplit.Item)) AS INT) = @InstitutionId
          AND ISJSON(pi.GradeAndSection) = 1
          AND NULLIF(LTRIM(RTRIM(sectionSplit.Item)), '') IS NOT NULL
    ),
    Allowed AS
    (
        SELECT c.GradeId, c.Section
        FROM Configured c
        WHERE @HasAssignedGradeSections = 0
           OR EXISTS
              (
                  SELECT 1 FROM Assigned a
                  WHERE a.GradeId = c.GradeId
                    AND LOWER(a.Section) = LOWER(c.Section)
              )
    )
    SELECT
        g.Id,
        g.GradeName Text,
        @InstitutionId ParentId,
        STRING_AGG(CONVERT(NVARCHAR(MAX), a.Section), ',') WITHIN GROUP (ORDER BY a.Section) Sections
    FROM Allowed a
    INNER JOIN dbo.Grades g ON g.Id = a.GradeId
        AND g.IsDeleted = 0 AND g.CurrentStatus = 1
    GROUP BY g.Id, g.GradeName
    ORDER BY g.Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetThemeActivityEligibleCount
    @UserId INT,
    @InstitutionId INT,
    @GradeSections NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    IF ISJSON(@GradeSections) <> 1
    BEGIN
        SELECT 0;
        RETURN;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Institutions i
        WHERE i.Id = @InstitutionId AND i.IsDeleted = 0 AND i.CurrentStatus = 1
          AND
          (
              EXISTS
              (
                  SELECT 1 FROM dbo.Users u
                  INNER JOIN dbo.Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
                  WHERE u.Id = @UserId AND u.IsDeleted = 0
                    AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
              )
              OR EXISTS
              (
                  SELECT 1 FROM dbo.PeopleDivisions pd
                  WHERE pd.UserId = @UserId AND pd.DivisionId = i.DivisionId
              )
              OR EXISTS
              (
                  SELECT 1 FROM dbo.PeopleInstitutions pi
                  CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split
                  WHERE pi.UserId = @UserId
                    AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = i.Id
              )
          )
    )
    BEGIN
        SELECT 0;
        RETURN;
    END;

    ;WITH Selected AS
    (
        SELECT DISTINCT
            selected.GradeId,
            LTRIM(RTRIM(selected.Section)) Section
        FROM OPENJSON(@GradeSections)
        WITH (GradeId INT '$.GradeId', Section VARCHAR(100) '$.Section') selected
        WHERE selected.GradeId > 0
          AND NULLIF(LTRIM(RTRIM(selected.Section)), '') IS NOT NULL
    )
    SELECT COUNT(*)
    FROM dbo.Students s
    WHERE s.InstitutionId = @InstitutionId
      AND s.IsDeleted = 0
      AND s.CurrentStatus = 1
      AND EXISTS
      (
          SELECT 1 FROM Selected selected
          WHERE selected.GradeId = s.GradeId
            AND LOWER(selected.Section) = LOWER(LTRIM(RTRIM(s.Section)))
      );
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetThemeActivities
    @UserId INT,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @InstitutionId INT = NULL,
    @ThemeId INT = NULL,
    @GradeId INT = NULL,
    @Section VARCHAR(100) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @PageNumber = CASE WHEN @PageNumber < 1 THEN 1 ELSE @PageNumber END;
    SET @PageSize = CASE WHEN @PageSize < 10 THEN 10 WHEN @PageSize > 100 THEN 100 ELSE @PageSize END;
    SET @Section = NULLIF(LTRIM(RTRIM(@Section)), '');
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @IsAdmin BIT = 0;
    DECLARE @HasExplicitScope BIT = 0;

    SELECT @IsAdmin = CASE WHEN LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin' THEN 1 ELSE 0 END
    FROM dbo.Users u
    INNER JOIN dbo.Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
    WHERE u.Id = @UserId AND u.IsDeleted = 0;

    IF EXISTS (SELECT 1 FROM dbo.PeopleDivisions WHERE UserId = @UserId)
       OR EXISTS (SELECT 1 FROM dbo.PeopleInstitutions WHERE UserId = @UserId)
        SET @HasExplicitScope = 1;

    CREATE TABLE #AllowedInstitutions
    (
        InstitutionId INT NOT NULL PRIMARY KEY
    );

    IF @IsAdmin = 0 AND @HasExplicitScope = 1
    BEGIN
        INSERT INTO #AllowedInstitutions (InstitutionId)
        SELECT DISTINCT i.Id
        FROM dbo.Institutions i
        INNER JOIN dbo.PeopleDivisions pd
            ON pd.DivisionId = i.DivisionId AND pd.UserId = @UserId
        WHERE i.IsDeleted = 0;

        INSERT INTO #AllowedInstitutions (InstitutionId)
        SELECT DISTINCT parsed.InstitutionId
        FROM dbo.PeopleInstitutions pi
        CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split
        CROSS APPLY (VALUES (TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT))) parsed(InstitutionId)
        WHERE pi.UserId = @UserId
          AND parsed.InstitutionId IS NOT NULL
          AND NOT EXISTS
          (
              SELECT 1
              FROM #AllowedInstitutions allowed
              WHERE allowed.InstitutionId = parsed.InstitutionId
          );
    END;

    ;WITH Filtered AS
    (
        SELECT
            ta.Id,
            ta.ThemeActivityDate ActivityDate,
            ta.InstitutionId,
            i.InstitutionName,
            ta.ThemeId,
            t.ThemeName,
            ta.TotalStudents,
            ta.StudentAttended StudentsAttended,
            ta.DidChildrenDayHappen DidChildrensDayHappen,
            ta.TotalParentsAttended ParentsAttended,
            ta.DateEntryPoint,
            ta.CreatedBy,
            ta.DateCreated
        FROM dbo.ThemeActivities ta
        INNER JOIN dbo.Institutions i ON i.Id = ta.InstitutionId AND i.IsDeleted = 0
        INNER JOIN dbo.Themes t ON t.Id = ta.ThemeId AND t.IsDeleted = 0
        WHERE ta.IsDeleted = 0
          AND
          (
              @IsAdmin = 1
              OR EXISTS
              (
                  SELECT 1
                  FROM #AllowedInstitutions allowed
                  WHERE allowed.InstitutionId = ta.InstitutionId
              )
              OR (@HasExplicitScope = 0 AND ta.CreatedBy = @UserId)
          )
          AND (@InstitutionId IS NULL OR ta.InstitutionId = @InstitutionId)
          AND (@ThemeId IS NULL OR ta.ThemeId = @ThemeId)
          AND
          (
              @GradeId IS NULL
              OR EXISTS
                 (
                     SELECT 1 FROM dbo.ThemeActivityGradeSections tags
                     WHERE tags.ThemeActivityId = ta.Id AND tags.GradeId = @GradeId
                 )
          )
          AND
          (
              @Section IS NULL
              OR EXISTS
                 (
                     SELECT 1 FROM dbo.ThemeActivityGradeSections tags
                     CROSS APPLY dbo.SplitString(tags.Section, ',') split
                     WHERE tags.ThemeActivityId = ta.Id
                       AND LOWER(LTRIM(RTRIM(split.Item))) = LOWER(@Section)
                 )
          )
          AND (@FromDate IS NULL OR ta.ThemeActivityDate >= @FromDate)
          AND (@ToDate IS NULL OR ta.ThemeActivityDate < DATEADD(DAY, 1, @ToDate))
    ),
    Numbered AS
    (
        SELECT
            ROW_NUMBER() OVER (ORDER BY ActivityDate DESC, Id DESC) RowNumber,
            COUNT(*) OVER () TotalCount,
            *
        FROM Filtered
    )
    SELECT
        *
    INTO #Page
    FROM Numbered
    WHERE RowNumber > @Offset
      AND RowNumber <= @Offset + @PageSize
    OPTION (RECOMPILE);

    SELECT
        page.RowNumber,
        page.TotalCount,
        page.Id,
        page.ActivityDate,
        page.InstitutionId,
        page.InstitutionName,
        page.ThemeId,
        page.ThemeName,
        page.TotalStudents,
        page.StudentsAttended,
        page.DidChildrensDayHappen,
        page.ParentsAttended,
        ISNULL(summary.GradeSectionsText, '') GradeSectionsText,
        page.DateEntryPoint,
        page.CreatedBy,
        page.DateCreated
    FROM #Page page
    OUTER APPLY
    (
        SELECT STRING_AGG(CONVERT(NVARCHAR(MAX), pairs.GradeSection), ', ')
               WITHIN GROUP (ORDER BY pairs.GradeSection) GradeSectionsText
        FROM
        (
            SELECT DISTINCT g.GradeName + ' - ' + LTRIM(RTRIM(tags.Section)) GradeSection
            FROM dbo.ThemeActivityGradeSections tags
            INNER JOIN dbo.Grades g ON g.Id = tags.GradeId
            WHERE tags.ThemeActivityId = page.Id
        ) pairs
    ) summary
    ORDER BY page.RowNumber;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetThemeActivity
    @Id INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CanRead BIT = 0;
    SELECT @CanRead = 1
    FROM dbo.ThemeActivities ta
    INNER JOIN dbo.Institutions i ON i.Id = ta.InstitutionId AND i.IsDeleted = 0
    WHERE ta.Id = @Id AND ta.IsDeleted = 0
      AND
      (
          EXISTS
          (
              SELECT 1 FROM dbo.Users u
              INNER JOIN dbo.Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
              WHERE u.Id = @UserId AND u.IsDeleted = 0
                AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
          )
          OR EXISTS
          (
              SELECT 1 FROM dbo.PeopleDivisions pd
              WHERE pd.UserId = @UserId AND pd.DivisionId = i.DivisionId
          )
          OR EXISTS
          (
              SELECT 1 FROM dbo.PeopleInstitutions pi
              CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split
              WHERE pi.UserId = @UserId
                AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = ta.InstitutionId
          )
      );

    SELECT
        ta.Id,
        ta.ThemeActivityDate ActivityDate,
        ta.InstitutionId,
        i.InstitutionName,
        ta.ThemeId,
        t.ThemeName,
        ta.TotalStudents,
        ta.StudentAttended StudentsAttended,
        ta.DidChildrenDayHappen DidChildrensDayHappen,
        ta.TotalParentsAttended ParentsAttended,
        ta.DateEntryPoint,
        ta.CreatedBy,
        ta.DateCreated
    FROM dbo.ThemeActivities ta
    INNER JOIN dbo.Institutions i ON i.Id = ta.InstitutionId
    INNER JOIN dbo.Themes t ON t.Id = ta.ThemeId
    WHERE @CanRead = 1 AND ta.Id = @Id;

    SELECT
        tags.GradeId,
        g.GradeName,
        LTRIM(RTRIM(tags.Section)) Section
    FROM dbo.ThemeActivityGradeSections tags
    INNER JOIN dbo.Grades g ON g.Id = tags.GradeId
    WHERE @CanRead = 1 AND tags.ThemeActivityId = @Id
    ORDER BY g.Id, tags.Section;
END;
GO
