Create OR ALTER PROCEDURE usp_GetProjects
@PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
     SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY P.Id) AS RowNumber,
        P.Id, P.ProjectName, P.StartDate, P.EndDate, P.CurrentStatus,
        STRING_AGG(PP.ProgramId, ',') AS ProjectPrograms,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.Projects P
    LEFT JOIN dbo.ProjectPrograms PP ON P.Id = PP.ProjectId
    WHERE P.IsDeleted = 0
        AND (@SearchTerm IS NULL 
            OR P.ProjectName LIKE '%' + @SearchTerm + '%')
    GROUP BY P.Id, P.ProjectName, P.StartDate, P.EndDate, P.CurrentStatus
    ORDER BY P.Id
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
