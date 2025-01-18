Create OR ALTER PROCEDURE usp_GetThemes
@PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
     SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY T.Id) AS RowNumber,
        T.Id, T.ThemeName, T.CurrentStatus,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.Themes T
    WHERE T.IsDeleted = 0
        AND (@SearchTerm IS NULL 
            OR T.ThemeName LIKE '%' + @SearchTerm + '%')
    ORDER BY Id
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
