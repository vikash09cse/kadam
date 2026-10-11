CREATE OR ALTER PROCEDURE dbo.usp_GetStudentExceptions
    @UserId INT,
    @StateId INT = NULL,
    @DivisionIds VARCHAR(2000) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @IncludeAll BIT = 1,
    @IncludeKadam BIT = 0,
    @IncludeKadamPlus BIT = 0,
    @ExceptionCode VARCHAR(50) = NULL,
    @DaysNoBaseline INT = 30,
    @DaysStuck INT = 14,
    @DaysInactive INT = 30,
    @Page INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @InstitutionIds VARCHAR(MAX) = NULL;
    DECLARE @FilterByInstitution BIT = 0;
    DECLARE @FilterByDivision BIT = 0;
    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
    DECLARE @Code VARCHAR(50) = NULLIF(LTRIM(RTRIM(@ExceptionCode)), '');

    IF @Page IS NULL OR @Page < 1 SET @Page = 1;
    IF @PageSize IS NULL OR @PageSize < 1 SET @PageSize = 50;
    IF @DaysNoBaseline IS NULL OR @DaysNoBaseline < 0 SET @DaysNoBaseline = 30;
    IF @DaysStuck IS NULL OR @DaysStuck < 0 SET @DaysStuck = 14;
    IF @DaysInactive IS NULL OR @DaysInactive < 0 SET @DaysInactive = 30;

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
            MAX(CASE WHEN BaselineType = 'baselinepreAssessment' THEN CompletedDate END) AS BaselineCompletedDate,
            MAX(CASE WHEN BaselineType = 'endlinepreAssessment' AND CompletedDate IS NOT NULL THEN 1 ELSE 0 END) AS IsEndlineCompleted,
            MAX(CASE WHEN BaselineType = 'endlinepreAssessment' THEN CompletedDate END) AS EndlineCompletedDate
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
    CompletedSteps AS (
        SELECT StudentId, StepId
        FROM dbo.StudentProgressSteps
        WHERE IsCompleted = 1
    ),
    MissingGradeTest AS (
        SELECT DISTINCT cs1.StudentId
        FROM CompletedSteps cs1
        INNER JOIN CompletedSteps cs2
            ON cs2.StudentId = cs1.StudentId
           AND cs2.StepId = cs1.StepId + 1
        CROSS APPLY (
            SELECT CASE WHEN cs1.StepId % 2 = 1 THEN (cs1.StepId + 1) / 2 ELSE NULL END AS GradeLevelId
        ) g
        WHERE g.GradeLevelId BETWEEN 1 AND 5
          AND NOT EXISTS (
              SELECT 1
              FROM dbo.StudentGradeTestDetails gt
              WHERE gt.StudentId = cs1.StudentId
                AND gt.GradeLevelId = g.GradeLevelId
                AND ISNULL(gt.IsDeleted, 0) = 0
          )
    ),
    MainstreamState AS (
        SELECT DISTINCT StudentId
        FROM dbo.StudentMainstreams
    ),
    BaseStudents AS (
        SELECT
            s.Id AS StudentPk,
            ISNULL(s.StudentId, '') AS StudentCode,
            LTRIM(RTRIM(CONCAT(ISNULL(s.FirstName, ''), ' ', ISNULL(s.LastName, '')))) AS StudentName,
            s.InstitutionId,
            i.InstitutionName,
            ISNULL(d.DivisionName, '') AS DivisionName,
            ISNULL(st.StateName, '') AS StateName,
            ISNULL(g.GradeName, '') AS GradeName,
            ISNULL(s.Section, '') AS Section,
            s.EnrollmentDate,
            s.CurrentStatus,
            CASE s.CurrentStatus WHEN 1 THEN 'Active' WHEN 2 THEN 'Inactive' WHEN 3 THEN 'Completed' ELSE '' END AS CurrentStatusText,
            CAST(ISNULL(s.IsKadamPlusStudent, 0) AS BIT) AS IsKadamPlusStudent,
            CAST(ISNULL(a.IsBaselineCompleted, 0) AS BIT) AS IsBaselineCompleted,
            a.BaselineCompletedDate,
            CAST(CASE WHEN p.LastProgressStepId IS NULL THEN 0 ELSE 1 END AS BIT) AS HasProgress,
            p.LastProgressStepId,
            level.ExitStepId,
            CAST(CASE WHEN level.ExitStepId IS NOT NULL AND p.LastProgressStepId >= level.ExitStepId THEN 1 ELSE 0 END AS BIT) AS AllStepsCompleted,
            CAST(ISNULL(a.IsEndlineCompleted, 0) AS BIT) AS IsEndlineCompleted,
            a.EndlineCompletedDate,
            CAST(CASE WHEN mainstream.StudentId IS NULL THEN 0 ELSE 1 END AS BIT) AS HasMainstream,
            CAST(CASE WHEN s.IsKadamPlusStudent = 0 AND s.CurrentStatus IN (1, 2)
                           AND ISNULL(a.IsBaselineCompleted, 0) = 1
                           AND ISNULL(a.IsEndlineCompleted, 0) = 1
                           AND mainstream.StudentId IS NULL
                      THEN 1 ELSE 0 END AS BIT) AS IsMainstreamEligible,
            CAST(CASE WHEN mgt.StudentId IS NULL THEN 0 ELSE 1 END AS BIT) AS HasMissingGradeTest,
            s.InActiveDate
        FROM dbo.Students s
        INNER JOIN dbo.Institutions i ON s.InstitutionId = i.Id AND i.IsDeleted = 0
        LEFT JOIN dbo.Divisions d ON d.Id = i.DivisionId AND d.IsDeleted = 0
        LEFT JOIN dbo.States st ON st.Id = i.StateId AND st.IsDeleted = 0
        LEFT JOIN dbo.Grades g ON g.Id = s.GradeId AND g.IsDeleted = 0
        LEFT JOIN AssessmentState a ON a.StudentId = s.Id
        LEFT JOIN ProgressState p ON p.StudentId = s.Id
        LEFT JOIN dbo.StudentGradeStartAndEndDetails level ON level.StudentId = s.Id
        LEFT JOIN MainstreamState mainstream ON mainstream.StudentId = s.Id
        LEFT JOIN MissingGradeTest mgt ON mgt.StudentId = s.Id
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
    ),
    Exceptions AS (
        SELECT
            b.*,
            v.ExceptionCode,
            v.ExceptionLabel,
            v.DaysOverdue
        FROM BaseStudents b
        CROSS APPLY (
            SELECT ExceptionCode, ExceptionLabel, DaysOverdue
            FROM (VALUES
                (
                    CASE WHEN b.CurrentStatus = 1
                              AND b.IsBaselineCompleted = 0
                              AND b.EnrollmentDate IS NOT NULL
                              AND b.EnrollmentDate <= DATEADD(DAY, -@DaysNoBaseline, @Today)
                         THEN 'NO_BASELINE' ELSE NULL END,
                    'No baseline',
                    CASE WHEN b.EnrollmentDate IS NULL THEN NULL
                         ELSE DATEDIFF(DAY, b.EnrollmentDate, @Today) - @DaysNoBaseline END
                ),
                (
                    CASE WHEN b.CurrentStatus = 1
                              AND b.IsBaselineCompleted = 1
                              AND b.HasProgress = 0
                              AND b.BaselineCompletedDate IS NOT NULL
                              AND CAST(b.BaselineCompletedDate AS DATE) <= DATEADD(DAY, -@DaysStuck, @Today)
                         THEN 'STUCK_AFTER_BASELINE' ELSE NULL END,
                    'Stuck after baseline',
                    CASE WHEN b.BaselineCompletedDate IS NULL THEN NULL
                         ELSE DATEDIFF(DAY, CAST(b.BaselineCompletedDate AS DATE), @Today) - @DaysStuck END
                ),
                (
                    CASE WHEN b.CurrentStatus = 1 AND b.HasMissingGradeTest = 1
                         THEN 'MISSING_GRADE_TEST' ELSE NULL END,
                    'Missing grade test',
                    CAST(NULL AS INT)
                ),
                (
                    CASE WHEN b.CurrentStatus = 1
                              AND b.AllStepsCompleted = 1
                              AND b.IsEndlineCompleted = 0
                         THEN 'READY_FOR_ENDLINE' ELSE NULL END,
                    'Ready for endline',
                    CAST(NULL AS INT)
                ),
                (
                    CASE WHEN b.IsMainstreamEligible = 1
                         THEN 'READY_FOR_MAINSTREAM' ELSE NULL END,
                    'Ready for mainstream',
                    CAST(NULL AS INT)
                ),
                (
                    CASE WHEN b.CurrentStatus = 2
                              AND b.InActiveDate IS NOT NULL
                              AND CAST(b.InActiveDate AS DATE) <= DATEADD(DAY, -@DaysInactive, @Today)
                         THEN 'LONG_INACTIVE' ELSE NULL END,
                    'Long inactive',
                    CASE WHEN b.InActiveDate IS NULL THEN NULL
                         ELSE DATEDIFF(DAY, CAST(b.InActiveDate AS DATE), @Today) - @DaysInactive END
                )
            ) AS x(ExceptionCode, ExceptionLabel, DaysOverdue)
            WHERE x.ExceptionCode IS NOT NULL
        ) v
        WHERE @Code IS NULL OR v.ExceptionCode = @Code
    ),
    Numbered AS (
        SELECT
            *,
            COUNT(*) OVER() AS TotalCount,
            ROW_NUMBER() OVER (ORDER BY ExceptionCode, EnrollmentDate DESC, StudentPk DESC) AS RowNum
        FROM Exceptions
    )
    SELECT
        StudentPk,
        StudentCode,
        StudentName,
        InstitutionId,
        InstitutionName,
        DivisionName,
        StateName,
        GradeName,
        Section,
        EnrollmentDate,
        CurrentStatus,
        CurrentStatusText,
        IsKadamPlusStudent,
        ExceptionCode,
        ExceptionLabel,
        LastProgressStepId,
        ExitStepId,
        BaselineCompletedDate,
        EndlineCompletedDate,
        InActiveDate,
        DaysOverdue,
        TotalCount
    FROM Numbered
    WHERE RowNum BETWEEN ((@Page - 1) * @PageSize) + 1 AND (@Page * @PageSize)
    ORDER BY RowNum;
END
GO
