CREATE OR ALTER PROCEDURE dbo.usp_BulkImportInstitutionsFromStaging
    @ImportId UNIQUEIDENTIFIER,
    @CreatedBy INT,
    @Inserted INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Inserted = 0;

    BEGIN TRY
        UPDATE stg
        SET stg.DivisionId = d.Id
        FROM dbo.InstitutionImportStaging stg
        INNER JOIN dbo.Divisions d
            ON d.DivisionName = LTRIM(RTRIM(stg.DivisionName))
            AND d.IsDeleted = 0
        WHERE stg.ImportId = @ImportId;

        UPDATE stg
        SET stg.StateId = s.Id
        FROM dbo.InstitutionImportStaging stg
        INNER JOIN dbo.States s
            ON s.StateName = LTRIM(RTRIM(stg.StateName))
            AND s.IsDeleted = 0
        WHERE stg.ImportId = @ImportId;

        UPDATE stg
        SET stg.DistrictId = dist.Id
        FROM dbo.InstitutionImportStaging stg
        INNER JOIN dbo.Districts dist
            ON dist.DistrictName = LTRIM(RTRIM(stg.DistrictName))
            AND dist.StateId = stg.StateId
            AND dist.IsDeleted = 0
        WHERE stg.ImportId = @ImportId
          AND stg.StateId IS NOT NULL;

        UPDATE stg
        SET stg.BlockId = b.Id
        FROM dbo.InstitutionImportStaging stg
        INNER JOIN dbo.Blocks b
            ON b.BlockName = LTRIM(RTRIM(stg.BlockName))
            AND b.DistrictId = stg.DistrictId
            AND b.IsDeleted = 0
        WHERE stg.ImportId = @ImportId
          AND stg.DistrictId IS NOT NULL;

        UPDATE stg
        SET stg.VillageId = v.Id
        FROM dbo.InstitutionImportStaging stg
        INNER JOIN dbo.Villages v
            ON v.VillageName = LTRIM(RTRIM(stg.VillageName))
            AND v.BlockId = stg.BlockId
            AND v.IsDeleted = 0
        WHERE stg.ImportId = @ImportId
          AND stg.BlockId IS NOT NULL;

        UPDATE stg
        SET stg.InstitutionType = CASE LOWER(LTRIM(RTRIM(stg.InstitutionTypeName)))
                WHEN 'primary school' THEN 1
                WHEN 'middle school' THEN 2
                WHEN 'high school' THEN 3
                WHEN 'pre school' THEN 4
                WHEN 'diet' THEN 5
                ELSE NULL END,
            stg.InstitutionBuilding = CASE LOWER(LTRIM(RTRIM(stg.InstitutionBuildingName)))
                WHEN 'public' THEN 1
                WHEN 'private' THEN 2
                WHEN 'govt. school' THEN 3
                WHEN 'govt school' THEN 3
                ELSE NULL END
        FROM dbo.InstitutionImportStaging stg
        WHERE stg.ImportId = @ImportId;

        CREATE TABLE #Errors (RowNumber INT, ErrorMessage NVARCHAR(500));

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Division is required.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND LTRIM(RTRIM(ISNULL(DivisionName, ''))) = '';

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'State is required.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND LTRIM(RTRIM(ISNULL(StateName, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'District is required.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND LTRIM(RTRIM(ISNULL(DistrictName, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Block is required.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND LTRIM(RTRIM(ISNULL(BlockName, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Village is required.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND LTRIM(RTRIM(ISNULL(VillageName, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Institution Type is required.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND LTRIM(RTRIM(ISNULL(InstitutionTypeName, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Institution Building is required.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND LTRIM(RTRIM(ISNULL(InstitutionBuildingName, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Institution Name is required.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND LTRIM(RTRIM(ISNULL(InstitutionName, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Institution Code is required.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND LTRIM(RTRIM(ISNULL(InstitutionCode, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Institution Id is required.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND LTRIM(RTRIM(ISNULL(InstitutionBusinessId, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Head Master / Principal is required.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND LTRIM(RTRIM(ISNULL(InstitutionHeadMasterName, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Contact is required.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND LTRIM(RTRIM(ISNULL(InstitutionPhone, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Grade Sections is required. Example: 1st:A,B|2nd:A'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND LTRIM(RTRIM(ISNULL(GradeSections, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Division "' + DivisionName + '" not found.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND DivisionId IS NULL
          AND LTRIM(RTRIM(ISNULL(DivisionName, ''))) <> ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'State "' + StateName + '" not found.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND StateId IS NULL
          AND LTRIM(RTRIM(ISNULL(StateName, ''))) <> ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'District "' + DistrictName + '" not found in State "' + StateName + '".'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND StateId IS NOT NULL AND DistrictId IS NULL
          AND LTRIM(RTRIM(ISNULL(DistrictName, ''))) <> ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Block "' + BlockName + '" not found in District "' + DistrictName + '".'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND DistrictId IS NOT NULL AND BlockId IS NULL
          AND LTRIM(RTRIM(ISNULL(BlockName, ''))) <> ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Village "' + VillageName + '" not found in Block "' + BlockName + '".'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND BlockId IS NOT NULL AND VillageId IS NULL
          AND LTRIM(RTRIM(ISNULL(VillageName, ''))) <> ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Invalid Institution Type "' + InstitutionTypeName + '".'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND InstitutionType IS NULL
          AND LTRIM(RTRIM(ISNULL(InstitutionTypeName, ''))) <> ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Invalid Institution Building "' + InstitutionBuildingName + '".'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId AND InstitutionBuilding IS NULL
          AND LTRIM(RTRIM(ISNULL(InstitutionBuildingName, ''))) <> ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Male Teachers must be a non-negative number.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId
          AND TRY_CONVERT(INT, LTRIM(RTRIM(MaleTeachers))) IS NULL
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Female Teachers must be a non-negative number.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId
          AND TRY_CONVERT(INT, LTRIM(RTRIM(FemaleTeachers))) IS NULL
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Total Students must be a non-negative number.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId
          AND TRY_CONVERT(INT, LTRIM(RTRIM(TotalStudents))) IS NULL
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Financial Year Start is invalid.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId
          AND TRY_CONVERT(DATE, LTRIM(RTRIM(FinancialYearStart)), 103) IS NULL
          AND TRY_CONVERT(DATE, LTRIM(RTRIM(FinancialYearStart)), 101) IS NULL
          AND TRY_CONVERT(DATE, LTRIM(RTRIM(FinancialYearStart)), 120) IS NULL
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Financial Year End is invalid.'
        FROM dbo.InstitutionImportStaging
        WHERE ImportId = @ImportId
          AND TRY_CONVERT(DATE, LTRIM(RTRIM(FinancialYearEnd)), 103) IS NULL
          AND TRY_CONVERT(DATE, LTRIM(RTRIM(FinancialYearEnd)), 101) IS NULL
          AND TRY_CONVERT(DATE, LTRIM(RTRIM(FinancialYearEnd)), 120) IS NULL
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT stg.RowNumber, 'Financial Year Start cannot be later than Financial Year End.'
        FROM dbo.InstitutionImportStaging stg
        WHERE stg.ImportId = @ImportId
          AND COALESCE(
                TRY_CONVERT(DATE, LTRIM(RTRIM(stg.FinancialYearStart)), 103),
                TRY_CONVERT(DATE, LTRIM(RTRIM(stg.FinancialYearStart)), 101),
                TRY_CONVERT(DATE, LTRIM(RTRIM(stg.FinancialYearStart)), 120)
              ) >
              COALESCE(
                TRY_CONVERT(DATE, LTRIM(RTRIM(stg.FinancialYearEnd)), 103),
                TRY_CONVERT(DATE, LTRIM(RTRIM(stg.FinancialYearEnd)), 101),
                TRY_CONVERT(DATE, LTRIM(RTRIM(stg.FinancialYearEnd)), 120)
              )
          AND stg.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT stg1.RowNumber, 'Duplicate Institution Code "' + stg1.InstitutionCode + '" in import file.'
        FROM dbo.InstitutionImportStaging stg1
        INNER JOIN dbo.InstitutionImportStaging stg2
            ON stg1.ImportId = stg2.ImportId
            AND stg1.InstitutionCode = stg2.InstitutionCode
            AND stg1.RowNumber <> stg2.RowNumber
        WHERE stg1.ImportId = @ImportId
          AND LTRIM(RTRIM(stg1.InstitutionCode)) <> ''
          AND stg1.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT stg1.RowNumber, 'Duplicate Institution Id "' + stg1.InstitutionBusinessId + '" in import file.'
        FROM dbo.InstitutionImportStaging stg1
        INNER JOIN dbo.InstitutionImportStaging stg2
            ON stg1.ImportId = stg2.ImportId
            AND stg1.InstitutionBusinessId = stg2.InstitutionBusinessId
            AND stg1.RowNumber <> stg2.RowNumber
        WHERE stg1.ImportId = @ImportId
          AND LTRIM(RTRIM(stg1.InstitutionBusinessId)) <> ''
          AND stg1.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT stg.RowNumber, 'Institution Code "' + stg.InstitutionCode + '" already exists.'
        FROM dbo.InstitutionImportStaging stg
        INNER JOIN dbo.Institutions i
            ON i.InstitutionCode = LTRIM(RTRIM(stg.InstitutionCode))
            AND i.IsDeleted = 0
        WHERE stg.ImportId = @ImportId
          AND stg.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT stg.RowNumber, 'Institution Id "' + stg.InstitutionBusinessId + '" already exists.'
        FROM dbo.InstitutionImportStaging stg
        INNER JOIN dbo.Institutions i
            ON i.InstitutionId = LTRIM(RTRIM(stg.InstitutionBusinessId))
            AND i.IsDeleted = 0
        WHERE stg.ImportId = @ImportId
          AND stg.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT stg.RowNumber, 'Institution Name "' + stg.InstitutionName + '" already exists.'
        FROM dbo.InstitutionImportStaging stg
        INNER JOIN dbo.Institutions i
            ON i.InstitutionName = LTRIM(RTRIM(stg.InstitutionName))
            AND i.IsDeleted = 0
        WHERE stg.ImportId = @ImportId
          AND stg.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        CREATE TABLE #ParsedGradeSections (
            RowNumber INT NOT NULL,
            GradeName NVARCHAR(50) NOT NULL,
            Sections NVARCHAR(55) NOT NULL
        );

        INSERT INTO #ParsedGradeSections (RowNumber, GradeName, Sections)
        SELECT
            stg.RowNumber,
            LTRIM(RTRIM(LEFT(part.[value], NULLIF(CHARINDEX(':', part.[value]), 0) - 1))),
            LTRIM(RTRIM(SUBSTRING(part.[value], CHARINDEX(':', part.[value]) + 1, 8000)))
        FROM dbo.InstitutionImportStaging stg
        CROSS APPLY STRING_SPLIT(stg.GradeSections, '|') part
        WHERE stg.ImportId = @ImportId
          AND CHARINDEX(':', part.[value]) > 0
          AND stg.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT stg.RowNumber, 'Grade Sections format is invalid. Use Grade:Section1,Section2|Grade2:Section1'
        FROM dbo.InstitutionImportStaging stg
        WHERE stg.ImportId = @ImportId
          AND stg.RowNumber NOT IN (SELECT RowNumber FROM #Errors)
          AND NOT EXISTS (SELECT 1 FROM #ParsedGradeSections p WHERE p.RowNumber = stg.RowNumber);

        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT DISTINCT p.RowNumber, 'Grade "' + p.GradeName + '" not found.'
        FROM #ParsedGradeSections p
        LEFT JOIN dbo.Grades g
            ON g.GradeName = p.GradeName
            AND g.IsDeleted = 0
        WHERE g.Id IS NULL
          AND p.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        IF EXISTS (SELECT 1 FROM #Errors)
        BEGIN
            SELECT RowNumber, ErrorMessage FROM #Errors ORDER BY RowNumber;
            DELETE FROM dbo.InstitutionImportStaging WHERE ImportId = @ImportId;
            DROP TABLE #Errors;
            DROP TABLE #ParsedGradeSections;
            RETURN;
        END;

        DROP TABLE #Errors;

        CREATE TABLE #ReadyRows (
            RowNumber INT NOT NULL PRIMARY KEY,
            DivisionId INT NOT NULL,
            StateId INT NOT NULL,
            DistrictId INT NOT NULL,
            BlockId INT NOT NULL,
            VillageId INT NOT NULL,
            InstitutionType INT NOT NULL,
            InstitutionBuilding INT NOT NULL,
            InstitutionName NVARCHAR(100) NOT NULL,
            InstitutionCode NVARCHAR(100) NOT NULL,
            InstitutionBusinessId NVARCHAR(100) NOT NULL,
            InstitutionHeadMasterName NVARCHAR(100) NOT NULL,
            InstitutionPhone NVARCHAR(25) NOT NULL,
            MaleTeachers INT NOT NULL,
            FemaleTeachers INT NOT NULL,
            TotalStudents INT NOT NULL,
            FinancialYearStart VARCHAR(25) NOT NULL,
            FinancialYearEnd VARCHAR(25) NOT NULL
        );

        INSERT INTO #ReadyRows
        SELECT
            stg.RowNumber,
            stg.DivisionId,
            stg.StateId,
            stg.DistrictId,
            stg.BlockId,
            stg.VillageId,
            stg.InstitutionType,
            stg.InstitutionBuilding,
            LTRIM(RTRIM(stg.InstitutionName)),
            LTRIM(RTRIM(stg.InstitutionCode)),
            LTRIM(RTRIM(stg.InstitutionBusinessId)),
            LTRIM(RTRIM(stg.InstitutionHeadMasterName)),
            LTRIM(RTRIM(stg.InstitutionPhone)),
            TRY_CONVERT(INT, LTRIM(RTRIM(stg.MaleTeachers))),
            TRY_CONVERT(INT, LTRIM(RTRIM(stg.FemaleTeachers))),
            TRY_CONVERT(INT, LTRIM(RTRIM(stg.TotalStudents))),
            CONVERT(VARCHAR(25), COALESCE(
                TRY_CONVERT(DATE, LTRIM(RTRIM(stg.FinancialYearStart)), 103),
                TRY_CONVERT(DATE, LTRIM(RTRIM(stg.FinancialYearStart)), 101),
                TRY_CONVERT(DATE, LTRIM(RTRIM(stg.FinancialYearStart)), 120)
            ), 120),
            CONVERT(VARCHAR(25), COALESCE(
                TRY_CONVERT(DATE, LTRIM(RTRIM(stg.FinancialYearEnd)), 103),
                TRY_CONVERT(DATE, LTRIM(RTRIM(stg.FinancialYearEnd)), 101),
                TRY_CONVERT(DATE, LTRIM(RTRIM(stg.FinancialYearEnd)), 120)
            ), 120)
        FROM dbo.InstitutionImportStaging stg
        WHERE stg.ImportId = @ImportId;

        CREATE TABLE #InsertedInstitutions (
            RowNumber INT NOT NULL,
            InstitutionId INT NOT NULL
        );

        MERGE dbo.Institutions AS target
        USING #ReadyRows AS source
        ON 1 = 0
        WHEN NOT MATCHED THEN
            INSERT (
                DivisionId, StateId, DistrictId, BlockId, VillageId,
                InstitutionType, InstitutionBuilding,
                InstitutionName, InstitutionCode, InstitutionId,
                InstitutionHeadMasterName, InstitutionPhone,
                InstitutionEmail, InstitutionWebsite, InstitutionLogo, InstitutionAddress,
                InstitutionMaleTeacherCount, InstitutionFemaleTeacherCount,
                InstitutionTotalTeacherCount, InstitutionTotalStudentCount,
                FinancialYearStart, FinancialYearEnd,
                CurrentStatus, CreatedBy, IsDeleted, DateCreated
            )
            VALUES (
                source.DivisionId, source.StateId, source.DistrictId, source.BlockId, source.VillageId,
                source.InstitutionType, source.InstitutionBuilding,
                source.InstitutionName, source.InstitutionCode, source.InstitutionBusinessId,
                source.InstitutionHeadMasterName, source.InstitutionPhone,
                '', '', '', '',
                source.MaleTeachers, source.FemaleTeachers,
                source.MaleTeachers + source.FemaleTeachers, source.TotalStudents,
                source.FinancialYearStart, source.FinancialYearEnd,
                1, @CreatedBy, 0, GETDATE()
            )
        OUTPUT source.RowNumber, inserted.Id INTO #InsertedInstitutions (RowNumber, InstitutionId);

        SET @Inserted = @@ROWCOUNT;

        INSERT INTO dbo.InstitutionGradeSections (InstitutionId, GradeId, Sections)
        SELECT
            ins.InstitutionId,
            g.Id,
            p.Sections
        FROM #ParsedGradeSections p
        INNER JOIN #InsertedInstitutions ins ON ins.RowNumber = p.RowNumber
        INNER JOIN dbo.Grades g ON g.GradeName = p.GradeName AND g.IsDeleted = 0;

        DROP TABLE #InsertedInstitutions;
        DROP TABLE #ReadyRows;
        DROP TABLE #ParsedGradeSections;
        DELETE FROM dbo.InstitutionImportStaging WHERE ImportId = @ImportId;
    END TRY
    BEGIN CATCH
        DELETE FROM dbo.InstitutionImportStaging WHERE ImportId = @ImportId;
        THROW;
    END CATCH;
END;
GO
