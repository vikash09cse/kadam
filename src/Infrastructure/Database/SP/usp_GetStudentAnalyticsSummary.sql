CREATE OR ALTER PROCEDURE dbo.usp_GetStudentAnalyticsSummary
    @UserId INT,
    @StateId INT = NULL,
    @DivisionIds VARCHAR(2000) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @IncludeAll BIT = 1,
    @IncludeKadam BIT = 0,
    @IncludeKadamPlus BIT = 0
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
            SELECT @InstitutionIds = STRING_AGG(CAST(LTRIM(RTRIM(s.Item)) AS VARCHAR(MAX)), ',')
            FROM dbo.PeopleInstitutions pi
            CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') s
            WHERE pi.UserId = @UserId
              AND LTRIM(RTRIM(ISNULL(pi.InstitutionIds, ''))) <> ''
              AND TRY_CAST(LTRIM(RTRIM(s.Item)) AS INT) IS NOT NULL;
        END
    END

    ;WITH AssessmentState AS (
        SELECT
            StudentId,
            MAX(CASE WHEN BaselineType = 'baselinepreAssessment' AND CompletedDate IS NOT NULL THEN 1 ELSE 0 END) AS IsBaselineCompleted,
            MAX(CASE WHEN BaselineType = 'endlinepreAssessment' AND CompletedDate IS NOT NULL THEN 1 ELSE 0 END) AS IsEndlineCompleted
        FROM dbo.StudentBaselineDetails
        WHERE ISNULL(IsDeleted, 0) = 0
        GROUP BY StudentId
    ),
    ProgressState AS (
        SELECT StudentId, MAX(StepId) AS LastProgressStepId
        FROM dbo.StudentProgressSteps
        WHERE IsCompleted = 1
        GROUP BY StudentId
    ),
    GradeTestState AS (
        SELECT
            StudentId,
            MAX(CASE WHEN GradeLevelId = 1 THEN 1 ELSE 0 END) AS HasGradeTest1,
            MAX(CASE WHEN GradeLevelId = 2 THEN 1 ELSE 0 END) AS HasGradeTest2,
            MAX(CASE WHEN GradeLevelId = 3 THEN 1 ELSE 0 END) AS HasGradeTest3,
            MAX(CASE WHEN GradeLevelId = 4 THEN 1 ELSE 0 END) AS HasGradeTest4,
            MAX(CASE WHEN GradeLevelId = 5 THEN 1 ELSE 0 END) AS HasGradeTest5
        FROM dbo.StudentGradeTestDetails
        WHERE ISNULL(IsDeleted, 0) = 0
        GROUP BY StudentId
    ),
    MainstreamState AS (
        SELECT DISTINCT StudentId
        FROM dbo.StudentMainstreams
    ),
    StudentFlags AS (
        SELECT
            s.Id,
            s.CurrentStatus,
            CAST(ISNULL(a.IsBaselineCompleted, 0) AS BIT) AS IsBaselineCompleted,
            CAST(CASE WHEN p.LastProgressStepId IS NULL THEN 0 ELSE 1 END AS BIT) AS HasProgress,
            CAST(CASE WHEN level.ExitStepId IS NOT NULL AND p.LastProgressStepId >= level.ExitStepId THEN 1 ELSE 0 END AS BIT) AS AllStepsCompleted,
            CAST(ISNULL(a.IsEndlineCompleted, 0) AS BIT) AS IsEndlineCompleted,
            CAST(CASE WHEN mainstream.StudentId IS NULL THEN 0 ELSE 1 END AS BIT) AS HasMainstream,
            CAST(CASE WHEN s.IsKadamPlusStudent = 0 AND s.CurrentStatus IN (1, 2)
                           AND ISNULL(a.IsBaselineCompleted, 0) = 1
                           AND ISNULL(a.IsEndlineCompleted, 0) = 1
                           AND mainstream.StudentId IS NULL
                      THEN 1 ELSE 0 END AS BIT) AS IsMainstreamEligible,
            ISNULL(gt.HasGradeTest1, 0) AS HasGradeTest1,
            ISNULL(gt.HasGradeTest2, 0) AS HasGradeTest2,
            ISNULL(gt.HasGradeTest3, 0) AS HasGradeTest3,
            ISNULL(gt.HasGradeTest4, 0) AS HasGradeTest4,
            ISNULL(gt.HasGradeTest5, 0) AS HasGradeTest5
        FROM dbo.Students s
        INNER JOIN dbo.Institutions i ON s.InstitutionId = i.Id AND i.IsDeleted = 0
        LEFT JOIN AssessmentState a ON a.StudentId = s.Id
        LEFT JOIN ProgressState p ON p.StudentId = s.Id
        LEFT JOIN dbo.StudentGradeStartAndEndDetails level ON level.StudentId = s.Id
        LEFT JOIN GradeTestState gt ON gt.StudentId = s.Id
        LEFT JOIN MainstreamState mainstream ON mainstream.StudentId = s.Id
        WHERE s.IsDeleted = 0
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
                      SELECT TRY_CAST(Item AS INT)
                      FROM dbo.SplitString(@InstitutionIds, ',')
                      WHERE TRY_CAST(Item AS INT) IS NOT NULL
                  )
              )
          )
          AND (@StateId IS NULL OR @StateId = 0 OR i.StateId = @StateId)
          AND (
              @DivisionIds IS NULL
              OR LTRIM(RTRIM(@DivisionIds)) = ''
              OR i.DivisionId IN (
                  SELECT TRY_CAST(LTRIM(RTRIM(Item)) AS INT)
                  FROM dbo.SplitString(@DivisionIds, ',')
                  WHERE TRY_CAST(LTRIM(RTRIM(Item)) AS INT) IS NOT NULL
              )
          )
          AND (@FromDate IS NULL OR s.EnrollmentDate >= @FromDate)
          AND (@ToDate IS NULL OR s.EnrollmentDate <= @ToDate)
          AND (
              @IncludeAll = 1
              OR (
                  (@IncludeKadam = 1 AND s.IsKadamPlusStudent = 0)
                  OR (@IncludeKadamPlus = 1 AND s.IsKadamPlusStudent = 1)
              )
          )
    )
    SELECT
        COUNT(*) AS TotalStudents,
        ISNULL(SUM(CASE WHEN CurrentStatus = 1 THEN 1 ELSE 0 END), 0) AS ActiveCount,
        ISNULL(SUM(CASE WHEN CurrentStatus = 2 THEN 1 ELSE 0 END), 0) AS InactiveCount,
        ISNULL(SUM(CASE WHEN CurrentStatus = 3 THEN 1 ELSE 0 END), 0) AS CompletedCount,
        ISNULL(SUM(CASE WHEN IsBaselineCompleted = 1 THEN 1 ELSE 0 END), 0) AS BaselineCompletedCount,
        CAST(CASE WHEN COUNT(*) = 0 THEN 0 ELSE ROUND(100.0 * SUM(CASE WHEN IsBaselineCompleted = 1 THEN 1 ELSE 0 END) / COUNT(*), 2) END AS DECIMAL(5, 2)) AS BaselineCompletedPercent,
        ISNULL(SUM(CASE WHEN HasProgress = 1 THEN 1 ELSE 0 END), 0) AS ProgressStartedCount,
        CAST(CASE WHEN COUNT(*) = 0 THEN 0 ELSE ROUND(100.0 * SUM(CASE WHEN HasProgress = 1 THEN 1 ELSE 0 END) / COUNT(*), 2) END AS DECIMAL(5, 2)) AS ProgressStartedPercent,
        ISNULL(SUM(CASE WHEN AllStepsCompleted = 1 THEN 1 ELSE 0 END), 0) AS StepsCompletedCount,
        CAST(CASE WHEN COUNT(*) = 0 THEN 0 ELSE ROUND(100.0 * SUM(CASE WHEN AllStepsCompleted = 1 THEN 1 ELSE 0 END) / COUNT(*), 2) END AS DECIMAL(5, 2)) AS StepsCompletedPercent,
        ISNULL(SUM(CASE WHEN IsEndlineCompleted = 1 THEN 1 ELSE 0 END), 0) AS EndlineCompletedCount,
        CAST(CASE WHEN COUNT(*) = 0 THEN 0 ELSE ROUND(100.0 * SUM(CASE WHEN IsEndlineCompleted = 1 THEN 1 ELSE 0 END) / COUNT(*), 2) END AS DECIMAL(5, 2)) AS EndlineCompletedPercent,
        ISNULL(SUM(CASE WHEN HasMainstream = 1 THEN 1 ELSE 0 END), 0) AS MainstreamedCount,
        CAST(CASE WHEN COUNT(*) = 0 THEN 0 ELSE ROUND(100.0 * SUM(CASE WHEN HasMainstream = 1 THEN 1 ELSE 0 END) / COUNT(*), 2) END AS DECIMAL(5, 2)) AS MainstreamedPercent,
        ISNULL(SUM(CASE WHEN IsMainstreamEligible = 1 THEN 1 ELSE 0 END), 0) AS MainstreamEligibleCount,
        ISNULL(SUM(CASE WHEN HasGradeTest1 = 1 THEN 1 ELSE 0 END), 0) AS GradeTest1Count,
        ISNULL(SUM(CASE WHEN HasGradeTest2 = 1 THEN 1 ELSE 0 END), 0) AS GradeTest2Count,
        ISNULL(SUM(CASE WHEN HasGradeTest3 = 1 THEN 1 ELSE 0 END), 0) AS GradeTest3Count,
        ISNULL(SUM(CASE WHEN HasGradeTest4 = 1 THEN 1 ELSE 0 END), 0) AS GradeTest4Count,
        ISNULL(SUM(CASE WHEN HasGradeTest5 = 1 THEN 1 ELSE 0 END), 0) AS GradeTest5Count,
        ISNULL(SUM(CASE WHEN CurrentStatus = 1 AND IsBaselineCompleted = 1 AND AllStepsCompleted = 0 THEN 1 ELSE 0 END), 0) AS InProgressCount,
        ISNULL(SUM(CASE WHEN CurrentStatus = 1 AND IsBaselineCompleted = 0 THEN 1 ELSE 0 END), 0) AS NoBaselineActiveCount
    FROM StudentFlags;
END
GO
