CREATE OR ALTER PROCEDURE dbo.usp_BulkImportVillages
    @Villages   dbo.VillageImportType READONLY,
    @CreatedBy  INT,
    @Inserted   INT OUTPUT,
    @Updated    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- ─────────────────────────────────────────────────────────────
    -- PHASE 1: Validate all rows before touching any data
    -- ─────────────────────────────────────────────────────────────
    CREATE TABLE #Errors (RowNumber INT, ErrorMessage NVARCHAR(500));

    -- Empty Village Name
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT RowNumber, 'Village Name is required.'
    FROM @Villages
    WHERE LTRIM(RTRIM(ISNULL(VillageName, ''))) = '';

    -- Empty State Name
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT RowNumber, 'State Name is required.'
    FROM @Villages
    WHERE LTRIM(RTRIM(ISNULL(StateName, ''))) = ''
      AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

    -- Empty District Name
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT RowNumber, 'District Name is required.'
    FROM @Villages
    WHERE LTRIM(RTRIM(ISNULL(DistrictName, ''))) = ''
      AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

    -- Empty Block Name
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT RowNumber, 'Block Name is required.'
    FROM @Villages
    WHERE LTRIM(RTRIM(ISNULL(BlockName, ''))) = ''
      AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

    -- State Name not found
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT v.RowNumber, 'State "' + v.StateName + '" not found.'
    FROM @Villages v
    LEFT JOIN dbo.States s
        ON LOWER(LTRIM(RTRIM(s.StateName))) = LOWER(LTRIM(RTRIM(v.StateName)))
        AND s.IsDeleted = 0
    WHERE s.Id IS NULL
      AND LTRIM(RTRIM(ISNULL(v.StateName, ''))) <> ''
      AND v.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

    -- District Name not found under the given State
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT v.RowNumber,
           'District "' + v.DistrictName + '" not found in State "' + v.StateName + '".'
    FROM @Villages v
    INNER JOIN dbo.States s
        ON LOWER(LTRIM(RTRIM(s.StateName))) = LOWER(LTRIM(RTRIM(v.StateName)))
        AND s.IsDeleted = 0
    LEFT JOIN dbo.Districts d
        ON LOWER(LTRIM(RTRIM(d.DistrictName))) = LOWER(LTRIM(RTRIM(v.DistrictName)))
        AND d.StateId = s.Id
        AND d.IsDeleted = 0
    WHERE d.Id IS NULL
      AND LTRIM(RTRIM(ISNULL(v.DistrictName, ''))) <> ''
      AND v.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

    -- Block Name not found under the given District
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT v.RowNumber,
           'Block "' + v.BlockName + '" not found in District "' + v.DistrictName + '".'
    FROM @Villages v
    INNER JOIN dbo.States s
        ON LOWER(LTRIM(RTRIM(s.StateName))) = LOWER(LTRIM(RTRIM(v.StateName)))
        AND s.IsDeleted = 0
    INNER JOIN dbo.Districts d
        ON LOWER(LTRIM(RTRIM(d.DistrictName))) = LOWER(LTRIM(RTRIM(v.DistrictName)))
        AND d.StateId = s.Id
        AND d.IsDeleted = 0
    LEFT JOIN dbo.Blocks b
        ON LOWER(LTRIM(RTRIM(b.BlockName))) = LOWER(LTRIM(RTRIM(v.BlockName)))
        AND b.DistrictId = d.Id
        AND b.IsDeleted = 0
    WHERE b.Id IS NULL
      AND LTRIM(RTRIM(ISNULL(v.BlockName, ''))) <> ''
      AND v.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

    -- If any validation errors, return them and abort
    IF EXISTS (SELECT 1 FROM #Errors)
    BEGIN
        SELECT RowNumber, ErrorMessage
        FROM #Errors
        ORDER BY RowNumber;

        SET @Inserted = 0;
        SET @Updated  = 0;

        DROP TABLE #Errors;
        RETURN;
    END;

    DROP TABLE #Errors;

    -- ─────────────────────────────────────────────────────────────
    -- PHASE 2: All rows valid — upsert
    -- ─────────────────────────────────────────────────────────────

    -- Update existing villages (match by VillageName + BlockId, case-insensitive)
    UPDATE vil
    SET
        vil.ModifyDate = GETDATE(),
        vil.ModifyBy   = @CreatedBy
    FROM dbo.Villages vil
    INNER JOIN @Villages v
        ON LOWER(vil.VillageName) = LOWER(LTRIM(RTRIM(v.VillageName)))
    INNER JOIN dbo.States s
        ON LOWER(s.StateName) = LOWER(LTRIM(RTRIM(v.StateName)))
        AND s.IsDeleted = 0
    INNER JOIN dbo.Districts d
        ON LOWER(d.DistrictName) = LOWER(LTRIM(RTRIM(v.DistrictName)))
        AND d.StateId = s.Id
        AND d.IsDeleted = 0
    INNER JOIN dbo.Blocks b
        ON LOWER(b.BlockName) = LOWER(LTRIM(RTRIM(v.BlockName)))
        AND b.DistrictId = d.Id
        AND b.IsDeleted = 0
    WHERE vil.BlockId = b.Id
      AND vil.IsDeleted = 0;

    SET @Updated = @@ROWCOUNT;

    -- Insert new villages
    INSERT INTO dbo.Villages
        (VillageName, BlockId, DistrictId, StateId, CurrentStatus, IsDeleted, DateCreated, CreatedBy)
    SELECT
        LTRIM(RTRIM(v.VillageName)),
        b.Id,
        d.Id,
        s.Id,
        1,       -- Active
        0,       -- Not deleted
        GETDATE(),
        @CreatedBy
    FROM @Villages v
    INNER JOIN dbo.States s
        ON LOWER(s.StateName) = LOWER(LTRIM(RTRIM(v.StateName)))
        AND s.IsDeleted = 0
    INNER JOIN dbo.Districts d
        ON LOWER(d.DistrictName) = LOWER(LTRIM(RTRIM(v.DistrictName)))
        AND d.StateId = s.Id
        AND d.IsDeleted = 0
    INNER JOIN dbo.Blocks b
        ON LOWER(b.BlockName) = LOWER(LTRIM(RTRIM(v.BlockName)))
        AND b.DistrictId = d.Id
        AND b.IsDeleted = 0
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Villages vil
        WHERE LOWER(vil.VillageName) = LOWER(LTRIM(RTRIM(v.VillageName)))
          AND vil.BlockId = b.Id
          AND vil.IsDeleted = 0
    );

    SET @Inserted = @@ROWCOUNT;
END;
