CREATE OR ALTER PROCEDURE dbo.usp_DivisionList
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY Id) AS RowNumber,
        Id, DivisionName, DivisionCode,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.Divisions
    WHERE IsDeleted = 0
        AND (@SearchTerm IS NULL 
            OR DivisionName LIKE '%' + @SearchTerm + '%'
            OR DivisionCode LIKE '%' + @SearchTerm + '%')
    ORDER BY Id
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END 