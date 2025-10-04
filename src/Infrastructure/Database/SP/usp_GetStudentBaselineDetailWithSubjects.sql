CREATE OR ALTER PROCEDURE usp_GetStudentBaselineDetailWithSubjects -- usp_GetStudentBaselineDetailWithSubjects 1, 'baselinepreAssessment'
    @StudentId INT,
    @BaselineType VARCHAR(50)= NULL
AS
BEGIN
    DECLARE @Age INT;
    DECLARE @GradeId INT;
    DECLARE @GradeName VARCHAR(50);
    DECLARE @IsKadam BIT;
    DECLARE @SubjectTotalMarks INT;

    -- Get student's age and grade information
    SELECT @Age = Age, @GradeId = GradeId
    FROM Students
    WHERE Id = @StudentId;

    -- Get grade name
    SELECT @GradeName = GradeName
    FROM Grades
    WHERE Id = @GradeId;

    -- Determine if it's Kadam or regular grade
    SET @IsKadam = CASE
        WHEN @GradeName LIKE '%1st%' OR @GradeName LIKE '%2nd%' OR @GradeName LIKE '%3rd%' OR @GradeName LIKE '%4th%' OR @GradeName LIKE '%5th%' THEN 1
        WHEN @GradeName LIKE '%Kadam%' THEN 0
        WHEN @GradeName LIKE '%Kadam+%' THEN 1
    END

    -- Calculate subject total marks based on grade/age
    IF @IsKadam = 1
    BEGIN
        -- Use grade for subject total marks
        SET @SubjectTotalMarks = CASE
            WHEN @GradeName LIKE '%1st%' THEN 10
            WHEN @GradeName LIKE '%2nd%' THEN 20
            WHEN @GradeName LIKE '%3rd%' THEN 30
            WHEN @GradeName LIKE '%4th%' THEN 40
            WHEN @GradeName LIKE '%5th%' THEN 50
            ELSE 50 -- Default fallback
        END
    END
    ELSE
    BEGIN
        -- Use age for subject total marks
        SET @SubjectTotalMarks = CASE
            WHEN @Age <= 5 THEN 10
            WHEN @Age = 6 THEN 10
            WHEN @Age = 7 THEN 20
            WHEN @Age = 8 THEN 30
            WHEN @Age = 9 THEN 40
            WHEN @Age >= 10 THEN 50
            ELSE 50 -- Default fallback
        END
    END

    SELECT 
        ROW_NUMBER() OVER (ORDER BY sb.SubjectName) AS RowNo,
        sbd.Id,
        sbd.StudentId,
        sb.Id As SubjectId,
        sbd.StudentAge,
        sbd.BaselineType,
        sbd.ObtainedMarks,
        sbd.PercentageMarks,
        ISNULL(sbd.TotalMarks, @SubjectTotalMarks) AS TotalMarks,
        sbd.CurrentStatus,
        sb.SubjectName
    FROM  Subjects sb 
    LEFT JOIN StudentBaselineDetails sbd ON sbd.SubjectId = sb.Id AND sbd.StudentId = @StudentId 
    AND sbd.BaselineType = COALESCE(@BaselineType, sbd.BaselineType)
    WHERE  sb.CurrentStatus = 1 AND sb.IsDeleted = 0 
    ORDER BY sb.DisplayOrder
END