-- Phase 3 web follow-up reads only. Web writes are performed through EF Core.
CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetFollowups
    @UserId INT,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @InstitutionId INT = NULL,
    @GradeId INT = NULL,
    @Section VARCHAR(25) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @PageNumber = CASE WHEN @PageNumber < 1 THEN 1 ELSE @PageNumber END;
    SET @PageSize = CASE WHEN @PageSize < 10 THEN 10 WHEN @PageSize > 100 THEN 100 ELSE @PageSize END;
    SET @Section = NULLIF(LTRIM(RTRIM(@Section)), '');

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    ;WITH Authorized AS
    (
        SELECT
            f.Id,
            f.FollowupDate VisitDate,
            f.InstitutionId,
            i.InstitutionName,
            f.GradeId,
            ISNULL(g.GradeName, '') GradeName,
            ISNULL(f.Section, '') Section,
            ISNULL(f.InchargeName, '') TeacherName,
            ISNULL(f.InchargeContactNumber, '') TeacherContact,
            ISNULL(f.MaleStudentCount, 0) MaleStudentCount,
            ISNULL(f.FemaleStudentCount, 0) FemaleStudentCount,
            ISNULL(f.TotalStudentCount, 0) TotalStudentCount,
            ISNULL(f.TodayStudentPresentCount, 0) PresentTodayCount,
            ISNULL(f.TotalStudentPercentage, 0) TodayAttendancePercentage,
            f.LastMonthWorkingDayCount LastMonthWorkingDays,
            f.LastMonthAttendanceCount LastMonthAttendance,
            f.LastMonthAttendancePercentage,
            NULLIF(LTRIM(RTRIM(f.IsChildSitTogether)), '') ChildrenSitTogether,
            f.DateEntryPoint,
            f.CreatedBy,
            f.DateCreated
        FROM dbo.StudentFollowups f
        INNER JOIN dbo.Institutions i ON i.Id = f.InstitutionId AND i.IsDeleted = 0
        LEFT JOIN dbo.Grades g ON g.Id = f.GradeId AND g.IsDeleted = 0
        WHERE f.IsDeleted = 0
          AND
          (
              EXISTS
              (
                  SELECT 1
                  FROM dbo.Users u
                  INNER JOIN dbo.Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
                  WHERE u.Id = @UserId
                    AND u.IsDeleted = 0
                    AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
              )
              OR EXISTS
              (
                  SELECT 1
                  FROM dbo.PeopleDivisions pd
                  WHERE pd.UserId = @UserId AND pd.DivisionId = i.DivisionId
              )
              OR EXISTS
              (
                  SELECT 1
                  FROM dbo.PeopleInstitutions pi
                  CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split
                  WHERE pi.UserId = @UserId
                    AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = f.InstitutionId
              )
          )
          AND (@InstitutionId IS NULL OR f.InstitutionId = @InstitutionId)
          AND (@GradeId IS NULL OR f.GradeId = @GradeId)
          AND (@Section IS NULL OR LTRIM(RTRIM(f.Section)) = @Section)
          AND (@FromDate IS NULL OR f.FollowupDate >= @FromDate)
          AND (@ToDate IS NULL OR f.FollowupDate < DATEADD(DAY, 1, @ToDate))
    )
    SELECT
        ROW_NUMBER() OVER (ORDER BY VisitDate DESC, Id DESC) RowNumber,
        COUNT(*) OVER () TotalCount,
        *
    FROM Authorized
    ORDER BY VisitDate DESC, Id DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetFollowup
    @Id INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        f.Id,
        f.FollowupDate VisitDate,
        f.InstitutionId,
        i.InstitutionName,
        f.GradeId,
        ISNULL(g.GradeName, '') GradeName,
        ISNULL(f.Section, '') Section,
        ISNULL(f.InchargeName, '') TeacherName,
        ISNULL(f.InchargeContactNumber, '') TeacherContact,
        ISNULL(f.MaleStudentCount, 0) MaleStudentCount,
        ISNULL(f.FemaleStudentCount, 0) FemaleStudentCount,
        ISNULL(f.TotalStudentCount, 0) TotalStudentCount,
        ISNULL(f.TodayStudentPresentCount, 0) PresentTodayCount,
        ISNULL(f.TotalStudentPercentage, 0) TodayAttendancePercentage,
        f.LastMonthWorkingDayCount LastMonthWorkingDays,
        f.LastMonthAttendanceCount LastMonthAttendance,
        f.LastMonthAttendancePercentage,
        NULLIF(LTRIM(RTRIM(f.IsChildSitTogether)), '') ChildrenSitTogether,
        f.DateEntryPoint,
        f.CreatedBy,
        f.DateCreated
    FROM dbo.StudentFollowups f
    INNER JOIN dbo.Institutions i ON i.Id = f.InstitutionId AND i.IsDeleted = 0
    LEFT JOIN dbo.Grades g ON g.Id = f.GradeId AND g.IsDeleted = 0
    WHERE f.Id = @Id
      AND f.IsDeleted = 0
      AND
      (
          EXISTS
          (
              SELECT 1
              FROM dbo.Users u
              INNER JOIN dbo.Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
              WHERE u.Id = @UserId
                AND u.IsDeleted = 0
                AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
          )
          OR EXISTS
          (
              SELECT 1
              FROM dbo.PeopleDivisions pd
              WHERE pd.UserId = @UserId AND pd.DivisionId = i.DivisionId
          )
          OR EXISTS
          (
              SELECT 1
              FROM dbo.PeopleInstitutions pi
              CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split
              WHERE pi.UserId = @UserId
                AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = f.InstitutionId
          )
      );
END;
GO
