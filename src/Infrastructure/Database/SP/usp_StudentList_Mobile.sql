CREATE OR ALTER PROCEDURE usp_StudentList_Mobile
    @CreatedBy INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id,
        CONCAT(FirstName, ' ', LastName) AS StudentName,
        Age,
        AadhaarCardNumber,
        StudentId,
        CONVERT(VARCHAR, EnrollmentDate, 103) AS EnrollmentDate
    FROM 
        Students
    WHERE 
        (@CreatedBy = 0 OR CreatedBy = @CreatedBy)
        AND IsDeleted = 0
    ORDER BY 
      EnrollmentDate DESC,  FirstName, LastName;
END
GO 