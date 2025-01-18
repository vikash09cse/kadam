CREATE OR ALTER PROCEDURE dbo.usp_GetDistricts
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY D.Id) AS RowNumber,
        D.Id, D.DistrictName, D.DistrictCode, S.StateName, D.CurrentStatus,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.Districts D INNER JOIN dbo.States S ON D.StateId = S.Id
    WHERE D.IsDeleted = 0
        AND (@SearchTerm IS NULL 
            OR D.DistrictName LIKE '%' + @SearchTerm + '%'
            OR D.DistrictCode LIKE '%' + @SearchTerm + '%')
    ORDER BY Id
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
