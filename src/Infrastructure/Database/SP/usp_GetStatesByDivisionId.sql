CREATE OR ALTER PROCEDURE dbo.usp_GetStatesByDivisionId
    @DivisionId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        s.Id AS [Value],
        s.StateName AS [Text]
    FROM dbo.DivisionLocations dl
    INNER JOIN dbo.States s
        ON s.Id = dl.StateId
    WHERE dl.DivisionId = @DivisionId
      AND dl.IsDeleted = 0
      AND s.IsDeleted = 0
      AND s.CurrentStatus = 1
    ORDER BY s.StateName;
END
