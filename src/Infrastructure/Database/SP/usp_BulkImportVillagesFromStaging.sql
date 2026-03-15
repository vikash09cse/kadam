CREATE OR ALTER PROCEDURE dbo.usp_BulkImportVillagesFromStaging
    @ImportId   UNIQUEIDENTIFIER,
    @CreatedBy  INT,
    @Inserted   INT OUTPUT,
    @Updated    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        -- ─────────────────────────────────────────────────────────────────────
        -- PHASE 1: Resolve names → IDs in the staging table (3 single-pass UPDATEs)
        --
        -- Why this is fast:
        --   • States/Districts/Blocks are small tables → joins are cheap.
        --   • We write IDs back to staging once; everything after uses integer keys.
        --   • No LOWER() → SQL Server can use indexes on name columns.
        --   • Default SQL Server collation (CI_AS) already handles case-insensitivity.
        -- ─────────────────────────────────────────────────────────────────────

        -- 1a. Resolve StateId
        UPDATE stg
        SET stg.StateId = s.Id
        FROM dbo.VillageImportStaging stg
        INNER JOIN dbo.States s
            ON s.StateName = LTRIM(RTRIM(stg.StateName))
            AND s.IsDeleted = 0
        WHERE stg.ImportId = @ImportId;

        -- 1b. Resolve DistrictId (only for rows whose State was found)
        UPDATE stg
        SET stg.DistrictId = d.Id
        FROM dbo.VillageImportStaging stg
        INNER JOIN dbo.Districts d
            ON d.DistrictName = LTRIM(RTRIM(stg.DistrictName))
            AND d.StateId = stg.StateId
            AND d.IsDeleted = 0
        WHERE stg.ImportId = @ImportId
          AND stg.StateId IS NOT NULL;

        -- 1c. Resolve BlockId (only for rows whose District was found)
        UPDATE stg
        SET stg.BlockId = b.Id
        FROM dbo.VillageImportStaging stg
        INNER JOIN dbo.Blocks b
            ON b.BlockName = LTRIM(RTRIM(stg.BlockName))
            AND b.DistrictId = stg.DistrictId
            AND b.IsDeleted = 0
        WHERE stg.ImportId = @ImportId
          AND stg.DistrictId IS NOT NULL;

        -- ─────────────────────────────────────────────────────────────────────
        -- PHASE 2: Validate — simple NULL/empty checks, no more JOIN overhead
        -- ─────────────────────────────────────────────────────────────────────
        CREATE TABLE #Errors (RowNumber INT, ErrorMessage NVARCHAR(500));

        -- Empty Village Name
        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Village Name is required.'
        FROM dbo.VillageImportStaging
        WHERE ImportId = @ImportId
          AND LTRIM(RTRIM(ISNULL(VillageName, ''))) = '';

        -- Empty State Name
        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'State Name is required.'
        FROM dbo.VillageImportStaging
        WHERE ImportId = @ImportId
          AND LTRIM(RTRIM(ISNULL(StateName, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        -- Empty District Name
        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'District Name is required.'
        FROM dbo.VillageImportStaging
        WHERE ImportId = @ImportId
          AND LTRIM(RTRIM(ISNULL(DistrictName, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        -- Empty Block Name
        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'Block Name is required.'
        FROM dbo.VillageImportStaging
        WHERE ImportId = @ImportId
          AND LTRIM(RTRIM(ISNULL(BlockName, ''))) = ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        -- State not found (StateId still NULL after Phase 1)
        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber, 'State "' + StateName + '" not found.'
        FROM dbo.VillageImportStaging
        WHERE ImportId = @ImportId
          AND StateId IS NULL
          AND LTRIM(RTRIM(ISNULL(StateName, ''))) <> ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        -- District not found under the resolved State
        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber,
               'District "' + DistrictName + '" not found in State "' + StateName + '".'
        FROM dbo.VillageImportStaging
        WHERE ImportId = @ImportId
          AND StateId IS NOT NULL
          AND DistrictId IS NULL
          AND LTRIM(RTRIM(ISNULL(DistrictName, ''))) <> ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        -- Block not found under the resolved District
        INSERT INTO #Errors (RowNumber, ErrorMessage)
        SELECT RowNumber,
               'Block "' + BlockName + '" not found in District "' + DistrictName + '".'
        FROM dbo.VillageImportStaging
        WHERE ImportId = @ImportId
          AND DistrictId IS NOT NULL
          AND BlockId IS NULL
          AND LTRIM(RTRIM(ISNULL(BlockName, ''))) <> ''
          AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

        -- If any errors, return them and clean up staging
        IF EXISTS (SELECT 1 FROM #Errors)
        BEGIN
            SELECT RowNumber, ErrorMessage
            FROM #Errors
            ORDER BY RowNumber;

            SET @Inserted = 0;
            SET @Updated  = 0;

            DELETE FROM dbo.VillageImportStaging WHERE ImportId = @ImportId;
            DROP TABLE #Errors;
            RETURN;
        END;

        DROP TABLE #Errors;

        -- ─────────────────────────────────────────────────────────────────────
        -- PHASE 3: Upsert using integer IDs — fast index seeks, no string joins
        -- ─────────────────────────────────────────────────────────────────────

        -- Update existing villages
        UPDATE vil
        SET
            vil.ModifyDate = GETDATE(),
            vil.ModifyBy   = @CreatedBy
        FROM dbo.Villages vil
        INNER JOIN dbo.VillageImportStaging stg
            ON vil.BlockId      = stg.BlockId
            AND vil.VillageName = LTRIM(RTRIM(stg.VillageName))
            AND vil.IsDeleted   = 0
        WHERE stg.ImportId = @ImportId;

        SET @Updated = @@ROWCOUNT;

        -- Insert new villages
        INSERT INTO dbo.Villages
            (VillageName, BlockId, DistrictId, StateId, CurrentStatus, IsDeleted, DateCreated, CreatedBy)
        SELECT
            LTRIM(RTRIM(stg.VillageName)),
            stg.BlockId,
            stg.DistrictId,
            stg.StateId,
            1, 0, GETDATE(), @CreatedBy
        FROM dbo.VillageImportStaging stg
        WHERE stg.ImportId = @ImportId
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.Villages vil
              WHERE vil.BlockId      = stg.BlockId
                AND vil.VillageName  = LTRIM(RTRIM(stg.VillageName))
                AND vil.IsDeleted    = 0
          );

        SET @Inserted = @@ROWCOUNT;

        -- Clean up staging for this session
        DELETE FROM dbo.VillageImportStaging WHERE ImportId = @ImportId;

    END TRY
    BEGIN CATCH
        -- Always clean up staging on error
        DELETE FROM dbo.VillageImportStaging WHERE ImportId = @ImportId;
        THROW;
    END CATCH;
END;
