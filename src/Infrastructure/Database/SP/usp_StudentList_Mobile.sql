ALTER     PROCEDURE [dbo].[usp_StudentList_Mobile]
    @CreatedBy INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        s.Id,
        CONCAT(FirstName, ' ', LastName) AS StudentName,
        Age,
        AadhaarCardNumber,
        s.StudentId,
        CONVERT(VARCHAR, EnrollmentDate, 103) AS EnrollmentDate,
		CurrentStatus,
		st.TrioId
		,-- Is Baseline Added
       Case When (SELECT COUNT(1) FROM StudentBaselineDetails WHERE StudentId = s.Id AND BaselineType = 'baselinepreAssessment') > 0 Then 1 Else 0 End AS IsBaselineAdded
    FROM 
        Students s
    LEFT JOIN StudentTrios st ON s.Id = st.StudentId
    WHERE 
        (@CreatedBy = 0 OR s.CreatedBy = @CreatedBy)
        AND s.IsDeleted = 0
    ORDER BY 
      s.DateCreated DESC,  FirstName, LastName;
END
