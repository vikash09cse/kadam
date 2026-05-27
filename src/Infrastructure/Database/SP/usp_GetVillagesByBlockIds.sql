CREATE OR ALTER PROCEDURE dbo.usp_GetVillagesByBlockIds
    @BlockIds NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        v.Id AS [Value],
        v.VillageName AS [Text]
    FROM OPENJSON(@BlockIds) j
    INNER JOIN dbo.Villages v
        ON v.BlockId = TRY_CAST(j.value AS INT)
    WHERE v.IsDeleted = 0
    ORDER BY v.VillageName;
END
