CREATE OR ALTER PROCEDURE dbo.usp_GetUserList
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY Id) AS RowNumber,
        Id, Email, UserName, FirstName, LastName, 
        Phone, Gender, 
        RoleId, ReporteeRoleId,
        COUNT(*) OVER() AS TotalCount
    FROM Users 
    WHERE IsDeleted = 0
        AND (@SearchTerm IS NULL 
            OR Email LIKE '%' + @SearchTerm + '%'
            OR UserName LIKE '%' + @SearchTerm + '%'
            OR FirstName LIKE '%' + @SearchTerm + '%'
            OR LastName LIKE '%' + @SearchTerm + '%'
            OR Phone LIKE '%' + @SearchTerm + '%')
    ORDER BY Id
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END