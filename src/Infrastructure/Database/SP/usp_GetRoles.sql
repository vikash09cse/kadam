Create OR ALTER PROCEDURE usp_GetRoles
@PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
     SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY R.Id) AS RowNumber,
        R.Id, R.RoleName, R.CurrentStatus,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.Roles R
    WHERE R.IsDeleted = 0
        AND (@SearchTerm IS NULL 
            OR R.RoleName LIKE '%' + @SearchTerm + '%')
    ORDER BY Id
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
