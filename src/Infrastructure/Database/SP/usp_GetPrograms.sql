Create OR ALTER PROCEDURE usp_GetPrograms
@PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
     SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY P.Id) AS RowNumber,
        P.Id, P.ProgramName, P.CurrentStatus,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.Programs P
    WHERE P.IsDeleted = 0
        AND (@SearchTerm IS NULL 
            OR P.ProgramName LIKE '%' + @SearchTerm + '%')
    ORDER BY Id
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END