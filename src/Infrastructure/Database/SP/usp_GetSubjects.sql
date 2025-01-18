Create OR ALTER PROCEDURE usp_GetSubjects
@PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
     SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY S.Id) AS RowNumber,
        S.Id, S.SubjectName, S.CurrentStatus,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.Subjects S
    WHERE S.IsDeleted = 0
        AND (@SearchTerm IS NULL 
            OR S.SubjectName LIKE '%' + @SearchTerm + '%')
    ORDER BY Id
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
