CREATE OR ALTER PROCEDURE usp_GetStudentBaselineDetailWithSubjects -- usp_GetStudentBaselineDetailWithSubjects 1
    @StudentId INT
AS
BEGIN
    SELECT 
        ROW_NUMBER() OVER (ORDER BY sb.SubjectName) AS RowNo,
        sbd.Id,
        sbd.StudentId,
        sb.Id As SubjectId,
        sbd.StudentAge,
        sbd.BaselineType,
        sbd.ObtainedMarks,
        sbd.PercentageMarks,
        ISNULL(sbd.TotalMarks, 50) AS TotalMarks,
        sbd.CurrentStatus,
        sb.SubjectName
    FROM  Subjects sb 
    LEFT JOIN StudentBaselineDetails sbd ON sbd.SubjectId = sb.Id AND sbd.StudentId = @StudentId
    WHERE  sb.CurrentStatus = 1 AND sb.IsDeleted = 0
    ORDER BY sb.SubjectName
END
