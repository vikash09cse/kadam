CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetAssessment
    @StudentId INT,
    @UserId INT,
    @AssessmentType VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF @AssessmentType NOT IN ('baselinepreAssessment', 'endlinepreAssessment')
        RETURN;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Students s
        INNER JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND i.IsDeleted = 0
        WHERE s.Id = @StudentId AND s.IsDeleted = 0
          AND (
              EXISTS (
                  SELECT 1 FROM dbo.Users u
                  INNER JOIN dbo.Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
                  WHERE u.Id = @UserId AND u.IsDeleted = 0
                    AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
              )
              OR EXISTS (
                  SELECT 1 FROM dbo.PeopleDivisions pd
                  WHERE pd.UserId = @UserId AND pd.DivisionId = i.DivisionId
              )
              OR EXISTS (
                  SELECT 1 FROM dbo.PeopleInstitutions pi
                  CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split
                  WHERE pi.UserId = @UserId
                    AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = s.InstitutionId
              )
              OR (
                  NOT EXISTS (SELECT 1 FROM dbo.PeopleDivisions pd WHERE pd.UserId = @UserId)
                  AND NOT EXISTS (SELECT 1 FROM dbo.PeopleInstitutions pi WHERE pi.UserId = @UserId)
                  AND s.CreatedBy = @UserId
              )
          )
    )
    BEGIN
        SELECT TOP (0)
            CAST(0 AS INT) StudentId,
            CAST('' AS VARCHAR(50)) StudentCode,
            CAST('' AS NVARCHAR(200)) StudentName,
            CAST(NULL AS DATETIME) EnrollmentDate,
            CAST(0 AS INT) Age,
            CAST(0 AS INT) GradeId,
            CAST('' AS VARCHAR(100)) GradeName,
            CAST(0 AS BIT) IsKadamPlusStudent,
            CAST(NULL AS DATETIME) CompletedDate,
            CAST(NULL AS DATETIME) BaselineCompletedDate,
            CAST(0 AS BIT) HasProgress,
            CAST(0 AS BIT) IsLocked,
            CAST(0 AS BIT) CanChangeCompletedDate,
            CAST('' AS VARCHAR(200)) LockReason;
        SELECT TOP (0)
            CAST(0 AS INT) DetailId,
            CAST(0 AS INT) SubjectId,
            CAST('' AS VARCHAR(100)) SubjectName,
            CAST(0 AS INT) DisplayOrder,
            CAST(NULL AS DECIMAL(18,2)) ObtainedMarks,
            CAST(0 AS DECIMAL(18,2)) TotalMarks,
            CAST(NULL AS DECIMAL(18,2)) PercentageMarks,
            CAST(NULL AS DECIMAL(18,2)) BaselineObtainedMarks;
        RETURN;
    END;

    SELECT
        s.Id StudentId,
        ISNULL(s.StudentId, '') StudentCode,
        LTRIM(RTRIM(CONCAT(s.FirstName, ' ', s.LastName))) StudentName,
        s.EnrollmentDate,
        s.Age,
        s.GradeId,
        ISNULL(g.GradeName, '') GradeName,
        s.IsKadamPlusStudent,
        assessment.CompletedDate,
        baseline.CompletedDate BaselineCompletedDate,
        CAST(CASE WHEN EXISTS (
            SELECT 1 FROM dbo.StudentProgressSteps progress
            WHERE progress.StudentId = s.Id AND progress.IsCompleted = 1
        ) THEN 1 ELSE 0 END AS BIT) HasProgress,
        CAST(CASE
            WHEN s.CurrentStatus = 3 THEN 1
            WHEN @AssessmentType = 'endlinepreAssessment' AND baseline.CompletedDate IS NULL THEN 1
            WHEN @AssessmentType = 'baselinepreAssessment' AND EXISTS (
                SELECT 1 FROM dbo.StudentProgressSteps progress
                WHERE progress.StudentId = s.Id AND progress.IsCompleted = 1
            ) THEN 1
            WHEN @AssessmentType = 'endlinepreAssessment' AND assessment.CompletedDate IS NOT NULL THEN 1
            ELSE 0 END AS BIT) IsLocked,
        CAST(CASE
            WHEN s.CurrentStatus = 3 THEN 0
            WHEN @AssessmentType = 'baselinepreAssessment' AND EXISTS (
                SELECT 1 FROM dbo.StudentBaselineDetails d
                WHERE d.StudentId = s.Id AND d.IsDeleted = 0
                  AND d.BaselineType = 'baselinepreAssessment'
            ) THEN 1
            ELSE 0 END AS BIT) CanChangeCompletedDate,
        CASE
            WHEN s.CurrentStatus = 3 THEN 'Completed students are read-only.'
            WHEN @AssessmentType = 'endlinepreAssessment' AND baseline.CompletedDate IS NULL
                THEN 'A completed baseline is required before endline.'
            WHEN @AssessmentType = 'baselinepreAssessment' AND EXISTS (
                SELECT 1 FROM dbo.StudentProgressSteps progress
                WHERE progress.StudentId = s.Id AND progress.IsCompleted = 1
            ) THEN 'Baseline marks are locked because student progress has started. You can still update the baseline date.'
            WHEN @AssessmentType = 'endlinepreAssessment' AND assessment.CompletedDate IS NOT NULL
                THEN 'Endline has already been completed.'
            ELSE '' END LockReason
    FROM dbo.Students s
    LEFT JOIN dbo.Grades g ON g.Id = s.GradeId AND g.IsDeleted = 0
    OUTER APPLY (
        SELECT MAX(d.CompletedDate) CompletedDate
        FROM dbo.StudentBaselineDetails d
        WHERE d.StudentId = s.Id AND d.IsDeleted = 0
          AND d.BaselineType = @AssessmentType
    ) assessment
    OUTER APPLY (
        SELECT MAX(d.CompletedDate) CompletedDate
        FROM dbo.StudentBaselineDetails d
        WHERE d.StudentId = s.Id AND d.IsDeleted = 0
          AND d.BaselineType = 'baselinepreAssessment'
    ) baseline
    WHERE s.Id = @StudentId AND s.IsDeleted = 0;

    SELECT
        ISNULL(detail.Id, 0) DetailId,
        subject.Id SubjectId,
        subject.SubjectName,
        subject.DisplayOrder,
        detail.ObtainedMarks,
        CAST(CASE
            WHEN s.IsKadamPlusStudent = 1 THEN
                CASE
                    WHEN g.GradeName LIKE '1%' THEN 10
                    WHEN g.GradeName LIKE '2%' THEN 20
                    WHEN g.GradeName LIKE '3%' THEN 30
                    WHEN g.GradeName LIKE '4%' THEN 40
                    ELSE 50
                END
            ELSE
                CASE
                    WHEN s.Age <= 6 THEN 10
                    WHEN s.Age = 7 THEN 20
                    WHEN s.Age = 8 THEN 30
                    WHEN s.Age = 9 THEN 40
                    ELSE 50
                END
        END AS DECIMAL(18,2)) TotalMarks,
        detail.PercentageMarks,
        baseline.ObtainedMarks BaselineObtainedMarks
    FROM dbo.Students s
    CROSS JOIN dbo.Subjects subject
    LEFT JOIN dbo.Grades g ON g.Id = s.GradeId AND g.IsDeleted = 0
    LEFT JOIN dbo.StudentBaselineDetails detail
        ON detail.StudentId = s.Id
       AND detail.SubjectId = subject.Id
       AND detail.BaselineType = @AssessmentType
       AND detail.IsDeleted = 0
    LEFT JOIN dbo.StudentBaselineDetails baseline
        ON baseline.StudentId = s.Id
       AND baseline.SubjectId = subject.Id
       AND baseline.BaselineType = 'baselinepreAssessment'
       AND baseline.IsDeleted = 0
    WHERE s.Id = @StudentId
      AND s.IsDeleted = 0
      AND subject.IsDeleted = 0
      AND subject.CurrentStatus = 1
    ORDER BY subject.DisplayOrder, subject.Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetGradeTest
    @StudentId INT,
    @GradeLevelId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.Id StudentId,
        ISNULL(s.StudentId, '') StudentCode,
        LTRIM(RTRIM(CONCAT(s.FirstName, ' ', s.LastName))) StudentName,
        @GradeLevelId GradeLevelId,
        s.EnrollmentDate,
        test.CompletedDate,
        level.EntryStepId,
        level.ExitStepId,
        previousResult.PreviousGradePercentage
    FROM dbo.Students s
    INNER JOIN dbo.StudentGradeStartAndEndDetails level ON level.StudentId = s.Id
    OUTER APPLY (
        SELECT MAX(detail.CompletedDate) CompletedDate
        FROM dbo.StudentGradeTestDetails detail
        WHERE detail.StudentId = s.Id
          AND detail.GradeLevelId = @GradeLevelId
          AND ISNULL(detail.IsDeleted, 0) = 0
    ) test
    OUTER APPLY (
        SELECT CAST(ROUND(
            SUM(detail.ObtainedMarks) / NULLIF(SUM(detail.TotalMarks), 0) * 100, 0
        ) AS DECIMAL(18,2)) PreviousGradePercentage
        FROM dbo.StudentGradeTestDetails detail
        WHERE detail.StudentId = s.Id
          AND detail.GradeLevelId = @GradeLevelId - 1
          AND ISNULL(detail.IsDeleted, 0) = 0
    ) previousResult
    WHERE s.Id = @StudentId AND s.IsDeleted = 0;

    SELECT
        ISNULL(detail.Id, 0) DetailId,
        subject.Id SubjectId,
        subject.SubjectName,
        subject.DisplayOrder,
        detail.ObtainedMarks,
        CAST(CASE WHEN subject.GradeTestTotalMarks > 0
                  THEN subject.GradeTestTotalMarks ELSE 50 END AS DECIMAL(18,2)) TotalMarks,
        detail.PercentageMarks
    FROM dbo.Subjects subject
    LEFT JOIN dbo.StudentGradeTestDetails detail
        ON detail.StudentId = @StudentId
       AND detail.GradeLevelId = @GradeLevelId
       AND detail.SubjectId = subject.Id
       AND ISNULL(detail.IsDeleted, 0) = 0
    WHERE subject.IsDeleted = 0 AND subject.CurrentStatus = 1
    ORDER BY subject.DisplayOrder, subject.Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetProgress
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.Id StudentId,
        ISNULL(s.StudentId, '') StudentCode,
        LTRIM(RTRIM(CONCAT(s.FirstName, ' ', s.LastName))) StudentName,
        ISNULL(g.GradeName, '') GradeName,
        baseline.CompletedDate BaselineCompletedDate,
        level.GradeEntryLevelId,
        level.GradeExitLevelId,
        level.EntryStepId,
        level.ExitStepId,
        level.LastCompletedStepId
    FROM dbo.Students s
    INNER JOIN dbo.StudentGradeStartAndEndDetails level ON level.StudentId = s.Id
    LEFT JOIN dbo.Grades g ON g.Id = s.GradeId AND g.IsDeleted = 0
    OUTER APPLY (
        SELECT MAX(detail.CompletedDate) CompletedDate
        FROM dbo.StudentBaselineDetails detail
        WHERE detail.StudentId = s.Id
          AND detail.BaselineType = 'baselinepreAssessment'
          AND detail.IsDeleted = 0
    ) baseline
    WHERE s.Id = @StudentId AND s.IsDeleted = 0;

    SELECT
        step.Id StepId,
        step.StepName StepText,
        CAST(CASE WHEN progress.Id IS NULL THEN 0 ELSE progress.IsCompleted END AS BIT) IsCompleted,
        CAST(CASE WHEN level.EntryStepId IS NOT NULL
                       AND step.Id BETWEEN level.EntryStepId AND level.ExitStepId
                  THEN 1 ELSE 0 END AS BIT) IsInRange
    FROM dbo.Steps step
    LEFT JOIN dbo.StudentGradeStartAndEndDetails level ON level.StudentId = @StudentId
    LEFT JOIN dbo.StudentProgressSteps progress
        ON progress.StudentId = @StudentId
       AND progress.StepId = step.Id
       AND progress.IsCompleted = 1
    WHERE step.IsDeleted = 0 AND step.CurrentStatus = 1
    ORDER BY step.Id;

    SELECT DISTINCT GradeLevelId
    FROM dbo.StudentGradeTestDetails
    WHERE StudentId = @StudentId
      AND ISNULL(IsDeleted, 0) = 0
    ORDER BY GradeLevelId;
END;
GO
