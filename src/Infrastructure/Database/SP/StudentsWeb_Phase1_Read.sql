CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_HasPageAccess
    @UserId INT,
    @PageUrl VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM dbo.Users u
        INNER JOIN dbo.Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
        WHERE u.Id = @UserId AND u.IsDeleted = 0
          AND (
              LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
              OR EXISTS (
                  SELECT 1
                  FROM dbo.UserMenuPermissions ump
                  INNER JOIN dbo.MenuPermissions mp ON mp.Id = ump.MenuId
                  WHERE ump.UserId = @UserId
                    AND ump.IsDeleted = 0
                    AND mp.IsDeleted = 0
                    AND mp.CurrentStatus = 1
                    AND LOWER(LTRIM(RTRIM(mp.MenuUrl))) = LOWER(LTRIM(RTRIM(@PageUrl)))
              )
              OR EXISTS (
                  SELECT 1
                  FROM dbo.RolePermissions rp
                  INNER JOIN dbo.MenuPermissions mp ON mp.Id = rp.MenuId
                  WHERE rp.RoleId = u.RoleId
                    AND rp.IsDeleted = 0
                    AND rp.CurrentStatus = 1
                    AND mp.IsDeleted = 0
                    AND mp.CurrentStatus = 1
                    AND LOWER(LTRIM(RTRIM(mp.MenuUrl))) = LOWER(LTRIM(RTRIM(@PageUrl)))
              )
          )
    ) THEN 1 ELSE 0 END AS BIT);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_CanAccessInstitution
    @InstitutionId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM dbo.Institutions i
        WHERE i.Id = @InstitutionId AND i.IsDeleted = 0
          AND (
              EXISTS (
                  SELECT 1 FROM dbo.Users u
                  INNER JOIN dbo.Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
                  WHERE u.Id = @UserId AND u.IsDeleted = 0
                    AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
              )
              OR EXISTS (
                  SELECT 1 FROM dbo.PeopleDivisions pd
                  WHERE pd.UserId = @UserId AND pd.DivisionId = i.DivisionId
              )
              OR EXISTS (
                  SELECT 1 FROM dbo.PeopleInstitutions pi
                  CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split
                  WHERE pi.UserId = @UserId
                    AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = i.Id
              )
          )
    ) THEN 1 ELSE 0 END AS BIT);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_CanAccessStudent
    @StudentId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM dbo.Students s
        INNER JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND i.IsDeleted = 0
        WHERE s.Id = @StudentId AND s.IsDeleted = 0
          AND (
              EXISTS (
                  SELECT 1 FROM dbo.Users u
                  INNER JOIN dbo.Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
                  WHERE u.Id = @UserId AND u.IsDeleted = 0
                    AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
              )
              OR EXISTS (
                  SELECT 1 FROM dbo.PeopleDivisions pd
                  WHERE pd.UserId = @UserId AND pd.DivisionId = i.DivisionId
              )
              OR EXISTS (
                  SELECT 1 FROM dbo.PeopleInstitutions pi
                  CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split
                  WHERE pi.UserId = @UserId
                    AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = s.InstitutionId
              )
              OR (
                  NOT EXISTS (SELECT 1 FROM dbo.PeopleDivisions pd WHERE pd.UserId = @UserId)
                  AND NOT EXISTS (SELECT 1 FROM dbo.PeopleInstitutions pi WHERE pi.UserId = @UserId)
                  AND s.CreatedBy = @UserId
              )
          )
    ) THEN 1 ELSE 0 END AS BIT);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetDashboard
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH AllowedStudents AS (
        SELECT s.CurrentStatus
        FROM dbo.Students s
        INNER JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND i.IsDeleted = 0
        WHERE s.IsDeleted = 0
          AND (
              EXISTS (
                  SELECT 1 FROM dbo.Users u
                  INNER JOIN dbo.Roles r ON r.Id = u.RoleId AND r.IsDeleted = 0
                  WHERE u.Id = @UserId AND u.IsDeleted = 0
                    AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
              )
              OR EXISTS (SELECT 1 FROM dbo.PeopleDivisions pd WHERE pd.UserId = @UserId AND pd.DivisionId = i.DivisionId)
              OR EXISTS (
                  SELECT 1 FROM dbo.PeopleInstitutions pi
                  CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split
                  WHERE pi.UserId = @UserId AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = s.InstitutionId
              )
              OR (
                  NOT EXISTS (SELECT 1 FROM dbo.PeopleDivisions pd WHERE pd.UserId = @UserId)
                  AND NOT EXISTS (SELECT 1 FROM dbo.PeopleInstitutions pi WHERE pi.UserId = @UserId)
                  AND s.CreatedBy = @UserId
              )
          )
    )
    SELECT
        ISNULL(SUM(CASE WHEN CurrentStatus = 1 THEN 1 ELSE 0 END), 0) ActiveCount,
        ISNULL(SUM(CASE WHEN CurrentStatus = 2 THEN 1 ELSE 0 END), 0) InactiveCount,
        ISNULL(SUM(CASE WHEN CurrentStatus = 3 THEN 1 ELSE 0 END), 0) CompletedCount
    FROM AllowedStudents;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetStudents
    @UserId INT,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @StudentName NVARCHAR(100) = NULL,
    @StudentId NVARCHAR(50) = NULL,
    @AadhaarNumber VARCHAR(16) = NULL,
    @Status INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @PageNumber = CASE WHEN @PageNumber < 1 THEN 1 ELSE @PageNumber END;
    SET @PageSize = CASE WHEN @PageSize < 1 THEN 10 WHEN @PageSize > 100 THEN 100 ELSE @PageSize END;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @IsAdmin BIT = 0;
    DECLARE @HasExplicitScope BIT = 0;

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

    ;WITH CurrentTrios AS (
        SELECT StudentId, MAX(TrioId) TrioId
        FROM dbo.StudentTrios
        WHERE IsDeleted = 0
        GROUP BY StudentId
    ),
    Filtered AS (
        SELECT
            s.Id, s.StudentId, s.FirstName, s.LastName, s.AadhaarCardNumber,
            s.Section, s.Age, s.EnrollmentDate, s.CurrentStatus,
            s.InActiveReason, s.InActiveDate, s.Remarks,
            s.DateEntryPoint, i.InstitutionName, g.GradeName, st.TrioId,
            CAST(CASE WHEN EXISTS (
                SELECT 1
                FROM dbo.StudentBaselineDetails baseline
                WHERE baseline.StudentId = s.Id
                  AND baseline.IsDeleted = 0
                  AND baseline.BaselineType = 'baselinepreAssessment'
            ) THEN 1 ELSE 0 END AS BIT) IsBaselineAdded,
            CAST(CASE WHEN EXISTS (
                SELECT 1 FROM dbo.StudentBaselineDetails baseline
                WHERE baseline.StudentId = s.Id AND baseline.IsDeleted = 0
                  AND baseline.BaselineType = 'baselinepreAssessment'
                  AND baseline.CompletedDate IS NOT NULL
            ) THEN 1 ELSE 0 END AS BIT) IsBaselineCompleted,
            CAST(CASE WHEN EXISTS (
                SELECT 1 FROM dbo.StudentBaselineDetails endline
                WHERE endline.StudentId = s.Id AND endline.IsDeleted = 0
                  AND endline.BaselineType = 'endlinepreAssessment'
                  AND endline.CompletedDate IS NOT NULL
            ) THEN 1 ELSE 0 END AS BIT) IsEndlineCompleted,
            CAST(CASE WHEN EXISTS (
                SELECT 1 FROM dbo.StudentMainstreams mainstream
                WHERE mainstream.StudentId = s.Id
            ) THEN 1 ELSE 0 END AS BIT) HasMainstream,
            CAST(CASE WHEN s.IsKadamPlusStudent = 0 AND s.CurrentStatus IN (1, 2)
                AND EXISTS (
                    SELECT 1 FROM dbo.StudentBaselineDetails baseline
                    WHERE baseline.StudentId = s.Id AND baseline.IsDeleted = 0
                      AND baseline.BaselineType = 'baselinepreAssessment'
                      AND baseline.CompletedDate IS NOT NULL
                )
                AND EXISTS (
                    SELECT 1 FROM dbo.StudentBaselineDetails endline
                    WHERE endline.StudentId = s.Id AND endline.IsDeleted = 0
                      AND endline.BaselineType = 'endlinepreAssessment'
                      AND endline.CompletedDate IS NOT NULL
                )
                AND NOT EXISTS (
                    SELECT 1 FROM dbo.StudentMainstreams mainstream
                    WHERE mainstream.StudentId = s.Id
                )
            THEN 1 ELSE 0 END AS BIT) IsMainstreamEligible
        FROM dbo.Students s
        INNER JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND i.IsDeleted = 0
        LEFT JOIN dbo.Grades g ON g.Id = s.GradeId AND g.IsDeleted = 0
        LEFT JOIN CurrentTrios st ON st.StudentId = s.Id
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
          AND (@StudentId IS NULL OR s.StudentId LIKE '%' + @StudentId + '%')
          AND (@AadhaarNumber IS NULL OR s.AadhaarCardNumber LIKE '%' + @AadhaarNumber + '%')
          AND (
              @StudentName IS NULL
              OR s.FirstName LIKE '%' + @StudentName + '%'
              OR s.LastName LIKE '%' + @StudentName + '%'
              OR CONCAT(s.FirstName, ' ', s.LastName) LIKE '%' + @StudentName + '%'
          )
    )
    SELECT
        ROW_NUMBER() OVER (ORDER BY EnrollmentDate DESC, Id DESC) RowNumber,
        Id,
        ISNULL(StudentId, '') StudentId,
        LTRIM(RTRIM(CONCAT(FirstName, ' ', LastName))) StudentName,
        InstitutionName,
        ISNULL(GradeName, '') GradeName,
        ISNULL(Section, '') Section,
        Age,
        EnrollmentDate,
        CurrentStatus,
        InActiveReason,
        InActiveDate,
        Remarks,
        TrioId,
        IsBaselineAdded,
        IsBaselineCompleted,
        IsEndlineCompleted,
        HasMainstream,
        IsMainstreamEligible,
        DateEntryPoint,
        COUNT(*) OVER() TotalCount
    FROM Filtered
    ORDER BY EnrollmentDate DESC, Id DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
    OPTION (RECOMPILE);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetStudent
    @Id INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Students s
        INNER JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND i.IsDeleted = 0
        WHERE s.Id = @Id AND s.IsDeleted = 0
          AND (
              EXISTS (
                  SELECT 1 FROM dbo.Users u INNER JOIN dbo.Roles r ON r.Id = u.RoleId
                  WHERE u.Id = @UserId AND u.IsDeleted = 0 AND r.IsDeleted = 0
                    AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
              )
              OR EXISTS (SELECT 1 FROM dbo.PeopleDivisions pd WHERE pd.UserId = @UserId AND pd.DivisionId = i.DivisionId)
              OR EXISTS (
                  SELECT 1 FROM dbo.PeopleInstitutions pi CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split
                  WHERE pi.UserId = @UserId AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = s.InstitutionId
              )
              OR (
                  NOT EXISTS (SELECT 1 FROM dbo.PeopleDivisions pd WHERE pd.UserId = @UserId)
                  AND NOT EXISTS (SELECT 1 FROM dbo.PeopleInstitutions pi WHERE pi.UserId = @UserId)
                  AND s.CreatedBy = @UserId
              )
          )
    ) RETURN;

    SELECT
        s.Id, s.StudentId, s.EnrollmentDate, s.FirstName, s.LastName, s.GenderId,
        s.DateOfBirth, s.Age, s.DoYouHaveAadhaarCard, s.AadhaarCardNumber,
        s.InstitutionId, s.GradeId, s.Section, s.StudentRegistratioNumber,
        s.ChildStatudBeforeKadamSTC, s.HowLongPlaningToStayThisArea, s.Class,
        s.ReasonId, s.DropoutClass, s.DropoutYear, s.IsKadamPlusStudent,
        s.ProfilePicturePath, s.CurrentStatus, s.DateEntryPoint,
        CAST(CASE WHEN EXISTS (
            SELECT 1 FROM dbo.StudentBaselineDetails baseline
            WHERE baseline.StudentId = s.Id AND baseline.IsDeleted = 0
        ) THEN 1 ELSE 0 END AS BIT) IsBaselineAdded,
        f.Id FamilyId, f.FatherName, f.FatherAge, f.FatherOccupationId,
        f.FatherEducationId, f.MotherName, f.MotherAge, f.MotherOccupationId,
        f.MotherEducationId, f.PrimaryContactNumber, f.AlternateContactNumber,
        f.HouseAddress, f.PinCode, f.PeopleInHouseId, f.CasteId, f.ReligionId,
        CONVERT(VARCHAR(50), f.ParentMonthlyIncome) ParentMonthlyIncome,
        CONVERT(VARCHAR(50), f.ParentMontlyExpenditure) ParentMontlyExpenditure
    FROM dbo.Students s
    LEFT JOIN dbo.StudentFamilyDetails f ON f.StudentId = s.Id AND f.IsDeleted = 0
    WHERE s.Id = @Id AND s.IsDeleted = 0;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetInstitutions
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT i.Id, i.InstitutionName Text
    FROM dbo.Institutions i
    WHERE i.IsDeleted = 0 AND i.CurrentStatus = 1
      AND (
          EXISTS (
              SELECT 1 FROM dbo.Users u INNER JOIN dbo.Roles r ON r.Id = u.RoleId
              WHERE u.Id = @UserId AND u.IsDeleted = 0 AND r.IsDeleted = 0
                AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
          )
          OR EXISTS (SELECT 1 FROM dbo.PeopleDivisions pd WHERE pd.UserId = @UserId AND pd.DivisionId = i.DivisionId)
          OR EXISTS (
              SELECT 1 FROM dbo.PeopleInstitutions pi CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split
              WHERE pi.UserId = @UserId AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = i.Id
          )
      )
    ORDER BY Text;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetGradeSections
    @InstitutionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT g.Id, g.GradeName Text, igs.InstitutionId ParentId, igs.Sections
    FROM dbo.InstitutionGradeSections igs
    INNER JOIN dbo.Grades g ON g.Id = igs.GradeId AND g.IsDeleted = 0
    WHERE igs.InstitutionId = @InstitutionId
    ORDER BY g.Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetHealth
    @StudentId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (
        SELECT 1 FROM dbo.Students s
        INNER JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND i.IsDeleted = 0
        WHERE s.Id = @StudentId AND s.IsDeleted = 0
          AND (
              EXISTS (SELECT 1 FROM dbo.Users u INNER JOIN dbo.Roles r ON r.Id = u.RoleId WHERE u.Id = @UserId AND u.IsDeleted = 0 AND r.IsDeleted = 0 AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin')
              OR EXISTS (SELECT 1 FROM dbo.PeopleDivisions pd WHERE pd.UserId = @UserId AND pd.DivisionId = i.DivisionId)
              OR EXISTS (SELECT 1 FROM dbo.PeopleInstitutions pi CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split WHERE pi.UserId = @UserId AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = s.InstitutionId)
              OR (NOT EXISTS (SELECT 1 FROM dbo.PeopleDivisions pd WHERE pd.UserId = @UserId) AND NOT EXISTS (SELECT 1 FROM dbo.PeopleInstitutions pi WHERE pi.UserId = @UserId) AND s.CreatedBy = @UserId)
          )
    ) RETURN;

    SELECT h.Id, s.Id StudentId, LTRIM(RTRIM(CONCAT(s.FirstName, ' ', s.LastName))) StudentName,
        h.PhysicallyChallenged, h.PhysicallyChallengedType,
        h.PercentagePhysicallyChallenged, h.DisabilityCertificatePath,
        h.DisabilityCertificateFileName, h.DateEntryPoint
    FROM dbo.Students s
    LEFT JOIN dbo.StudentHealths h ON h.StudentId = s.Id AND h.IsDeleted = 0
    WHERE s.Id = @StudentId AND s.IsDeleted = 0;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetDocuments
    @StudentId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (
        SELECT 1 FROM dbo.Students s
        INNER JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND i.IsDeleted = 0
        WHERE s.Id = @StudentId AND s.IsDeleted = 0
          AND (
              EXISTS (SELECT 1 FROM dbo.Users u INNER JOIN dbo.Roles r ON r.Id = u.RoleId WHERE u.Id = @UserId AND u.IsDeleted = 0 AND r.IsDeleted = 0 AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin')
              OR EXISTS (SELECT 1 FROM dbo.PeopleDivisions pd WHERE pd.UserId = @UserId AND pd.DivisionId = i.DivisionId)
              OR EXISTS (SELECT 1 FROM dbo.PeopleInstitutions pi CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') split WHERE pi.UserId = @UserId AND TRY_CAST(LTRIM(RTRIM(split.Item)) AS INT) = s.InstitutionId)
              OR (NOT EXISTS (SELECT 1 FROM dbo.PeopleDivisions pd WHERE pd.UserId = @UserId) AND NOT EXISTS (SELECT 1 FROM dbo.PeopleInstitutions pi WHERE pi.UserId = @UserId) AND s.CreatedBy = @UserId)
          )
    ) RETURN;

    SELECT d.Id, s.Id StudentId, LTRIM(RTRIM(CONCAT(s.FirstName, ' ', s.LastName))) StudentName,
        d.DocumentTypeId, d.DocumentNumber, d.DocumentPath, d.DocumentFileName, d.DateEntryPoint
    FROM dbo.Students s
    INNER JOIN dbo.StudentDocuments d ON d.StudentId = s.Id AND d.IsDeleted = 0
    WHERE s.Id = @StudentId AND s.IsDeleted = 0
    ORDER BY d.Id DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_CheckTrioCapacity
    @StudentId INT,
    @TrioId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @InstitutionId INT, @GradeId INT;
    SELECT @InstitutionId = InstitutionId, @GradeId = GradeId
    FROM dbo.Students
    WHERE Id = @StudentId AND IsDeleted = 0;

    IF @InstitutionId IS NULL OR @GradeId IS NULL OR @TrioId <= 0
    BEGIN
        SELECT CAST(0 AS BIT);
        RETURN;
    END;

    DECLARE @DestinationCount INT;
    SELECT @DestinationCount = COUNT(1)
    FROM dbo.StudentTrios st
    INNER JOIN dbo.Students s ON s.Id = st.StudentId AND s.IsDeleted = 0
    WHERE st.TrioId = @TrioId
      AND st.StudentId <> @StudentId
      AND st.IsDeleted = 0
      AND s.InstitutionId = @InstitutionId
      AND s.GradeId = @GradeId;

    DECLARE @FullTrioCount INT;
    SELECT @FullTrioCount = COUNT(1)
    FROM (
        SELECT st.TrioId
        FROM dbo.StudentTrios st
        INNER JOIN dbo.Students s ON s.Id = st.StudentId AND s.IsDeleted = 0
        WHERE st.StudentId <> @StudentId
          AND st.IsDeleted = 0
          AND s.InstitutionId = @InstitutionId
          AND s.GradeId = @GradeId
        GROUP BY st.TrioId
        HAVING COUNT(1) >= 4
    ) fullTrios;

    SELECT CAST(CASE
        WHEN @DestinationCount >= 4 THEN 0
        WHEN @DestinationCount = 3 AND @FullTrioCount >= 2 THEN 0
        ELSE 1
    END AS BIT);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_RegistrationNumberExists
    @RegistrationNumber VARCHAR(50),
    @InstitutionId INT,
    @ExceptId INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1 FROM dbo.Students
        WHERE StudentRegistratioNumber = @RegistrationNumber
          AND InstitutionId = @InstitutionId
          AND Id <> @ExceptId
          AND IsDeleted = 0
    ) THEN 1 ELSE 0 END AS BIT);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_GetMainstream
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.Id StudentId,
        LTRIM(RTRIM(CONCAT(s.FirstName, ' ', s.LastName))) StudentName,
        ISNULL(s.StudentId, '') StudentCode,
        s.InstitutionId EnrolledInstitutionId,
        ISNULL(i.InstitutionName, '') EnrolledInstitutionName,
        ISNULL(g.GradeName, '') EnrolledGradeName,
        s.EnrollmentDate,
        CAST(CASE WHEN s.IsKadamPlusStudent = 0
                      AND s.CurrentStatus IN (1, 2)
                      AND EXISTS (
                          SELECT 1
                          FROM dbo.StudentBaselineDetails baseline
                          WHERE baseline.StudentId = s.Id
                            AND baseline.IsDeleted = 0
                            AND baseline.BaselineType = 'baselinepreAssessment'
                            AND baseline.CompletedDate IS NOT NULL
                      )
                      AND EXISTS (
                          SELECT 1
                          FROM dbo.StudentBaselineDetails endline
                          WHERE endline.StudentId = s.Id
                            AND endline.IsDeleted = 0
                            AND endline.BaselineType = 'endlinepreAssessment'
                            AND endline.CompletedDate IS NOT NULL
                      )
                      AND NOT EXISTS (
                          SELECT 1
                          FROM dbo.StudentMainstreams mainstream
                          WHERE mainstream.StudentId = s.Id
                      )
                  THEN 1 ELSE 0 END AS BIT) IsEligible,
        CAST(CASE WHEN EXISTS (
            SELECT 1
            FROM dbo.StudentMainstreams mainstream
            WHERE mainstream.StudentId = s.Id
        ) THEN 1 ELSE 0 END AS BIT) HasExistingMainstream
    FROM dbo.Students s
    LEFT JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND i.IsDeleted = 0
    LEFT JOIN dbo.Grades g ON g.Id = s.GradeId AND g.IsDeleted = 0
    WHERE s.Id = @StudentId
      AND s.IsDeleted = 0;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WebStudents_AadhaarExists
    @AadhaarNumber VARCHAR(16),
    @ExceptId INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1 FROM dbo.Students
        WHERE AadhaarCardNumber = @AadhaarNumber
          AND Id <> @ExceptId
          AND IsDeleted = 0
    ) THEN 1 ELSE 0 END AS BIT);
END;
GO
