CREATE OR ALTER PROCEDURE dbo.usp_GetBlocksByDivisionId
    @DivisionId INT,
    @DistrictId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        b.Id AS [Value],
        b.BlockName AS [Text]
    FROM dbo.DivisionLocations dl
    INNER JOIN dbo.Blocks b
        ON b.Id = dl.BlockId
    WHERE dl.DivisionId = @DivisionId
      AND dl.DistrictId = @DistrictId
      AND dl.IsDeleted = 0
      AND b.IsDeleted = 0
      AND b.CurrentStatus = 1
    ORDER BY b.BlockName;
END
