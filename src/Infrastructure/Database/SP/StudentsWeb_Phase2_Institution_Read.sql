CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetInstitutionStudents
    @UserId INT,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchText NVARCHAR(100) = NULL,
    @InstitutionId INT = NULL,
    @GradeId INT = NULL,
    @Section VARCHAR(25) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @Status INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET @PageNumber = CASE WHEN @PageNumber < 1 THEN 1 ELSE @PageNumber END;
    SET @PageSize = CASE WHEN @PageSize < 10 THEN 10 WHEN @PageSize > 100 THEN 100 ELSE @PageSize END;
    SET @SearchText = NULLIF(LTRIM(RTRIM(@SearchText)), '');
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
          AND NOT EXISTS (
              SELECT 1 FROM #AllowedInstitutions allowed
              WHERE allowed.InstitutionId = parsed.InstitutionId
          );
    END;

    ;WITH AssessmentState AS (
        SELECT
            StudentId,
            MAX(CASE WHEN BaselineType = 'baselinepreAssessment' THEN 1 ELSE 0 END) IsBaselineAdded,
            MAX(CASE WHEN BaselineType = 'baselinepreAssessment' AND CompletedDate IS NOT NULL THEN 1 ELSE 0 END) IsBaselineCompleted,
            MAX(CASE WHEN BaselineType = 'baselinepreAssessment' THEN CompletedDate END) BaselineCompletedDate,
            MAX(CASE WHEN BaselineType = 'endlinepreAssessment' THEN 1 ELSE 0 END) IsEndlineAdded,
            MAX(CASE WHEN BaselineType = 'endlinepreAssessment' AND CompletedDate IS NOT NULL THEN 1 ELSE 0 END) IsEndlineCompleted,
            MAX(CASE WHEN BaselineType = 'endlinepreAssessment' THEN CompletedDate END) EndlineCompletedDate
        FROM dbo.StudentBaselineDetails
        WHERE IsDeleted = 0
        GROUP BY StudentId
    ),
    ProgressState AS (
        SELECT StudentId, MAX(StepId) LastProgressStepId
        FROM dbo.StudentProgressSteps
        WHERE IsCompleted = 1
        GROUP BY StudentId
    ),
    Filtered AS (
        SELECT
            s.Id,
            s.StudentId,
            s.FirstName,
            s.LastName,
            ISNULL(f.FatherName, '') FatherName,
            s.InstitutionId,
            i.InstitutionName,
            s.GradeId,
            ISNULL(g.GradeName, '') GradeName,
            ISNULL(s.Section, '') Section,
            s.EnrollmentDate,
            s.Age,
            s.CurrentStatus,
            s.IsKadamPlusStudent,
            CAST(ISNULL(a.IsBaselineAdded, 0) AS BIT) IsBaselineAdded,
            CAST(ISNULL(a.IsBaselineCompleted, 0) AS BIT) IsBaselineCompleted,
            a.BaselineCompletedDate,
            CAST(CASE WHEN p.LastProgressStepId IS NULL THEN 0 ELSE 1 END AS BIT) HasProgress,
            p.LastProgressStepId,
            level.ExitStepId,
            CAST(CASE WHEN level.ExitStepId IS NOT NULL AND p.LastProgressStepId >= level.ExitStepId THEN 1 ELSE 0 END AS BIT) AllStepsCompleted,
            CAST(ISNULL(a.IsEndlineAdded, 0) AS BIT) IsEndlineAdded,
            CAST(ISNULL(a.IsEndlineCompleted, 0) AS BIT) IsEndlineCompleted,
            a.EndlineCompletedDate,
            CAST(CASE WHEN mainstream.StudentId IS NULL THEN 0 ELSE 1 END AS BIT) HasMainstream,
            CAST(CASE WHEN s.IsKadamPlusStudent = 0 AND s.CurrentStatus IN (1, 2)
                           AND ISNULL(a.IsBaselineCompleted, 0) = 1
                           AND ISNULL(a.IsEndlineCompleted, 0) = 1
                           AND mainstream.StudentId IS NULL
                      THEN 1 ELSE 0 END AS BIT) IsMainstreamEligible,
            CAST(CASE WHEN s.CurrentStatus <> 3 AND p.LastProgressStepId IS NULL THEN 1 ELSE 0 END AS BIT) CanEditBaseline,
            CAST(CASE WHEN s.CurrentStatus <> 3 AND ISNULL(a.IsBaselineCompleted, 0) = 1 THEN 1 ELSE 0 END AS BIT) CanOpenProgress,
            CAST(CASE WHEN s.CurrentStatus <> 3
                           AND ISNULL(a.IsBaselineCompleted, 0) = 1
                           AND ISNULL(a.IsEndlineAdded, 0) = 0
                      THEN 1 ELSE 0 END AS BIT) CanAddEndline
        FROM dbo.Students s
        INNER JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND i.IsDeleted = 0
        LEFT JOIN dbo.Grades g ON g.Id = s.GradeId AND g.IsDeleted = 0
        OUTER APPLY (
            SELECT TOP (1) family.FatherName
            FROM dbo.StudentFamilyDetails family
            WHERE family.StudentId = s.Id AND family.IsDeleted = 0
            ORDER BY family.Id
        ) f
        LEFT JOIN AssessmentState a ON a.StudentId = s.Id
        LEFT JOIN ProgressState p ON p.StudentId = s.Id
        LEFT JOIN dbo.StudentGradeStartAndEndDetails level ON level.StudentId = s.Id
        LEFT JOIN (
            SELECT DISTINCT StudentId FROM dbo.StudentMainstreams
        ) mainstream ON mainstream.StudentId = s.Id
        WHERE s.IsDeleted = 0
          AND (
              @IsAdmin = 1
              OR EXISTS (
                  SELECT 1 FROM #AllowedInstitutions allowed
                  WHERE allowed.InstitutionId = s.InstitutionId
              )
              OR (@HasExplicitScope = 0 AND s.CreatedBy = @UserId)
          )
          AND (@InstitutionId IS NULL OR s.InstitutionId = @InstitutionId)
          AND (@GradeId IS NULL OR s.GradeId = @GradeId)
          AND (@Section IS NULL OR LTRIM(RTRIM(s.Section)) = @Section)
          AND (@FromDate IS NULL OR s.EnrollmentDate >= @FromDate)
          AND (@ToDate IS NULL OR s.EnrollmentDate < DATEADD(DAY, 1, @ToDate))
          AND (@Status IS NULL OR s.CurrentStatus = @Status)
          AND (
              @SearchText IS NULL
              OR s.StudentId LIKE '%' + @SearchText + '%'
              OR s.FirstName LIKE '%' + @SearchText + '%'
              OR s.LastName LIKE '%' + @SearchText + '%'
              OR CONCAT(s.FirstName, ' ', s.LastName) LIKE '%' + @SearchText + '%'
              OR f.FatherName LIKE '%' + @SearchText + '%'
          )
    )
    SELECT
        ROW_NUMBER() OVER (ORDER BY EnrollmentDate DESC, Id DESC) RowNumber,
        Id,
        ISNULL(StudentId, '') StudentId,
        LTRIM(RTRIM(CONCAT(FirstName, ' ', LastName))) StudentName,
        FatherName,
        InstitutionId,
        InstitutionName,
        GradeId,
        GradeName,
        Section,
        EnrollmentDate,
        Age,
        CurrentStatus,
        IsKadamPlusStudent,
        IsBaselineAdded,
        IsBaselineCompleted,
        BaselineCompletedDate,
        HasProgress,
        LastProgressStepId,
        ExitStepId,
        AllStepsCompleted,
        IsEndlineAdded,
        IsEndlineCompleted,
        EndlineCompletedDate,
        HasMainstream,
        IsMainstreamEligible,
        CanEditBaseline,
        CanOpenProgress,
        CanAddEndline,
        COUNT(*) OVER() TotalCount
    FROM Filtered
    ORDER BY EnrollmentDate DESC, Id DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
    OPTION (RECOMPILE);
END;
GO
