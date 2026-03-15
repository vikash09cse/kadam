CREATE OR ALTER PROCEDURE dbo.usp_BulkImportDistricts
    @Districts  dbo.DistrictImportType READONLY,
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

    -- Empty District Name
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT RowNumber, 'District Name is required.'
    FROM @Districts
    WHERE LTRIM(RTRIM(ISNULL(DistrictName, ''))) = '';

    -- Empty State Name (only if not already flagged)
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT RowNumber, 'State Name is required.'
    FROM @Districts
    WHERE LTRIM(RTRIM(ISNULL(StateName, ''))) = ''
      AND RowNumber NOT IN (SELECT RowNumber FROM #Errors);

    -- State Name not found in States table
    INSERT INTO #Errors (RowNumber, ErrorMessage)
    SELECT d.RowNumber, 'State "' + d.StateName + '" not found.'
    FROM @Districts d
    LEFT JOIN dbo.States s
        ON LOWER(LTRIM(RTRIM(s.StateName))) = LOWER(LTRIM(RTRIM(d.StateName)))
        AND s.IsDeleted = 0
    WHERE s.Id IS NULL
      AND LTRIM(RTRIM(ISNULL(d.StateName, ''))) <> ''
      AND d.RowNumber NOT IN (SELECT RowNumber FROM #Errors);

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

    -- Update existing districts (match by DistrictName + StateId, case-insensitive)
    UPDATE dist
    SET
        dist.DistrictCode = LTRIM(RTRIM(d.DistrictCode)),
        dist.ModifyDate   = GETDATE(),
        dist.ModifyBy     = @CreatedBy
    FROM dbo.Districts dist
    INNER JOIN @Districts d
        ON LOWER(dist.DistrictName) = LOWER(LTRIM(RTRIM(d.DistrictName)))
    INNER JOIN dbo.States s
        ON LOWER(s.StateName) = LOWER(LTRIM(RTRIM(d.StateName)))
        AND s.IsDeleted = 0
    WHERE dist.StateId = s.Id
      AND dist.IsDeleted = 0;

    SET @Updated = @@ROWCOUNT;

    -- Insert new districts
    INSERT INTO dbo.Districts
        (DistrictName, DistrictCode, StateId, CurrentStatus, IsDeleted, DateCreated, CreatedBy)
    SELECT
        LTRIM(RTRIM(d.DistrictName)),
        LTRIM(RTRIM(d.DistrictCode)),
        s.Id,
        1,       -- Active
        0,       -- Not deleted
        GETDATE(),
        @CreatedBy
    FROM @Districts d
    INNER JOIN dbo.States s
        ON LOWER(s.StateName) = LOWER(LTRIM(RTRIM(d.StateName)))
        AND s.IsDeleted = 0
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Districts dist
        WHERE LOWER(dist.DistrictName) = LOWER(LTRIM(RTRIM(d.DistrictName)))
          AND dist.StateId = s.Id
          AND dist.IsDeleted = 0
    );

    SET @Inserted = @@ROWCOUNT;
END;
