CREATE OR ALTER PROCEDURE dbo.usp_GetVillagesByDivisionId
    @DivisionId INT,
    @BlockId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        v.Id AS [Value],
        v.VillageName AS [Text]
    FROM dbo.DivisionLocations dl
    INNER JOIN dbo.Villages v
        ON v.Id = dl.VillageId
    WHERE dl.DivisionId = @DivisionId
      AND dl.BlockId = @BlockId
      AND dl.IsDeleted = 0
      AND v.IsDeleted = 0
    ORDER BY v.VillageName;
END
