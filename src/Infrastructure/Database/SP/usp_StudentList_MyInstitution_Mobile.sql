CREATE OR Alter PROCEDURE usp_StudentList_MyInstitution_Mobile
    @InstitutionId INT = NULL,
    @GradeId INT = NULL,
    @Section Varchar(25) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
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
        s.Id
    FROM 
        Students s
	LEFT Join StudentFamilyDetails SF On S.Id=SF.StudentId
    LEFT JOIN Grades g ON s.GradeId = g.Id
    LEFT JOIN Institutions i ON s.InstitutionId = i.Id
    WHERE 
        s.IsDeleted = 0
        AND (@InstitutionId IS NULL OR s.InstitutionId = @InstitutionId)
        AND (@GradeId IS NULL OR s.GradeId = @GradeId)
        AND (@Section IS NULL OR s.Section = @Section)
        AND (@FromDate IS NULL OR s.EnrollmentDate >= @FromDate)
        AND (@ToDate IS NULL OR s.EnrollmentDate <= @ToDate)
        AND (@CreatedBy = 0 OR s.CreatedBy = @CreatedBy)
    ORDER BY 
        s.EnrollmentDate DESC, s.FirstName, s.LastName;
END
GO 