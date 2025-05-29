CREATE OR ALTER PROCEDURE usp_IdentifyStudentGradeEntryAndExitLevel --1,1
    @StudentId INT,
    @CreatedBy INT
AS
BEGIN
    DECLARE @EntryGradeLevel INT;
    DECLARE @ExitGradeLevel INT;

    Declare @Age INT;
    Declare @TotalObtainedMarks INT;
    Declare @GradeId INT;
    Declare @GradeName VARCHAR(50);
    Declare @IsKadam BIT;
    Declare @EntryStepId INT;
    Declare @ExitStepId INT;

    Select @Age = Age, @GradeId = GradeId
    FROM Students
    WHERE Id = @StudentId;

    Select @GradeName = GradeName
    FROM Grades
    WHERE Id = @GradeId;


    -- If GradeName contains 1st, 2nd, 3rd, 4th, 5th then IsKadam is 1
    -- If GradeName is Kadam then IsKadam is 0
    -- If GradeName is Kadam+ then IsKadam is 1
    Set @IsKadam = CASE
        WHEN @GradeName LIKE '%1st%' OR @GradeName LIKE '%2nd%' OR @GradeName LIKE '%3rd%' OR @GradeName LIKE '%4th%' OR @GradeName LIKE '%5th%' THEN 1
        WHEN @GradeName LIKE '%Kadam%' THEN 0
        WHEN @GradeName LIKE '%Kadam+%' THEN 1
    END

    Select @TotalObtainedMarks = SUM(ObtainedMarks)
    FROM StudentBaselineDetails
    WHERE StudentId = @StudentId And BaselineType = 'baselinepreAssessment'

	-- Print 'Age' + CAST(@Age AS VARCHAR)
    -- Print 'GradeId' + CAST(@GradeId AS VARCHAR)
    -- Print 'GradeName' + @GradeName
	-- Print 'TotalObtainedMarks' + CAST(@TotalObtainedMarks AS VARCHAR)
    -- Print 'IsKadam' + CAST(@IsKadam AS VARCHAR)

    Select @EntryGradeLevel = CASE
        WHEN @TotalObtainedMarks >= 0 AND @TotalObtainedMarks <= 40 THEN 1
        WHEN @TotalObtainedMarks >= 41 AND @TotalObtainedMarks <= 80 THEN 2
        WHEN @TotalObtainedMarks >= 81 AND @TotalObtainedMarks <= 120 THEN 3
        WHEN @TotalObtainedMarks >= 121 AND @TotalObtainedMarks <= 160 THEN 4
        WHEN @TotalObtainedMarks >= 161 AND @TotalObtainedMarks <= 200 THEN 5
    END

   if @IsKadam = 1
   BEGIN
    Select @ExitGradeLevel = CASE
        WHEN @GradeName LIKE '%1st%' THEN 1
        WHEN @GradeName LIKE '%2nd%' THEN 2
        WHEN @GradeName LIKE '%3rd%' THEN 3
        WHEN @GradeName LIKE '%4th%' THEN 4
        WHEN @GradeName LIKE '%5th%' THEN 5
    END
	If @EntryGradeLevel > @ExitGradeLevel
     BEGIN
        Select @EntryGradeLevel = @ExitGradeLevel
     END
   END
   ELSE
   BEGIN
    Select @ExitGradeLevel = CASE
        WHEN @Age <= 5 THEN 1
        WHEN @Age = 6 THEN 2
        WHEN @Age = 7 THEN 3
        WHEN @Age = 8 THEN 4
        WHEN @Age = 9 THEN 5
        WHEN @Age >= 10 THEN 5
       END
	End
    Select @EntryStepId = CASE
        WHEN @EntryGradeLevel = 1 THEN 1
        WHEN @EntryGradeLevel = 2 THEN 3
        WHEN @EntryGradeLevel = 3 THEN 5
        WHEN @EntryGradeLevel = 4 THEN 7
        WHEN @EntryGradeLevel = 5 THEN 9
    END

    Select @ExitStepId = CASE 
        WHEN @ExitGradeLevel = 1 THEN 2
        WHEN @ExitGradeLevel = 2 THEN 4
        WHEN @ExitGradeLevel = 3 THEN 6
        WHEN @ExitGradeLevel = 4 THEN 8
        WHEN @ExitGradeLevel = 5 THEN 10
    END

    
    IF NOT EXISTS (SELECT 1 FROM StudentGradeStartAndEndDetails WHERE StudentId = @StudentId)
    BEGIN
        Insert into StudentGradeStartAndEndDetails (StudentId, GradeEntryLevelId, GradeExitLevelId, EntryStepId, ExitStepId, CreatedBy)
        VALUES (@StudentId, @EntryGradeLevel, @ExitGradeLevel, @EntryStepId, @ExitStepId, @CreatedBy)
    END
    ELSE
    BEGIN
        Update StudentGradeStartAndEndDetails
        SET GradeEntryLevelId = @EntryGradeLevel,
            GradeExitLevelId = @ExitGradeLevel,
            EntryStepId = @EntryStepId,
            ExitStepId = @ExitStepId,
            CreatedBy = @CreatedBy
        WHERE StudentId = @StudentId
    END

END