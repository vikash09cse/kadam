-- Backfill DivisionLocations and Divisions.StateId from existing Institutions.
-- Run once after deploying DivisionLocations table for legacy data.

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @DivisionsUpdated INT;

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
        ISNULL(i.CreatedBy, 0)
    FROM dbo.Institutions i
    WHERE i.IsDeleted = 0
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.DivisionLocations dl
          WHERE dl.DivisionId = i.DivisionId
            AND dl.VillageId = i.VillageId
            AND dl.IsDeleted = 0
      );

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
    SET d.StateId = rs.StateId
    FROM dbo.Divisions d
    INNER JOIN RankedStates rs
        ON rs.DivisionId = d.Id
       AND rs.RowNum = 1
    WHERE d.IsDeleted = 0
      AND (d.StateId IS NULL OR d.StateId <> rs.StateId);

    SET @DivisionsUpdated = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT
        (SELECT COUNT(*) FROM dbo.DivisionLocations WHERE IsDeleted = 0) AS TotalDivisionLocations,
        @DivisionsUpdated AS DivisionsStateUpdated;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH
