CREATE OR ALTER PROCEDURE dbo.usp_BackfillDivisionLocationsFromInstitutions
    @UserId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

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
            i.DivisionId,
            i.StateId,
            i.DistrictId,
            i.BlockId,
            i.VillageId,
            0,
            GETUTCDATE(),
            CASE WHEN @UserId > 0 THEN @UserId ELSE ISNULL(i.CreatedBy, 0) END
        FROM dbo.Institutions i
        WHERE i.IsDeleted = 0
          AND NOT EXISTS (
              SELECT 1
              FROM dbo.DivisionLocations dl
              WHERE dl.DivisionId = i.DivisionId
                AND dl.VillageId = i.VillageId
                AND dl.IsDeleted = 0
          );

        DECLARE @InsertedCount INT = @@ROWCOUNT;

        ;WITH StateCounts AS (
            SELECT
                i.DivisionId,
                i.StateId,
                COUNT(*) AS InstitutionCount
            FROM dbo.Institutions i
            WHERE i.IsDeleted = 0
            GROUP BY i.DivisionId, i.StateId
        ),
        RankedStates AS (
            SELECT
                DivisionId,
                StateId,
                ROW_NUMBER() OVER (
                    PARTITION BY DivisionId
                    ORDER BY InstitutionCount DESC, StateId
                ) AS RowNum
            FROM StateCounts
        )
        UPDATE d
        SET d.StateId = rs.StateId,
            d.ModifyDate = GETUTCDATE(),
            d.ModifyBy = CASE WHEN @UserId > 0 THEN @UserId ELSE d.ModifyBy END
        FROM dbo.Divisions d
        INNER JOIN RankedStates rs
            ON rs.DivisionId = d.Id
           AND rs.RowNum = 1
        WHERE d.IsDeleted = 0
          AND (d.StateId IS NULL OR d.StateId <> rs.StateId);

        DECLARE @DivisionsUpdated INT = @@ROWCOUNT;

        COMMIT TRANSACTION;

        SELECT
            @InsertedCount AS InsertedLocationCount,
            @DivisionsUpdated AS DivisionsStateUpdated,
            1 AS Success;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
