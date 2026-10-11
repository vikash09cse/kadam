CREATE OR ALTER PROCEDURE dbo.usp_GetStudentAnalyticsBreakdown
    @UserId INT,
    @StateId INT = NULL,
    @DivisionIds VARCHAR(2000) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @IncludeAll BIT = 1,
    @IncludeKadam BIT = 0,
    @IncludeKadamPlus BIT = 0,
    @GroupBy VARCHAR(20) = 'DIVISION'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @InstitutionIds VARCHAR(MAX) = NULL;
    DECLARE @FilterByInstitution BIT = 0;
    DECLARE @FilterByDivision BIT = 0;
    DECLARE @GroupByNormalized VARCHAR(20) = UPPER(LTRIM(RTRIM(ISNULL(@GroupBy, 'DIVISION'))));

    IF @GroupByNormalized NOT IN ('DIVISION', 'INSTITUTION')
        SET @GroupByNormalized = 'DIVISION';

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
    MainstreamState AS (
        SELECT DISTINCT StudentId
        FROM dbo.StudentMainstreams
    ),
    StudentFlags AS (
        SELECT
            CASE WHEN @GroupByNormalized = 'INSTITUTION' THEN i.Id ELSE i.DivisionId END AS DimensionId,
            CASE WHEN @GroupByNormalized = 'INSTITUTION' THEN i.InstitutionName ELSE ISNULL(d.DivisionName, '') END AS DimensionName,
            ISNULL(st.StateName, '') AS StateName,
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
                      THEN 1 ELSE 0 END AS BIT) AS IsMainstreamEligible
        FROM dbo.Students s
        INNER JOIN dbo.Institutions i ON s.InstitutionId = i.Id AND i.IsDeleted = 0
        LEFT JOIN dbo.Divisions d ON d.Id = i.DivisionId AND d.IsDeleted = 0
        LEFT JOIN dbo.States st ON st.Id = i.StateId AND st.IsDeleted = 0
        LEFT JOIN AssessmentState a ON a.StudentId = s.Id
        LEFT JOIN ProgressState p ON p.StudentId = s.Id
        LEFT JOIN dbo.StudentGradeStartAndEndDetails level ON level.StudentId = s.Id
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
        ISNULL(DimensionId, 0) AS DimensionId,
        ISNULL(DimensionName, '') AS DimensionName,
        MAX(StateName) AS StateName,
        COUNT(*) AS TotalStudents,
        ISNULL(SUM(CASE WHEN CurrentStatus = 1 THEN 1 ELSE 0 END), 0) AS ActiveCount,
        ISNULL(SUM(CASE WHEN CurrentStatus = 2 THEN 1 ELSE 0 END), 0) AS InactiveCount,
        ISNULL(SUM(CASE WHEN CurrentStatus = 3 THEN 1 ELSE 0 END), 0) AS CompletedCount,
        ISNULL(SUM(CASE WHEN IsBaselineCompleted = 1 THEN 1 ELSE 0 END), 0) AS BaselineCompletedCount,
        ISNULL(SUM(CASE WHEN HasProgress = 1 THEN 1 ELSE 0 END), 0) AS ProgressStartedCount,
        ISNULL(SUM(CASE WHEN AllStepsCompleted = 1 THEN 1 ELSE 0 END), 0) AS StepsCompletedCount,
        ISNULL(SUM(CASE WHEN IsEndlineCompleted = 1 THEN 1 ELSE 0 END), 0) AS EndlineCompletedCount,
        ISNULL(SUM(CASE WHEN HasMainstream = 1 THEN 1 ELSE 0 END), 0) AS MainstreamedCount,
        ISNULL(SUM(CASE WHEN IsMainstreamEligible = 1 THEN 1 ELSE 0 END), 0) AS MainstreamEligibleCount,
        CAST(CASE WHEN COUNT(*) = 0 THEN 0 ELSE ROUND(100.0 * SUM(CASE WHEN IsBaselineCompleted = 1 THEN 1 ELSE 0 END) / COUNT(*), 2) END AS DECIMAL(5, 2)) AS BaselineCompletedPercent
    FROM StudentFlags
    GROUP BY DimensionId, DimensionName
    ORDER BY DimensionName, DimensionId;
END
GO
