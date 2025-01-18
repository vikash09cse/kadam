Create Or Alter Procedure usp_GetInstitutions
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @CurrentStatus INT = NULL,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY Id) AS RowNumber,
        Id, InstitutionType, InstitutionName, InstitutionCode, InstitutionId,
        InstitutionHeadMasterName, InstitutionPhone, FinancialYearStart, FinancialYearEnd, CurrentStatus,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.Institutions
    WHERE IsDeleted = 0 
        AND (@CurrentStatus IS NULL OR CurrentStatus = @CurrentStatus)
        AND (@SearchTerm IS NULL 
            OR InstitutionName LIKE '%' + @SearchTerm + '%'
            OR InstitutionCode LIKE '%' + @SearchTerm + '%'
            OR InstitutionId LIKE '%' + @SearchTerm + '%'
            OR InstitutionHeadMasterName LIKE '%' + @SearchTerm + '%'
            OR InstitutionPhone LIKE '%' + @SearchTerm + '%' )
    ORDER BY Id
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END