CREATE OR ALTER PROCEDURE dbo.usp_BulkImportBlocks
    @Blocks     dbo.BlockImportType READONLY,
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

    -- Empty Block Name
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT RowNumber, 'Block Name is required.'
    FROM @Blocks
    WHERE LTRIM(RTRIM(ISNULL(BlockName, ''))) = '';

    -- Empty State Name (only if not already flagged)
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT RowNumber, 'State Name is required.'
    FROM @Blocks
    WHERE LTRIM(RTRIM(ISNULL(StateName, ''))) = ''
      AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

    -- Empty District Name (only if not already flagged)
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT RowNumber, 'District Name is required.'
    FROM @Blocks
    WHERE LTRIM(RTRIM(ISNULL(DistrictName, ''))) = ''
      AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

    -- State Name not found in States table
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT b.RowNumber, 'State "' + b.StateName + '" not found.'
    FROM @Blocks b
    LEFT JOIN dbo.States s
        ON LOWER(LTRIM(RTRIM(s.StateName))) = LOWER(LTRIM(RTRIM(b.StateName)))
        AND s.IsDeleted = 0
    WHERE s.Id IS NULL
      AND LTRIM(RTRIM(ISNULL(b.StateName, ''))) <> ''
      AND b.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

    -- District Name not found under the given State
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT b.RowNumber,
           'District "' + b.DistrictName + '" not found in State "' + b.StateName + '".'
    FROM @Blocks b
    INNER JOIN dbo.States s
        ON LOWER(LTRIM(RTRIM(s.StateName))) = LOWER(LTRIM(RTRIM(b.StateName)))
        AND s.IsDeleted = 0
    LEFT JOIN dbo.Districts d
        ON LOWER(LTRIM(RTRIM(d.DistrictName))) = LOWER(LTRIM(RTRIM(b.DistrictName)))
        AND d.StateId = s.Id
        AND d.IsDeleted = 0
    WHERE d.Id IS NULL
      AND LTRIM(RTRIM(ISNULL(b.DistrictName, ''))) <> ''
      AND b.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

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

    -- Update existing blocks (match by BlockName + DistrictId, case-insensitive)
    UPDATE blk
    SET
        blk.ModifyDate = GETDATE(),
        blk.ModifyBy   = @CreatedBy
    FROM dbo.Blocks blk
    INNER JOIN @Blocks b
        ON LOWER(blk.BlockName) = LOWER(LTRIM(RTRIM(b.BlockName)))
    INNER JOIN dbo.States s
        ON LOWER(s.StateName) = LOWER(LTRIM(RTRIM(b.StateName)))
        AND s.IsDeleted = 0
    INNER JOIN dbo.Districts d
        ON LOWER(d.DistrictName) = LOWER(LTRIM(RTRIM(b.DistrictName)))
        AND d.StateId = s.Id
        AND d.IsDeleted = 0
    WHERE blk.DistrictId = d.Id
      AND blk.IsDeleted = 0;

    SET @Updated = @@ROWCOUNT;

    -- Insert new blocks
    INSERT INTO dbo.Blocks
        (BlockName, DistrictId, StateId, CurrentStatus, IsDeleted, DateCreated, CreatedBy)
    SELECT
        LTRIM(RTRIM(b.BlockName)),
        d.Id,
        s.Id,
        1,       -- Active
        0,       -- Not deleted
        GETDATE(),
        @CreatedBy
    FROM @Blocks b
    INNER JOIN dbo.States s
        ON LOWER(s.StateName) = LOWER(LTRIM(RTRIM(b.StateName)))
        AND s.IsDeleted = 0
    INNER JOIN dbo.Districts d
        ON LOWER(d.DistrictName) = LOWER(LTRIM(RTRIM(b.DistrictName)))
        AND d.StateId = s.Id
        AND d.IsDeleted = 0
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Blocks blk
        WHERE LOWER(blk.BlockName) = LOWER(LTRIM(RTRIM(b.BlockName)))
          AND blk.DistrictId = d.Id
          AND blk.IsDeleted = 0
    );

    SET @Inserted = @@ROWCOUNT;
END;
