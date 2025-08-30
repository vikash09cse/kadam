CREATE OR ALter PROCEDURE [dbo].[usp_CheckTrioCapacity] --[usp_CheckTrioCapacity] 1,1
    @StudentId INT,
    @TrioId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Get the institution and grade of the student
        DECLARE @InstitutionId INT, @GradeId INT;
        
        SELECT @InstitutionId = InstitutionId, @GradeId = GradeId
        FROM Students 
        WHERE Id = @StudentId AND IsDeleted = 0;
        
        IF @InstitutionId IS NULL OR @GradeId IS NULL
        BEGIN
            SELECT 0 AS HasCapacity, 'Student not found' AS Message;
			 RETURN;
        END
        
       
        
        -- Check current trio count for this institution and grade
        DECLARE @CurrentTrioCount INT;
        SELECT @CurrentTrioCount = COUNT(*)
        FROM StudentTrios st
        INNER JOIN Students s ON st.StudentId = s.Id
        WHERE st.TrioId = @TrioId 
        AND s.InstitutionId = @InstitutionId
        AND s.GradeId = @GradeId
        AND st.IsDeleted = 0
        AND s.IsDeleted = 0;
        
        -- Return capacity status
        IF @CurrentTrioCount >= 3
        BEGIN
            SELECT 0 AS HasCapacity, 'Trio is already at maximum capacity (3 students)' AS Message;
        END
        ELSE
        BEGIN
            SELECT 1 AS HasCapacity, 'Trio has capacity for more students' AS Message;
        END
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
