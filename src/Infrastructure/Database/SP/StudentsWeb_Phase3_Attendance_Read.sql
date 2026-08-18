-- Phase 3 web attendance reads only. Web writes are performed transactionally through EF Core.
CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetAttendanceRoster
    @UserId INT,
    @InstitutionId INT,
    @GradeId INT,
    @Section VARCHAR(25),
    @AttendanceDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SET @Section = NULLIF(LTRIM(RTRIM(@Section)), '');

    IF @UserId <= 0 OR @InstitutionId <= 0 OR @GradeId <= 0
       OR @Section IS NULL OR @AttendanceDate IS NULL
       OR @AttendanceDate > CONVERT(DATE, GETDATE())
       OR NOT EXISTS
       (
           SELECT 1
           FROM dbo.Institutions i
           WHERE i.Id = @InstitutionId
             AND i.IsDeleted = 0
             AND i.CurrentStatus = 1
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
                       AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = i.Id
                 )
             )
       )
       OR NOT EXISTS
       (
           SELECT 1
           FROM dbo.InstitutionGradeSections igs
           CROSS APPLY dbo.SplitString(igs.Sections, ',') split
           WHERE igs.InstitutionId = @InstitutionId
             AND igs.GradeId = @GradeId
             AND LTRIM(RTRIM(split.Item)) = @Section
       )
        RETURN;

    SELECT
        s.Id StudentId,
        ISNULL(s.StudentId, '') StudentCode,
        LTRIM(RTRIM(CONCAT(s.FirstName, ' ', s.LastName))) StudentName,
        ISNULL(f.FatherName, '') FatherName,
        s.EnrollmentDate,
        sa.AttendanceStatus,
        NULLIF(LTRIM(RTRIM(sa.AttendanceNote)), '') AttendanceNote,
        sa.DateEntryPoint
    FROM dbo.Students s
    OUTER APPLY
    (
        SELECT TOP (1) sf.FatherName
        FROM dbo.StudentFamilyDetails sf
        WHERE sf.StudentId = s.Id AND sf.IsDeleted = 0
        ORDER BY sf.Id
    ) f
    LEFT JOIN dbo.StudentAttendances sa
        ON sa.StudentId = s.Id
       AND sa.AttendanceDate = @AttendanceDate
    WHERE s.IsDeleted = 0
      AND s.CurrentStatus = 1
      AND s.InstitutionId = @InstitutionId
      AND s.GradeId = @GradeId
      AND LTRIM(RTRIM(s.Section)) = @Section
      AND CONVERT(DATE, s.EnrollmentDate) <= @AttendanceDate
    ORDER BY s.FirstName, s.LastName, s.Id;
END;
GO
