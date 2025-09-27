CREATE OR ALTER PROCEDURE [dbo].[usp_CheckStudentPreviousGradeMarks] --[usp_CheckStudentPreviousGradeMarks] @StudentId = 1, @GradeLevelId = 2
    @StudentId INT,
    @GradeLevelId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Variables to store results
        DECLARE @PreviousGradeExists BIT = 0;
        DECLARE @TotalObtainedMarks DECIMAL(18,2) = 0;
        DECLARE @TotalMarks DECIMAL(18,2) = 0;
        DECLARE @OverallPercentage DECIMAL(18,2) = 0;
        DECLARE @HasPassed80Percent BIT = 0;
        DECLARE @Message NVARCHAR(500) = '';
        DECLARE @PreviousGradeLevelId INT;
        
        -- Calculate previous grade level (assuming grade levels are sequential)
        SET @PreviousGradeLevelId = @GradeLevelId - 1;
        
        -- Check if previous grade test exists for the student
        IF EXISTS (
            SELECT 1 
            FROM StudentGradeTestDetails 
            WHERE StudentId = @StudentId 
            AND GradeLevelId = @PreviousGradeLevelId 
            AND IsDeleted = 0
        )
        BEGIN
            SET @PreviousGradeExists = 1;
            
            -- Calculate total obtained marks and total marks for all subjects in previous grade
            SELECT 
                @TotalObtainedMarks = ISNULL(SUM(ObtainedMarks), 0),
                @TotalMarks = ISNULL(SUM(TotalMarks), 0)
            FROM StudentGradeTestDetails 
            WHERE StudentId = @StudentId 
            AND GradeLevelId = @PreviousGradeLevelId 
            AND IsDeleted = 0
            AND ObtainedMarks IS NOT NULL 
            AND TotalMarks IS NOT NULL;
            
            -- Calculate overall percentage
            IF @TotalMarks > 0
            BEGIN
                SET @OverallPercentage = (@TotalObtainedMarks / @TotalMarks) * 100;
                
                -- Check if student obtained 80% or more
                IF @OverallPercentage >= 80
                BEGIN
                    SET @HasPassed80Percent = 1;
                    SET @Message = 'Student has passed with ' + CAST(@OverallPercentage AS NVARCHAR(10)) + '% in previous grade';
                END
                ELSE
                BEGIN
                    SET @HasPassed80Percent = 0;
                    SET @Message = 'Student has not achieved 80% in previous grade. Obtained: ' + CAST(@OverallPercentage AS NVARCHAR(10)) + '%';
                END
            END
            ELSE
            BEGIN
                SET @Message = 'No valid marks found for previous grade';
            END
        END
        ELSE
        BEGIN
            SET @PreviousGradeExists = 0;
            SET @Message = 'Previous grade test does not exist for this student';
        END
        
        -- Return the results
        SELECT 
            @PreviousGradeExists AS PreviousGradeExists,
            @TotalObtainedMarks AS TotalObtainedMarks,
            @TotalMarks AS TotalMarks,
            @OverallPercentage AS OverallPercentage,
            @HasPassed80Percent AS HasPassed80Percent,
            @Message AS Message,
            @PreviousGradeLevelId AS PreviousGradeLevelId;
            
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        -- Return error information
        SELECT 
            0 AS PreviousGradeExists,
            0 AS TotalObtainedMarks,
            0 AS TotalMarks,
            0 AS OverallPercentage,
            0 AS HasPassed80Percent,
            'Error: ' + @ErrorMessage AS Message,
            @GradeLevelId - 1 AS PreviousGradeLevelId;
            
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
