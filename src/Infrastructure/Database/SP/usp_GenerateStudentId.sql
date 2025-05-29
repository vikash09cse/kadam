CREATE OR ALTER PROCEDURE usp_GenerateStudentId
    @Id INT 
AS
BEGIN

    DECLARE @StudentId VARCHAR(55), @InstitutionId INT, @StateCode NVARCHAR(10), @DisctrictCode VARCHAR(10), @Year INT,
    @EnrollmentDate DATETIME, @NextYear INT

    

    SELECT @InstitutionId = InstitutionId, @EnrollmentDate = EnrollmentDate FROM Students WHERE Id = @Id;
    
    Select @StateCode = StateCode, @DisctrictCode = DistrictCode FROM Institutions
    JOIN States ON Institutions.StateId = States.Id
    JOIN Districts ON Institutions.DistrictId = Districts.Id
    WHERE Institutions.Id = @InstitutionId;

    SELECT @Year = YEAR(@EnrollmentDate);
    SELECT @NextYear = @Year + 1;

    
    SELECT @NextYear = RIGHT(@NextYear, 2); 

    SET @StudentId = 'KP/' + @StateCode + '/' + @DisctrictCode + '/' + CAST(@Year AS VARCHAR) + '-' + CAST(@NextYear AS VARCHAR) + '/' + CAST(@Id AS VARCHAR);

    UPDATE Students SET StudentId = @StudentId WHERE Id = @Id;

END;


-- Update Students Identity seed with 1000 
--DBCC CHECKIDENT ('Students', RESEED, 1000);
