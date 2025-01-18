Create OR ALTER PROCEDURE usp_GetMenuPermissions
@PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
     SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY MP.Id) AS RowNumber,
        MP.Id, MP.MenuName, MP.ParentId, MP.CurrentStatus,
        (SELECT MenuName FROM dbo.MenuPermissions WHERE Id = MP.ParentId) AS ParentMenuName,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.MenuPermissions MP
    WHERE MP.IsDeleted = 0
        AND (@SearchTerm IS NULL 
            OR MP.MenuName LIKE '%' + @SearchTerm + '%')
    ORDER BY MP.Id
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
