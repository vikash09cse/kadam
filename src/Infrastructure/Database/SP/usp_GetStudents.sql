CREATE OR ALTER PROCEDURE dbo.usp_GetStudents
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @StudentName NVARCHAR(100) = NULL,
    @StudentId NVARCHAR(50) = NULL,
    @UserId INT,
    @Status INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @PageNumber = CASE WHEN @PageNumber < 1 THEN 1 ELSE @PageNumber END;
    SET @PageSize = CASE WHEN @PageSize < 1 THEN 10 WHEN @PageSize > 100 THEN 100 ELSE @PageSize END;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @IsAdmin BIT = 0;
    DECLARE @HasExplicitScope BIT = 0;
    DECLARE @Name NVARCHAR(100) = NULLIF(LTRIM(RTRIM(@StudentName)), '');
    DECLARE @Code NVARCHAR(50) = NULLIF(LTRIM(RTRIM(@StudentId)), '');

    SELECT @IsAdmin = CASE WHEN LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin' THEN 1 ELSE 0 END
    FROM dbo.Users u
    INNER JOIN dbo.Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
    WHERE u.Id = @UserId AND u.IsDeleted = 0;

    IF EXISTS (SELECT 1 FROM dbo.PeopleDivisions WHERE UserId = @UserId)
       OR EXISTS (SELECT 1 FROM dbo.PeopleInstitutions WHERE UserId = @UserId)
        SET @HasExplicitScope = 1;

    CREATE TABLE #AllowedInstitutions
    (
        InstitutionId INT NOT NULL PRIMARY KEY
    );

    IF @IsAdmin = 0 AND @HasExplicitScope = 1
    BEGIN
        INSERT INTO #AllowedInstitutions (InstitutionId)
        SELECT DISTINCT i.Id
        FROM dbo.Institutions i
        INNER JOIN dbo.PeopleDivisions pd
            ON pd.DivisionId = i.DivisionId AND pd.UserId = @UserId
        WHERE i.IsDeleted = 0;

        INSERT INTO #AllowedInstitutions (InstitutionId)
        SELECT DISTINCT parsed.InstitutionId
        FROM dbo.PeopleInstitutions pi
        CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split
        CROSS APPLY (VALUES (TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT))) parsed(InstitutionId)
        WHERE pi.UserId = @UserId
          AND parsed.InstitutionId IS NOT NULL
          AND NOT EXISTS (
              SELECT 1
              FROM #AllowedInstitutions allowed
              WHERE allowed.InstitutionId = parsed.InstitutionId
          );
    END;

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
      AND (
          @IsAdmin = 1
          OR EXISTS (
              SELECT 1 FROM #AllowedInstitutions allowed
              WHERE allowed.InstitutionId = s.InstitutionId
          )
          OR (@HasExplicitScope = 0 AND s.CreatedBy = @UserId)
      )
      AND (@Status IS NULL OR s.CurrentStatus = @Status)
      AND (
          @Name IS NULL
          OR s.FirstName LIKE '%' + @Name + '%'
          OR s.LastName LIKE '%' + @Name + '%'
          OR CONCAT(s.FirstName, ' ', s.LastName) LIKE '%' + @Name + '%'
      )
      AND (
          @Code IS NULL
          OR s.StudentId LIKE '%' + @Code + '%'
      )
    ORDER BY s.EnrollmentDate DESC, s.Id DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY
    OPTION (RECOMPILE);
END
GO
