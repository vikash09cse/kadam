CREATE OR ALTER PROCEDURE dbo.usp_GetDistrictsByDivisionId
    @DivisionId INT,
    @StateId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        d.Id AS [Value],
        d.DistrictName AS [Text]
    FROM dbo.DivisionLocations dl
    INNER JOIN dbo.Districts d
        ON d.Id = dl.DistrictId
    WHERE dl.DivisionId = @DivisionId
      AND dl.StateId = @StateId
      AND dl.IsDeleted = 0
      AND d.IsDeleted = 0
      AND d.CurrentStatus = 1
    ORDER BY d.DistrictName;
END
