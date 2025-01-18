CREATE OR ALTER PROCEDURE usp_Villages
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY V.Id) AS RowNumber,
        V.Id, V.VillageName, B.BlockName, D.DistrictName, S.StateName, V.CurrentStatus,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.Villages V INNER JOIN dbo.Districts D ON V.DistrictId = D.Id
    INNER JOIN dbo.States S ON D.StateId = S.Id
    INNER JOIN dbo.Blocks B ON V.BlockId = B.Id
    WHERE V.IsDeleted = 0
        AND (@SearchTerm IS NULL 
            OR V.VillageName LIKE '%' + @SearchTerm + '%'
            OR D.DistrictName LIKE '%' + @SearchTerm + '%'
            OR S.StateName LIKE '%' + @SearchTerm + '%'
            OR B.BlockName LIKE '%' + @SearchTerm + '%')
    ORDER BY V.Id
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;



END