CREATE OR ALTER PROCEDURE dbo.usp_GetUserList
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY u.Id) AS RowNumber,
         u.Id, Email, UserName, FirstName, LastName, 
        Phone, Gender, 
        r.RoleName AS RoleName, rr.RoleName AS ReporteeRoleName,
        COUNT(*) OVER() AS TotalCount
    FROM Users u 
    LEFT JOIN Roles r ON u.RoleId = r.Id
    LEFT JOIN Roles rr ON u.ReporteeRoleId = rr.Id
    WHERE u.IsDeleted = 0
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