-- Student Attendance Summary Report
-- One row per student: Present / Absent / Holiday counts + attendance % for a date range
CREATE OR ALTER PROCEDURE [dbo].[usp_StudentAttendanceSummaryReport]
    @UserId        INT,
    @InstitutionId INT,
    @GradeId       INT = NULL,
    @Section       VARCHAR(55) = NULL,
    @FromDate      DATE,
    @ToDate        DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @InstitutionId IS NULL OR @InstitutionId <= 0
       OR @FromDate IS NULL
       OR @ToDate IS NULL
    BEGIN
        RETURN;
    END;

    DECLARE @IsAdmin BIT = 0;
    DECLARE @HasAccess BIT = 0;

    IF EXISTS (
        SELECT 1
        FROM Users u
        INNER JOIN Roles r ON u.RoleId = r.Id AND r.IsDeleted = 0
        WHERE u.Id = @UserId
          AND u.IsDeleted = 0
          AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
    )
        SET @IsAdmin = 1;

    IF @IsAdmin = 1
        SET @HasAccess = 1;
    ELSE
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM PeopleInstitutions pi
            CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') s
            WHERE pi.UserId = @UserId
              AND TRY_CAST(LTRIM(RTRIM(s.Item)) AS INT) = @InstitutionId
        )
            SET @HasAccess = 1;
    END;

    IF @HasAccess = 0
        RETURN;

    ;WITH AttendanceAgg AS
    (
        SELECT
            sa.StudentId,
            SUM(CASE WHEN sa.AttendanceStatus = 1 THEN 1 ELSE 0 END) AS PresentCount,
            SUM(CASE WHEN sa.AttendanceStatus = 2 THEN 1 ELSE 0 END) AS AbsentCount,
            SUM(CASE WHEN sa.AttendanceStatus = 3 THEN 1 ELSE 0 END) AS HolidayCount
        FROM dbo.StudentAttendances sa
        WHERE sa.AttendanceDate >= @FromDate
          AND sa.AttendanceDate <= @ToDate
        GROUP BY sa.StudentId
    )
    SELECT
        ISNULL(s.StudentId, '') AS StudentId,
        LTRIM(RTRIM(ISNULL(s.FirstName, '') + ' ' + ISNULL(s.LastName, ''))) AS StudentName,
        i.InstitutionName,
        ISNULL(g.GradeName, '') AS GradeName,
        ISNULL(s.Section, '') AS Section,
        ISNULL(a.PresentCount, 0) AS PresentCount,
        ISNULL(a.AbsentCount, 0) AS AbsentCount,
        ISNULL(a.HolidayCount, 0) AS HolidayCount,
        ISNULL(a.PresentCount, 0) + ISNULL(a.AbsentCount, 0) AS WorkingDays,
        CASE
            WHEN (ISNULL(a.PresentCount, 0) + ISNULL(a.AbsentCount, 0)) = 0 THEN CAST(0 AS DECIMAL(5, 2))
            ELSE CAST(
                ROUND(
                    (ISNULL(a.PresentCount, 0) * 100.0)
                    / (ISNULL(a.PresentCount, 0) + ISNULL(a.AbsentCount, 0))
                , 2) AS DECIMAL(5, 2)
            )
        END AS AttendancePercent
    FROM dbo.Students s
    INNER JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND ISNULL(i.IsDeleted, 0) = 0
    LEFT JOIN dbo.Grades g ON g.Id = s.GradeId AND ISNULL(g.IsDeleted, 0) = 0
    LEFT JOIN AttendanceAgg a ON a.StudentId = s.Id
    WHERE ISNULL(s.IsDeleted, 0) = 0
      AND s.InstitutionId = @InstitutionId
      AND (@GradeId IS NULL OR @GradeId <= 0 OR s.GradeId = @GradeId)
      AND (
            @Section IS NULL
            OR LTRIM(RTRIM(@Section)) = ''
            OR LTRIM(RTRIM(ISNULL(s.Section, ''))) = LTRIM(RTRIM(@Section))
          )
    ORDER BY s.FirstName, s.LastName, s.Id;
END;
GO
