
CREATE OR ALTER PROCEDURE usp_SaveStudentBaseline
    @StudentId INT,
    @CreatedBy INT,
    @BaselineType VARCHAR(50),
    @BaselineDetails NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Delete existing baseline details for this student
        DELETE FROM StudentBaselineDetails 
        WHERE StudentId = @StudentId AND BaselineType = @BaselineType;
        
        -- Insert new baseline details from JSON
        INSERT INTO StudentBaselineDetails (
            StudentId,
            SubjectId,
            StudentAge,
            BaselineType,
            ObtainedMarks,
            PercentageMarks,
            TotalMarks,
            CurrentStatus,
            IsDeleted,
            DateCreated,
            CompletedDate
        )
        SELECT
            @StudentId,
            JSON_VALUE(b.value, '$.SubjectId'),
            JSON_VALUE(b.value, '$.StudentAge'),
            @BaselineType,
            JSON_VALUE(b.value, '$.ObtainedMarks'),
            JSON_VALUE(b.value, '$.PercentageMarks'),
            JSON_VALUE(b.value, '$.TotalMarks'),
            1, -- CurrentStatus
            0, -- IsDeleted
            GETDATE(),
            CASE 
                WHEN JSON_VALUE(b.value, '$.CompletedDate') IS NOT NULL 
                THEN TRY_CONVERT(DATETIME2, JSON_VALUE(b.value, '$.CompletedDate'))
                ELSE NULL 
            END -- CompletedDate
        FROM OPENJSON(@BaselineDetails) AS b;

        if @BaselineType = 'baselinepreAssessment'
        BEGIN
            EXEC usp_IdentifyStudentGradeEntryAndExitLevel @StudentId, @CreatedBy
        END
        
        COMMIT TRANSACTION;
        SELECT 1 AS Success;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SELECT 0 AS Success, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH;
END
