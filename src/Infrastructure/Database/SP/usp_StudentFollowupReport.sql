CREATE OR ALTER PROCEDURE dbo.usp_StudentFollowupReport
    @UserId INT = NULL,
    @StateId INT = NULL,
    @DivisionId INT = NULL,
    @InstitutionId INT = NULL,
    @GradeId INT = NULL,
    @Section VARCHAR(25) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
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
        ROW_NUMBER() OVER (ORDER BY sf.FollowupDate DESC, sf.Id DESC) AS RowNumber,
        sf.Id,
        sf.FollowupDate,
        sf.InstitutionId,
        i.InstitutionName,
        sf.GradeId,
        g.GradeName,
        ISNULL(sf.Section, '') AS Section,
        ISNULL(sf.InchargeName, '') AS InchargeName,
        ISNULL(sf.InchargeContactNumber, '') AS InchargeContactNumber,
        ISNULL(sf.IsChildSitTogether, '') AS IsChildSitTogether,
        sf.LastMonthAttendanceCount,
        sf.LastMonthWorkingDayCount,
        sf.LastMonthAttendancePercentage,
        sf.MaleStudentCount,
        sf.FemaleStudentCount,
        sf.TodayStudentPresentCount,
        sf.TotalStudentCount,
        sf.TotalStudentPercentage,
        sf.CreatedBy,
        LTRIM(RTRIM(CONCAT(ISNULL(u.FirstName, ''), ' ', ISNULL(u.LastName, '')))) AS CreatedByName
    FROM dbo.StudentFollowups sf
    INNER JOIN dbo.Institutions i ON i.Id = sf.InstitutionId AND i.IsDeleted = 0
    INNER JOIN dbo.Grades g ON g.Id = sf.GradeId
    LEFT JOIN dbo.Users u ON u.Id = sf.CreatedBy
    WHERE ISNULL(sf.IsDeleted, 0) = 0
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
              AND i.Id IN (
                  SELECT TRY_CAST(LTRIM(RTRIM(Item)) AS INT)
                  FROM dbo.SplitString(@InstitutionIds, ',')
                  WHERE TRY_CAST(LTRIM(RTRIM(Item)) AS INT) IS NOT NULL
              )
          )
      )
      AND (@StateId IS NULL OR @StateId = 0 OR i.StateId = @StateId)
      AND (@DivisionId IS NULL OR @DivisionId = 0 OR i.DivisionId = @DivisionId)
      AND (@InstitutionId IS NULL OR @InstitutionId = 0 OR sf.InstitutionId = @InstitutionId)
      AND (@GradeId IS NULL OR @GradeId = 0 OR sf.GradeId = @GradeId)
      AND (
          @Section IS NULL
          OR LTRIM(RTRIM(@Section)) = ''
          OR sf.Section = LTRIM(RTRIM(@Section))
      )
      AND (@FromDate IS NULL OR CAST(sf.FollowupDate AS DATE) >= @FromDate)
      AND (@ToDate IS NULL OR CAST(sf.FollowupDate AS DATE) <= @ToDate)
    ORDER BY sf.FollowupDate DESC, sf.Id DESC;
END
GO
