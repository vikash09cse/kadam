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
    DECLARE @InstitutionIds VARCHAR(MAX) = NULL;
    DECLARE @FilterByInstitution BIT = 0;
    DECLARE @FilterByDivision BIT = 0;
    DECLARE @FilterByCreatedBy BIT = 0;

    DECLARE @AssignedDivisions TABLE (DivisionId INT NOT NULL PRIMARY KEY);

    IF @UserId IS NOT NULL AND @UserId > 0
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM dbo.Users u
            INNER JOIN dbo.Roles r ON u.RoleId = r.Id AND r.IsDeleted = 0
            WHERE u.Id = @UserId
              AND u.IsDeleted = 0
              AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
        )
        BEGIN
            SET @FilterByInstitution = 0;
            SET @FilterByDivision = 0;
            SET @FilterByCreatedBy = 0;
        END
        ELSE IF OBJECT_ID(N'dbo.PeopleDivisions', N'U') IS NOT NULL
        BEGIN
            IF EXISTS (SELECT 1 FROM dbo.PeopleDivisions WHERE UserId = @UserId)
            BEGIN
                SET @FilterByDivision = 1;
                INSERT INTO @AssignedDivisions (DivisionId)
                SELECT DISTINCT pd.DivisionId
                FROM dbo.PeopleDivisions pd
                WHERE pd.UserId = @UserId;
            END
            ELSE IF EXISTS (
                SELECT 1
                FROM dbo.PeopleInstitutions pi
                WHERE pi.UserId = @UserId
                  AND LTRIM(RTRIM(ISNULL(pi.InstitutionIds, ''))) <> ''
            )
            BEGIN
                SET @FilterByInstitution = 1;
                SELECT @InstitutionIds = STRING_AGG(CAST(LTRIM(RTRIM(s.Item)) AS VARCHAR(20)), ',')
                FROM dbo.PeopleInstitutions pi
                CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') s
                WHERE pi.UserId = @UserId
                  AND LTRIM(RTRIM(ISNULL(pi.InstitutionIds, ''))) <> ''
                  AND TRY_CAST(LTRIM(RTRIM(s.Item)) AS INT) IS NOT NULL;
            END
            ELSE
            BEGIN
                SET @FilterByCreatedBy = 1;
            END
        END
        ELSE IF EXISTS (
            SELECT 1
            FROM dbo.PeopleInstitutions pi
            WHERE pi.UserId = @UserId
              AND LTRIM(RTRIM(ISNULL(pi.InstitutionIds, ''))) <> ''
        )
        BEGIN
            SET @FilterByInstitution = 1;
            SELECT @InstitutionIds = STRING_AGG(CAST(LTRIM(RTRIM(s.Item)) AS VARCHAR(20)), ',')
            FROM dbo.PeopleInstitutions pi
            CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') s
            WHERE pi.UserId = @UserId
              AND LTRIM(RTRIM(ISNULL(pi.InstitutionIds, ''))) <> ''
              AND TRY_CAST(LTRIM(RTRIM(s.Item)) AS INT) IS NOT NULL;
        END
        ELSE
        BEGIN
            SET @FilterByCreatedBy = 1;
        END
    END

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
          (@FilterByInstitution = 0 AND @FilterByDivision = 0 AND @FilterByCreatedBy = 0)
          OR (
              @FilterByDivision = 1
              AND i.Id IS NOT NULL
              AND i.DivisionId IN (SELECT ad.DivisionId FROM @AssignedDivisions ad)
          )
          OR (
              @FilterByInstitution = 1
              AND @InstitutionIds IS NOT NULL
              AND LTRIM(RTRIM(@InstitutionIds)) <> ''
              AND s.InstitutionId IN (
                  SELECT TRY_CAST(LTRIM(RTRIM(Item)) AS INT)
                  FROM dbo.SplitString(@InstitutionIds, ',')
                  WHERE TRY_CAST(LTRIM(RTRIM(Item)) AS INT) IS NOT NULL
              )
          )
          OR (
              @FilterByCreatedBy = 1
              AND s.CreatedBy = @UserId
          )
      )
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
GO
