CREATE OR ALTER PROCEDURE usp_GetStudentGradeTestDetailsWithSubjects -- usp_GetStudentBaselineDetailWithSubjects 1, 'baselinepreAssessment'
    @StudentId INT,
    @GradeLevelId INT
AS
BEGIN
    SELECT 
        ROW_NUMBER() OVER (ORDER BY sb.SubjectName) AS RowNo,
        sbd.Id,
        sbd.StudentId,
        sb.Id As SubjectId,
        sbd.StudentAge,
        sbd.ObtainedMarks,
        sbd.PercentageMarks,
        ISNULL(sbd.TotalMarks, 50) AS TotalMarks,
        sb.SubjectName,
		sbd.CompletedDate
    FROM  Subjects sb 
    LEFT JOIN StudentGradeTestDetails sbd 
		ON sbd.SubjectId = sb.Id AND sbd.StudentId = @StudentId 
		AND sbd.GradeLevelId = COALESCE(@GradeLevelId, sbd.GradeLevelId)
    
    WHERE  sb.CurrentStatus = 1 AND sb.IsDeleted = 0 
    ORDER BY sb.SubjectName
END