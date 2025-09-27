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
        
        -- Check how many trios of 4 students already exist in this class
        DECLARE @TriosOfFourCount INT;
        SELECT @TriosOfFourCount = COUNT(*)
        FROM (
            SELECT st.TrioId, COUNT(*) as StudentCount
            FROM StudentTrios st
            INNER JOIN Students s ON st.StudentId = s.Id
            WHERE s.InstitutionId = @InstitutionId
            AND s.GradeId = @GradeId
            AND st.IsDeleted = 0
            AND s.IsDeleted = 0
            GROUP BY st.TrioId
            HAVING COUNT(*) = 4
        ) AS TriosOfFour;
        
        -- Return capacity status based on new rules
        IF @CurrentTrioCount >= 4
        BEGIN
            SELECT 0 AS HasCapacity, 'Trio is already at maximum capacity (4 students)' AS Message;
        END
        ELSE IF @CurrentTrioCount = 3 AND @TriosOfFourCount >= 2
        BEGIN
            SELECT 0 AS HasCapacity, 'Cannot exceed 4 students as maximum 2 trios of 4 students allowed per class' AS Message;
        END
        ELSE
        BEGIN
            DECLARE @CanExpand BIT = 1;
            DECLARE @CapacityMessage NVARCHAR(255) = 'Trio has capacity for more students';
            
            -- If this would be the 3rd trio of 4 students, don't allow it
            IF @CurrentTrioCount = 3 AND @TriosOfFourCount >= 2
            BEGIN
                SET @CanExpand = 0;
                SET @CapacityMessage = 'Cannot add more students - maximum 2 trios of 4 students allowed per class';
            END
            
            SELECT @CanExpand AS HasCapacity, @CapacityMessage AS Message;
        END
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
