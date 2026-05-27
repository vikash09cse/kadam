CREATE OR ALTER PROCEDURE dbo.usp_SaveDivisionLocation
    @DivisionId INT,
    @StateId INT,
    @VillageIds NVARCHAR(MAX),
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Divisions WHERE Id = @DivisionId AND IsDeleted = 0)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success;
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM OPENJSON(@VillageIds))
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success;
            RETURN;
        END

        IF EXISTS (
            SELECT 1
            FROM OPENJSON(@VillageIds) j
            LEFT JOIN dbo.Villages v
                ON v.Id = TRY_CAST(j.value AS INT)
               AND v.IsDeleted = 0
               AND v.StateId = @StateId
            WHERE v.Id IS NULL
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success;
            RETURN;
        END

        UPDATE dbo.DivisionLocations
        SET IsDeleted = 1,
            DeletedBy = @UserId,
            DeletedDate = GETUTCDATE()
        WHERE DivisionId = @DivisionId
          AND IsDeleted = 0;

        INSERT INTO dbo.DivisionLocations
        (
            DivisionId,
            StateId,
            DistrictId,
            BlockId,
            VillageId,
            IsDeleted,
            DateCreated,
            CreatedBy
        )
        SELECT DISTINCT
            @DivisionId,
            v.StateId,
            v.DistrictId,
            v.BlockId,
            v.Id,
            0,
            GETUTCDATE(),
            @UserId
        FROM OPENJSON(@VillageIds) j
        INNER JOIN dbo.Villages v
            ON v.Id = TRY_CAST(j.value AS INT)
        WHERE v.IsDeleted = 0
          AND v.StateId = @StateId;

        UPDATE dbo.Divisions
        SET StateId = @StateId,
            ModifyBy = @UserId,
            ModifyDate = GETUTCDATE()
        WHERE Id = @DivisionId;

        COMMIT TRANSACTION;
        SELECT 1 AS Success;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
