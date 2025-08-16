CREATE OR ALTER PROCEDURE [dbo].[usp_StudentFollowupList]
(
    @StudentId INT = NULL,
    @InstitutionId INT = NULL,
    @GradeId INT = NULL,
    @Section VARCHAR(25) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @CreatedBy INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        sf.Id,
        sf.StudentId,
        ISNULL(s.FirstName + ' ' + s.LastName, 'Unknown Student') AS StudentName,
        sf.FollowupDate,
        sf.InstitutionId,
        i.InstitutionName AS InstitutionName,
        sf.GradeId,
        g.GradeName AS GradeName,
        sf.Section,
        sf.InchargeName,
        sf.InchargeContactNumber,
        sf.IsChildSitTogether,
        sf.LastMonthAttendanceCount,
        sf.LastMonthWorkingDayCount,
        sf.LastMonthAttendancePercentage,
        sf.MaleStudentCount,
        sf.FemaleStudentCount,
        sf.TodayStudentPresentCount,
        sf.TotalStudentCount,
        sf.TotalStudentPercentage,
        sf.DateCreated,
        sf.CreatedBy
    FROM StudentFollowups sf
        LEFT JOIN Students s ON sf.StudentId = s.Id
        INNER JOIN Institutions i ON sf.InstitutionId = i.Id
        INNER JOIN Grades g ON sf.GradeId = g.Id
    WHERE sf.IsDeleted = 0
        AND (@StudentId IS NULL OR sf.StudentId = @StudentId)
        AND (@InstitutionId IS NULL OR sf.InstitutionId = @InstitutionId)
        AND (@GradeId IS NULL OR sf.GradeId = @GradeId)
        AND (@Section IS NULL OR sf.Section = @Section)
        AND (@FromDate IS NULL OR sf.FollowupDate >= @FromDate)
        AND (@ToDate IS NULL OR sf.FollowupDate <= @ToDate)
        AND (@CreatedBy = 0 OR sf.CreatedBy = @CreatedBy)
    ORDER BY sf.FollowupDate DESC;
END
