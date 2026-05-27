CREATE OR ALTER PROCEDURE dbo.usp_GetDivisionLocationAssignment
    @DivisionId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DistrictIdsJson NVARCHAR(MAX) = '[]';
    DECLARE @BlockIdsJson NVARCHAR(MAX) = '[]';
    DECLARE @VillageIdsJson NVARCHAR(MAX) = '[]';

    IF NOT EXISTS (SELECT 1 FROM dbo.Divisions WHERE Id = @DivisionId AND IsDeleted = 0)
    BEGIN
        SELECT
            @DivisionId AS DivisionId,
            CAST('' AS VARCHAR(55)) AS DivisionName,
            CAST(0 AS BIT) AS HasLocation,
            CAST(NULL AS INT) AS StateId,
            @DistrictIdsJson AS DistrictIdsJson,
            @BlockIdsJson AS BlockIdsJson,
            @VillageIdsJson AS VillageIdsJson;
        RETURN;
    END

    SELECT @DistrictIdsJson =
        '[' + ISNULL(STRING_AGG(CAST(DistrictId AS NVARCHAR(20)), ','), '') + ']'
    FROM (
        SELECT DISTINCT DistrictId
        FROM dbo.DivisionLocations
        WHERE DivisionId = @DivisionId AND IsDeleted = 0
    ) d;

    SELECT @BlockIdsJson =
        '[' + ISNULL(STRING_AGG(CAST(BlockId AS NVARCHAR(20)), ','), '') + ']'
    FROM (
        SELECT DISTINCT BlockId
        FROM dbo.DivisionLocations
        WHERE DivisionId = @DivisionId AND IsDeleted = 0
    ) b;

    SELECT @VillageIdsJson =
        '[' + ISNULL(STRING_AGG(CAST(VillageId AS NVARCHAR(20)), ','), '') + ']'
    FROM (
        SELECT DISTINCT VillageId
        FROM dbo.DivisionLocations
        WHERE DivisionId = @DivisionId AND IsDeleted = 0
    ) v;

    SELECT
        d.Id AS DivisionId,
        d.DivisionName,
        CAST(
            CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM dbo.DivisionLocations dl
                    WHERE dl.DivisionId = d.Id AND dl.IsDeleted = 0
                )
                THEN 1
                ELSE 0
            END AS BIT
        ) AS HasLocation,
        COALESCE(
            d.StateId,
            (
                SELECT TOP 1 dl.StateId
                FROM dbo.DivisionLocations dl
                WHERE dl.DivisionId = d.Id AND dl.IsDeleted = 0
            )
        ) AS StateId,
        @DistrictIdsJson AS DistrictIdsJson,
        @BlockIdsJson AS BlockIdsJson,
        @VillageIdsJson AS VillageIdsJson
    FROM dbo.Divisions d
    WHERE d.Id = @DivisionId
      AND d.IsDeleted = 0;
END
