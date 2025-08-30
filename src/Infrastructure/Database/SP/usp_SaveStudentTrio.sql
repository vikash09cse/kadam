CREATE Or ALter PROCEDURE [dbo].[usp_SaveStudentTrio]
    @StudentId INT,
    @TrioId INT,
    @CreatedBy INT
AS
BEGIN
    --SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Use MERGE to handle both INSERT and UPDATE operations
        MERGE StudentTrios AS target
        USING (SELECT @StudentId AS StudentId, @TrioId AS TrioId) AS source
        ON target.StudentId = source.StudentId
        
        WHEN MATCHED AND target.IsDeleted = 0 THEN
            -- Update existing record
            UPDATE SET 
                TrioId = source.TrioId,
                ModifyBy = @CreatedBy,
                ModifyDate = GETDATE()
        
        WHEN NOT MATCHED THEN
            -- Insert new record
            INSERT (StudentId, TrioId, CreatedBy, DateCreated, IsDeleted)
            VALUES (source.StudentId, source.TrioId, @CreatedBy, GETDATE(), 0);
        
        COMMIT TRANSACTION;
        
        --SELECT 1 AS Success;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
