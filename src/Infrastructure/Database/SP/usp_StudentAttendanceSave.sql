CREATE OR ALTER PROCEDURE [dbo].[usp_StudentAttendanceSave]
    @AttendanceData NVARCHAR(MAX),
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Result INT = 0;
    DECLARE @ErrorMessage NVARCHAR(500) = '';
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Parse JSON data
        IF ISJSON(@AttendanceData) = 0
        BEGIN
            RAISERROR('Invalid JSON format provided', 16, 1);
            RETURN;
        END
        
        -- Insert or Update attendance records
        MERGE StudentAttendances AS target
        USING (
            SELECT 
                CAST(JSON_VALUE(value, '$.StudentId') AS INT) AS StudentId,
                CAST(JSON_VALUE(value, '$.AttendanceDate') AS DATE) AS AttendanceDate,
                CAST(JSON_VALUE(value, '$.AttendanceStatus') AS INT) AS AttendanceStatus,
                ISNULL(JSON_VALUE(value, '$.AttendanceNote'), '') AS AttendanceNote
            FROM OPENJSON(@AttendanceData)
        ) AS source
        ON (target.StudentId = source.StudentId AND target.AttendanceDate = source.AttendanceDate)
        
        WHEN MATCHED THEN
            UPDATE SET
                target.AttendanceStatus = source.AttendanceStatus,
                target.AttendanceNote = source.AttendanceNote,
                target.ModifyBy = @CreatedBy,
                target.ModifyDate = GETDATE()
                
        WHEN NOT MATCHED THEN
            INSERT (StudentId, AttendanceDate, AttendanceStatus, AttendanceNote, CreatedBy, DateCreated)
            VALUES (source.StudentId, source.AttendanceDate, source.AttendanceStatus, source.AttendanceNote, @CreatedBy, GETDATE());
        
        SET @Result = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SELECT 
            @Result AS RowsAffected,
            'Success' AS Status,
            '' AS ErrorMessage;
            
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SET @ErrorMessage = ERROR_MESSAGE();
        
        SELECT 
            0 AS RowsAffected,
            'Error' AS Status,
            @ErrorMessage AS ErrorMessage;
    END CATCH
END
