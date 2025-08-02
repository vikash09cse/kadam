CREATE OR Alter PROCEDURE usp_StudentList_MyInstitution_Mobile
    @InstitutionId INT = NULL,
    @GradeId INT = NULL,
    @Section Varchar(25) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @CurrentStatus INT = NULL,
    @CreatedBy INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ROW_NUMBER() OVER (ORDER BY s.EnrollmentDate DESC, s.FirstName, s.LastName) AS SrNo,
        s.StudentId,
        CONCAT(s.FirstName, ' ', s.LastName) AS StudentName,
        SF.FatherName,
        CONVERT(VARCHAR, s.EnrollmentDate, 103) AS EnrollmentDate,
        s.Age,
        g.GradeName,
        s.CurrentStatus,
        s.Id,
        CASE WHEN preBaseline.StudentId IS NOT NULL THEN 1 ELSE 0 END AS IsBaselineAdded,
        CASE WHEN postBaseline.StudentId IS NOT NULL THEN 1 ELSE 0 END AS IsEndBaselineAdded
    FROM 
        Students s
    LEFT JOIN StudentFamilyDetails SF ON S.Id = SF.StudentId
    LEFT JOIN Grades g ON s.GradeId = g.Id
    LEFT JOIN Institutions i ON s.InstitutionId = i.Id
    LEFT JOIN (
        SELECT DISTINCT StudentId 
        FROM StudentBaselineDetails 
        WHERE BaselineType = 'baselinepreAssessment'
    ) preBaseline ON s.Id = preBaseline.StudentId
    LEFT JOIN (
        SELECT DISTINCT StudentId 
        FROM StudentBaselineDetails 
        WHERE BaselineType = 'endlinepreAssessment'
    ) postBaseline ON s.Id = postBaseline.StudentId
    WHERE 
        s.IsDeleted = 0
        AND (@InstitutionId IS NULL OR s.InstitutionId = @InstitutionId)
        AND (@GradeId IS NULL OR s.GradeId = @GradeId)
        AND (@Section IS NULL OR s.Section = @Section)
        AND (@FromDate IS NULL OR s.EnrollmentDate >= @FromDate)
        AND (@ToDate IS NULL OR s.EnrollmentDate <= @ToDate)
        AND (@CurrentStatus IS NULL OR s.CurrentStatus = @CurrentStatus)
        AND (@CreatedBy = 0 OR s.CreatedBy = @CreatedBy)
    ORDER BY 
        s.EnrollmentDate DESC, s.FirstName, s.LastName;
END
GO 