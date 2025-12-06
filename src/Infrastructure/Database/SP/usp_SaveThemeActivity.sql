CREATE OR ALTER PROCEDURE usp_SaveThemeActivity
    @Id INT = 0,
    @ThemeId INT,
    @InstitutionId INT,
    @TotalStudents INT,
    @StudentAttended INT,
    @DidChildrenDayHappen BIT,
    @TotalParentsAttended INT = NULL,
    @ThemeActivityDate DATETIME = NULL,
    @CreatedBy INT,
    @GradeSections NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @ThemeActivityId INT;
        
        IF @Id > 0
        BEGIN
            -- Update existing ThemeActivity
            UPDATE ThemeActivities
            SET ThemeId = @ThemeId,
                InstitutionId = @InstitutionId,
                TotalStudents = @TotalStudents,
                StudentAttended = @StudentAttended,
                DidChildrenDayHappen = @DidChildrenDayHappen,
                TotalParentsAttended = @TotalParentsAttended,
                ThemeActivityDate = @ThemeActivityDate,
                ModifyBy = @CreatedBy,
                ModifyDate = GETDATE()
            WHERE Id = @Id;
            
            SET @ThemeActivityId = @Id;
            
            -- Delete existing GradeSections for this ThemeActivity
            DELETE FROM ThemeActivityGradeSections 
            WHERE ThemeActivityId = @ThemeActivityId;
        END
        ELSE
        BEGIN
            -- Insert new ThemeActivity
            INSERT INTO ThemeActivities (
                ThemeId,
                InstitutionId,
                TotalStudents,
                StudentAttended,
                DidChildrenDayHappen,
                TotalParentsAttended,
                ThemeActivityDate,
                CurrentStatus,
                CreatedBy,
                DateCreated,
                IsDeleted
            )
            VALUES (
                @ThemeId,
                @InstitutionId,
                @TotalStudents,
                @StudentAttended,
                @DidChildrenDayHappen,
                @TotalParentsAttended,
                @ThemeActivityDate,
                1, -- CurrentStatus
                @CreatedBy,
                GETDATE(),
                0 -- IsDeleted
            );
            
            SET @ThemeActivityId = SCOPE_IDENTITY();
        END
        
        -- Insert GradeSections from JSON if provided
        IF @GradeSections IS NOT NULL AND @GradeSections != ''
        BEGIN
            INSERT INTO ThemeActivityGradeSections (
                ThemeActivityId,
                GradeId,
                Section
            )
            SELECT
                @ThemeActivityId,
                CAST(JSON_VALUE(gs.value, '$.GradeId') AS INT),
                JSON_VALUE(gs.value, '$.Section')
            FROM OPENJSON(@GradeSections) AS gs;
        END
        
        COMMIT TRANSACTION;
        SELECT @ThemeActivityId AS Id, 1 AS Success;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SELECT 0 AS Id, 0 AS Success, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH;
END

