CREATE OR ALTER PROCEDURE usp_IdentifyStudentGradeEntryAndExitLevel
    @StudentId INT,
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- ================================================================
    -- PROCEDURE: usp_IdentifyStudentGradeEntryAndExitLevel
    -- PURPOSE: Determines student entry and exit steps based on age and baseline scores
    -- AUTHOR: System Generated
    -- DATE: Updated with new criteria logic
    -- ================================================================
    
    -- Variable declarations
    DECLARE @Age INT,
            @TotalObtainedMarks INT,
            @GradeId INT,
            @GradeName VARCHAR(50),
            @IsKadam BIT,
            @EntryStepId INT,
            @ExitStepId INT,
            @EntryGradeLevel INT,
            @ExitGradeLevel INT;

    -- Get student basic information
    SELECT @Age = Age, @GradeId = GradeId
    FROM Students
    WHERE Id = @StudentId;

    -- Get grade name
    SELECT @GradeName = GradeName
    FROM Grades
    WHERE Id = @GradeId;

    -- Determine if student is in Kadam program
    -- Kadam classification logic for future use
    SET @IsKadam = CASE
        WHEN @GradeName LIKE '%1st%' OR @GradeName LIKE '%2nd%' OR @GradeName LIKE '%3rd%' OR @GradeName LIKE '%4th%' OR @GradeName LIKE '%5th%' THEN 1
        WHEN @GradeName LIKE '%Kadam%' AND @GradeName NOT LIKE '%Kadam+%' THEN 0
        WHEN @GradeName LIKE '%Kadam+%' THEN 1
        ELSE 1
    END;

    -- Get total baseline assessment marks
    SELECT @TotalObtainedMarks = ISNULL(SUM(ObtainedMarks), 0)
    FROM StudentBaselineDetails
    WHERE StudentId = @StudentId 
      AND BaselineType = 'baselinepreAssessment';

    -- Ensure marks are within valid range (0-200)
    SET @TotalObtainedMarks = CASE 
        WHEN @TotalObtainedMarks < 0 THEN 0
        WHEN @TotalObtainedMarks > 200 THEN 200
        ELSE @TotalObtainedMarks
    END;

    -- ================================================================
    -- STEP DETERMINATION LOGIC
    -- Based on Age/Grade and Baseline Score criteria table
    -- ================================================================
    
    -- Create a score range identifier for cleaner logic
    DECLARE @ScoreRange INT = 
        CASE 
            WHEN @TotalObtainedMarks BETWEEN 0 AND 40 THEN 1
            WHEN @TotalObtainedMarks BETWEEN 41 AND 60 THEN 2
            WHEN @TotalObtainedMarks BETWEEN 61 AND 80 THEN 3
            WHEN @TotalObtainedMarks BETWEEN 81 AND 100 THEN 4
            WHEN @TotalObtainedMarks BETWEEN 101 AND 120 THEN 5
            WHEN @TotalObtainedMarks BETWEEN 121 AND 140 THEN 6
            WHEN @TotalObtainedMarks BETWEEN 141 AND 160 THEN 7
            WHEN @TotalObtainedMarks BETWEEN 161 AND 180 THEN 8
            WHEN @TotalObtainedMarks BETWEEN 181 AND 200 THEN 9
            ELSE 1 -- Default to lowest range
        END;

    -- Determine Entry and Exit Steps using unified logic
    SELECT 
        @EntryStepId = EntryStep,
        @ExitStepId = ExitStep
    FROM (
        SELECT 
            CASE 
                -- Age 6 (Grade 1)
                WHEN @Age = 6 AND @ScoreRange = 1 THEN 1  -- 0-40
                WHEN @Age = 6 AND @ScoreRange = 2 THEN 1  -- 41-60
                WHEN @Age = 6 AND @ScoreRange = 3 THEN 3  -- 61-80
                WHEN @Age = 6 AND @ScoreRange = 4 THEN 3  -- 81-100
                WHEN @Age = 6 AND @ScoreRange = 5 THEN 5  -- 101-120
                
                -- Age 7 (Grade 2)
                WHEN @Age = 7 AND @ScoreRange IN (1,2) THEN 1  -- 0-60
                WHEN @Age = 7 AND @ScoreRange IN (3,4) THEN 3  -- 61-100
                WHEN @Age = 7 AND @ScoreRange = 5 THEN 5       -- 101-120
                
                -- Age 8 (Grade 3)
                WHEN @Age = 8 AND @ScoreRange IN (1,2) THEN 1  -- 0-60
                WHEN @Age = 8 AND @ScoreRange IN (3,4,5) THEN 3 -- 61-120
                
                -- Age 9 (Grade 4)
                WHEN @Age = 9 AND @ScoreRange IN (1,2) THEN 1  -- 0-60
                WHEN @Age = 9 AND @ScoreRange IN (3,4) THEN 3  -- 61-100
                WHEN @Age = 9 AND @ScoreRange IN (5,6) THEN 5  -- 101-140
                WHEN @Age = 9 AND @ScoreRange = 7 THEN 7       -- 141-160
                
                -- Age 10+ (Grade 5)
                WHEN @Age >= 10 AND @ScoreRange IN (1,2) THEN 1  -- 0-60
                WHEN @Age >= 10 AND @ScoreRange IN (3,4) THEN 3  -- 61-100
                WHEN @Age >= 10 AND @ScoreRange IN (5,6) THEN 5  -- 101-140
                WHEN @Age >= 10 AND @ScoreRange IN (7,8) THEN 7  -- 141-180
                WHEN @Age >= 10 AND @ScoreRange = 9 THEN 9       -- 181-200
                
                -- Default case
                ELSE 1
            END AS EntryStep,
            
            CASE 
                -- Age 6 (Grade 1)
                WHEN @Age = 6 AND @ScoreRange = 1 THEN 2  -- 0-40
                WHEN @Age = 6 AND @ScoreRange = 2 THEN 4  -- 41-60
                WHEN @Age = 6 AND @ScoreRange = 3 THEN 4  -- 61-80
                WHEN @Age = 6 AND @ScoreRange = 4 THEN 6  -- 81-100
                WHEN @Age = 6 AND @ScoreRange = 5 THEN 6  -- 101-120
                
                -- Age 7 (Grade 2)
                WHEN @Age = 7 AND @ScoreRange IN (1,2) THEN 4  -- 0-60
                WHEN @Age = 7 AND @ScoreRange = 3 THEN 4       -- 61-80
                WHEN @Age = 7 AND @ScoreRange = 4 THEN 6       -- 81-100
                WHEN @Age = 7 AND @ScoreRange = 5 THEN 6       -- 101-120
                
                -- Age 8 (Grade 3)
                WHEN @Age = 8 THEN 8  -- All ranges lead to Step 8
                
                -- Age 9 (Grade 4)
                WHEN @Age = 9 THEN 8  -- All ranges lead to Step 8
                
                -- Age 10+ (Grade 5)
                WHEN @Age >= 10 THEN 10  -- All ranges lead to Step 10
                
                -- Default case
                ELSE 2
            END AS ExitStep
    ) AS StepCalculation;
    
    -- ================================================================
    -- GRADE LEVEL CALCULATION
    -- Calculate grade levels based on step ranges (Steps 1-2=Grade1, 3-4=Grade2, etc.)
    -- ================================================================
    
    SELECT 
        @EntryGradeLevel = (@EntryStepId + 1) / 2,    -- Formula: (Step + 1) / 2
        @ExitGradeLevel = (@ExitStepId + 1) / 2;      -- More efficient than CASE statements

    -- Ensure grade levels are within valid range (1-5)
    SET @EntryGradeLevel = CASE 
        WHEN @EntryGradeLevel < 1 THEN 1 
        WHEN @EntryGradeLevel > 5 THEN 5 
        ELSE @EntryGradeLevel 
    END;
    
    SET @ExitGradeLevel = CASE 
        WHEN @ExitGradeLevel < 1 THEN 1 
        WHEN @ExitGradeLevel > 5 THEN 5 
        ELSE @ExitGradeLevel 
    END;

    -- ================================================================
    -- DATABASE OPERATION
    -- Insert or Update student grade start and end details
    -- ================================================================
    
    -- Use MERGE for more efficient INSERT/UPDATE operation
    MERGE StudentGradeStartAndEndDetails AS target
    USING (
        SELECT 
            @StudentId AS StudentId,
            @EntryGradeLevel AS GradeEntryLevelId,
            @ExitGradeLevel AS GradeExitLevelId,
            @EntryStepId AS EntryStepId,
            @ExitStepId AS ExitStepId,
            @CreatedBy AS CreatedBy,
            GETDATE() AS DateCreated
    ) AS source ON target.StudentId = source.StudentId
    
    WHEN MATCHED THEN
        UPDATE SET 
            GradeEntryLevelId = source.GradeEntryLevelId,
            GradeExitLevelId = source.GradeExitLevelId,
            EntryStepId = source.EntryStepId,
            ExitStepId = source.ExitStepId,
            ModifyBy = source.CreatedBy,
            ModifyDate = source.DateCreated
    
    WHEN NOT MATCHED THEN
        INSERT (StudentId, GradeEntryLevelId, GradeExitLevelId, EntryStepId, ExitStepId, CreatedBy, DateCreated)
        VALUES (source.StudentId, source.GradeEntryLevelId, source.GradeExitLevelId, 
                source.EntryStepId, source.ExitStepId, source.CreatedBy, source.DateCreated);

    -- ================================================================
    -- RETURN RESULT (Optional: for debugging/verification)
    -- ================================================================
    
    --SELECT 
    --    @StudentId AS StudentId,
    --    @Age AS Age,
    --    @TotalObtainedMarks AS BaselineMarks,
    --    @ScoreRange AS ScoreRangeId,
    --    @EntryStepId AS EntryStepId,
    --    @ExitStepId AS ExitStepId,
    --    @EntryGradeLevel AS EntryGradeLevel,
    --    @ExitGradeLevel AS ExitGradeLevel,
    --    'Success' AS Status;

END