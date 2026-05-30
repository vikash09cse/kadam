CREATE OR ALTER PROCEDURE dbo.usp_GetStudents
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @StudentName NVARCHAR(100) = NULL,
    @StudentId NVARCHAR(50) = NULL,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @FilterByUser BIT = 1;

    IF EXISTS (
        SELECT 1
        FROM dbo.Users u
        INNER JOIN dbo.Roles r ON u.RoleId = r.Id AND r.IsDeleted = 0
        WHERE u.Id = @UserId
          AND u.IsDeleted = 0
          AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
    )
        SET @FilterByUser = 0;

    SELECT
        ROW_NUMBER() OVER (ORDER BY s.EnrollmentDate DESC, s.Id DESC) AS RowNumber,
        s.Id,
        ISNULL(s.StudentId, '') AS StudentId,
        LTRIM(RTRIM(CONCAT(s.FirstName, ' ', s.LastName))) AS StudentName,
        ISNULL(i.InstitutionName, '') AS InstitutionName,
        ISNULL(g.GradeName, '') AS GradeName,
        ISNULL(s.Section, '') AS Section,
        s.Age,
        s.EnrollmentDate,
        ISNULL(s.StudentRegistratioNumber, '') AS StudentRegistratioNumber,
        s.CurrentStatus,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.Students s
    LEFT JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND i.IsDeleted = 0
    LEFT JOIN dbo.Grades g ON g.Id = s.GradeId
    WHERE s.IsDeleted = 0
      AND (@FilterByUser = 0 OR s.CreatedBy = @UserId)
      AND (
          @StudentName IS NULL
          OR LTRIM(RTRIM(@StudentName)) = ''
          OR s.FirstName LIKE '%' + LTRIM(RTRIM(@StudentName)) + '%'
          OR s.LastName LIKE '%' + LTRIM(RTRIM(@StudentName)) + '%'
          OR CONCAT(s.FirstName, ' ', s.LastName) LIKE '%' + LTRIM(RTRIM(@StudentName)) + '%'
      )
      AND (
          @StudentId IS NULL
          OR LTRIM(RTRIM(@StudentId)) = ''
          OR s.StudentId LIKE '%' + LTRIM(RTRIM(@StudentId)) + '%'
      )
    ORDER BY s.EnrollmentDate DESC, s.Id DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
