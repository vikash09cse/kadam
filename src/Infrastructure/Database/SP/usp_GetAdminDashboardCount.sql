CREATE OR ALTER PROCEDURE dbo.usp_GetAdminDashboardCount
    @UserId INT,
    @StateId INT = NULL,
    @DivisionIds VARCHAR(2000) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @IncludeAll BIT = 1,
    @IncludeKadam BIT = 0,
    @IncludeKadamPlus BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @InstitutionIds VARCHAR(MAX) = NULL;
    DECLARE @FilterByInstitution BIT = 0;
    DECLARE @FilterByDivision BIT = 0;

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
        END
        ELSE IF EXISTS (SELECT 1 FROM dbo.PeopleDivisions WHERE UserId = @UserId)
        BEGIN
            SET @FilterByDivision = 1;
        END
        ELSE
        BEGIN
            SET @FilterByInstitution = 1;
            SELECT @InstitutionIds = STRING_AGG(CAST(LTRIM(RTRIM(s.Item)) AS VARCHAR(20)), ',')
            FROM dbo.PeopleInstitutions pi
            CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') s
            WHERE pi.UserId = @UserId
              AND LTRIM(RTRIM(ISNULL(pi.InstitutionIds, ''))) <> ''
              AND TRY_CAST(LTRIM(RTRIM(s.Item)) AS INT) IS NOT NULL;
        END
    END

    SELECT
        ISNULL(SUM(CASE WHEN s.CurrentStatus = 1 THEN 1 ELSE 0 END), 0) AS ActiveCount,
        ISNULL(SUM(CASE WHEN s.CurrentStatus = 2 THEN 1 ELSE 0 END), 0) AS InactiveCount,
        ISNULL(SUM(CASE WHEN s.CurrentStatus = 3 THEN 1 ELSE 0 END), 0) AS CompletedCount
    FROM dbo.Students s
    INNER JOIN dbo.Institutions i ON s.InstitutionId = i.Id AND i.IsDeleted = 0
    WHERE s.IsDeleted = 0
      AND (
          (@FilterByInstitution = 0 AND @FilterByDivision = 0)
          OR (
              @FilterByDivision = 1
              AND i.DivisionId IN (
                  SELECT pd.DivisionId
                  FROM dbo.PeopleDivisions pd
                  WHERE pd.UserId = @UserId
              )
          )
          OR (
              @FilterByInstitution = 1
              AND @InstitutionIds IS NOT NULL
              AND LTRIM(RTRIM(@InstitutionIds)) <> ''
              AND i.Id IN (SELECT TRY_CAST(Item AS INT) FROM dbo.SplitString(@InstitutionIds, ',') WHERE TRY_CAST(Item AS INT) IS NOT NULL)
          )
      )
      AND (@StateId IS NULL OR @StateId = 0 OR i.StateId = @StateId)
      AND (
          @DivisionIds IS NULL
          OR LTRIM(RTRIM(@DivisionIds)) = ''
          OR i.DivisionId IN (
              SELECT TRY_CAST(LTRIM(RTRIM(Item)) AS INT)
              FROM dbo.SplitString(@DivisionIds, ',')
              WHERE TRY_CAST(LTRIM(RTRIM(Item)) AS INT) IS NOT NULL
          )
      )
      AND (@FromDate IS NULL OR s.EnrollmentDate >= @FromDate)
      AND (@ToDate IS NULL OR s.EnrollmentDate <= @ToDate)
      AND (
          @IncludeAll = 1
          OR (
              (@IncludeKadam = 1 AND s.IsKadamPlusStudent = 0)
              OR (@IncludeKadamPlus = 1 AND s.IsKadamPlusStudent = 1)
          )
      );
END
GO
