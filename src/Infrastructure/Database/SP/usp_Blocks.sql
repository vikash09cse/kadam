Create or alter procedure dbo.usp_Blocks
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY B.Id) AS RowNumber,
        B.Id, B.BlockName, D.DistrictName, S.StateName, B.CurrentStatus,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.Blocks B INNER JOIN dbo.Districts D ON B.DistrictId = D.Id
    INNER JOIN dbo.States S ON B.StateId = S.Id
    WHERE B.IsDeleted = 0
        AND (@SearchTerm IS NULL 
            OR B.BlockName LIKE '%' + @SearchTerm + '%'
            OR D.DistrictName LIKE '%' + @SearchTerm + '%'
            OR S.StateName LIKE '%' + @SearchTerm + '%')
    ORDER BY B.Id
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END

