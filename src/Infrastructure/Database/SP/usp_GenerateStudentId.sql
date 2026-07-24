CREATE OR ALTER PROCEDURE usp_GenerateStudentId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StudentId      VARCHAR(55);
    DECLARE @ExistingId     VARCHAR(55);
    DECLARE @InstitutionId  INT;
    DECLARE @StateCode      NVARCHAR(10);
    DECLARE @DistrictCode   VARCHAR(25);
    DECLARE @EnrollmentDate DATETIME;
    DECLARE @Year           INT;
    DECLARE @NextYearSuffix CHAR(2);

    SELECT
        @InstitutionId  = InstitutionId,
        @EnrollmentDate = EnrollmentDate,
        @ExistingId     = StudentId
    FROM Students
    WHERE Id = @Id
      AND ISNULL(IsDeleted, 0) = 0;

    -- Already has an ID — do not overwrite
    IF @ExistingId IS NOT NULL AND LTRIM(RTRIM(@ExistingId)) <> ''
        RETURN;

    IF @InstitutionId IS NULL OR @InstitutionId <= 0
        RETURN;

    IF @EnrollmentDate IS NULL OR @EnrollmentDate <= '1900-01-01'
        RETURN;

    SELECT
        @StateCode    = NULLIF(LTRIM(RTRIM(States.StateCode)), ''),
        @DistrictCode = NULLIF(LTRIM(RTRIM(Districts.DistrictCode)), '')
    FROM Institutions
    INNER JOIN States    ON Institutions.StateId = States.Id
    INNER JOIN Districts ON Institutions.DistrictId = Districts.Id
    WHERE Institutions.Id = @InstitutionId
      AND ISNULL(Institutions.IsDeleted, 0) = 0;

    -- Guard: NULL concat would wipe StudentId if we still updated
    -- IF @StateCode IS NULL OR @DistrictCode IS NULL
    --     RETURN;

    SET @Year = YEAR(@EnrollmentDate);
    SET @NextYearSuffix = RIGHT(CAST(@Year + 1 AS VARCHAR(4)), 2);

    SET @StudentId =
          'KP/'
        + ISNULL(@StateCode, '') + '/'
        + ISNULL(@DistrictCode, '') + '/'
        + CAST(@Year AS VARCHAR(4)) + '-'
        + @NextYearSuffix + '/'
        + CAST(@Id AS VARCHAR(20));

    UPDATE Students
    SET StudentId = @StudentId
    WHERE Id = @Id
      AND (StudentId IS NULL OR LTRIM(RTRIM(StudentId)) = '');
END;
GO

-- Update Students Identity seed with 1000
--DBCC CHECKIDENT ('Students', RESEED, 1000);
