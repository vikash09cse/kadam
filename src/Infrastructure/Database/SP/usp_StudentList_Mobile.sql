CREATE OR ALTER   PROCEDURE [dbo].[usp_StudentList_Mobile]
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
    FROM 
        Students s
    LEFT JOIN StudentTrios st ON s.Id = st.StudentId
    WHERE 
        (@CreatedBy = 0 OR s.CreatedBy = @CreatedBy)
        AND s.IsDeleted = 0
    ORDER BY 
      s.DateCreated DESC,  FirstName, LastName;
END
