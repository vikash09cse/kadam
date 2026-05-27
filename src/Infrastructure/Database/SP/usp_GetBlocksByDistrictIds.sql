CREATE OR ALTER PROCEDURE dbo.usp_GetBlocksByDistrictIds
    @DistrictIds NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        b.Id AS [Value],
        b.BlockName AS [Text]
    FROM OPENJSON(@DistrictIds) j
    INNER JOIN dbo.Blocks b
        ON b.DistrictId = TRY_CAST(j.value AS INT)
    WHERE b.IsDeleted = 0
      AND b.CurrentStatus = 1
    ORDER BY b.BlockName;
END
