CREATE OR ALTER PROCEDURE usp_SaveStudentGRadeTestDetail
    @StudentId INT,
    @GradeLevelId INT,
    @CreatedBy INT,
    @TestDetails NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Delete existing test details for this student and grade level
        DELETE FROM StudentGradeTestDetails 
        WHERE StudentId = @StudentId AND GradeLevelId = @GradeLevelId AND IsDeleted = 0;
        
        -- Insert new test details from JSON
        INSERT INTO StudentGradeTestDetails (
            StudentId,
            GradeLevelId,
            SubjectId,
            StudentAge,
            ObtainedMarks,
            PercentageMarks,
            TotalMarks,
            CompletedDate,
            CreatedBy,
            DateCreated,
            IsDeleted
        )
        SELECT
            @StudentId,
            @GradeLevelId,
            JSON_VALUE(t.value, '$.SubjectId'),
            JSON_VALUE(t.value, '$.StudentAge'),
            JSON_VALUE(t.value, '$.ObtainedMarks'),
            JSON_VALUE(t.value, '$.PercentageMarks'),
            JSON_VALUE(t.value, '$.TotalMarks'),
            CASE 
                WHEN JSON_VALUE(t.value, '$.CompletedDate') IS NOT NULL 
                THEN TRY_CONVERT(DATETIME, JSON_VALUE(t.value, '$.CompletedDate')) 
                ELSE GETDATE() 
            END,
            @CreatedBy,
            GETDATE(),
            0  -- Not deleted
        FROM OPENJSON(@TestDetails) AS t;
        
        COMMIT TRANSACTION;
        SELECT 1 AS Success;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SELECT 0 AS Success, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH;
END