-- =============================================================================
-- Backfill missing Students.StudentId (deadlock-safe)
-- Format: KP/{StateCode}/{DistrictCode}/{YYYY}-{YY+1}/{Students.Id}
-- Example: KP/UP/GZB/2025-26/2793
--
-- How to run:
--   1) Run Section 1 alone (preview) — optional
--   2) Run Section 2 alone (batched update; auto-commits each batch)
--   3) Run Section 3 alone (rows still missing)
-- Do NOT wrap the whole script in one long open transaction.
-- =============================================================================

SET NOCOUNT ON;
SET DEADLOCK_PRIORITY LOW;
SET XACT_ABORT ON;

/* -----------------------------------------------------------------------------
   1) Preview (read-only — run separately if needed)
   ----------------------------------------------------------------------------- */
/*
SELECT
    s.Id,
    s.FirstName,
    s.LastName,
    s.InstitutionId,
    s.EnrollmentDate,
    s.StudentId AS CurrentStudentId,
    LTRIM(RTRIM(st.StateCode)) AS StateCode,
    LTRIM(RTRIM(d.DistrictCode)) AS DistrictCode,
    'KP/'
        + LTRIM(RTRIM(st.StateCode)) + '/'
        + LTRIM(RTRIM(d.DistrictCode)) + '/'
        + CAST(YEAR(s.EnrollmentDate) AS VARCHAR(4)) + '-'
        + RIGHT(CAST(YEAR(s.EnrollmentDate) + 1 AS VARCHAR(4)), 2) + '/'
        + CAST(s.Id AS VARCHAR(20)) AS ProposedStudentId
FROM dbo.Students s WITH (NOLOCK)
INNER JOIN dbo.Institutions i WITH (NOLOCK)
    ON i.Id = s.InstitutionId AND ISNULL(i.IsDeleted, 0) = 0
INNER JOIN dbo.States st WITH (NOLOCK)
    ON st.Id = i.StateId
INNER JOIN dbo.Districts d WITH (NOLOCK)
    ON d.Id = i.DistrictId
WHERE ISNULL(s.IsDeleted, 0) = 0
  AND (s.StudentId IS NULL OR LTRIM(RTRIM(s.StudentId)) = '')
  AND s.InstitutionId > 0
  AND s.EnrollmentDate IS NOT NULL
  AND s.EnrollmentDate > '1900-01-01'
  AND NULLIF(LTRIM(RTRIM(st.StateCode)), '') IS NOT NULL
  AND NULLIF(LTRIM(RTRIM(d.DistrictCode)), '') IS NOT NULL
ORDER BY s.Id;
*/

/* -----------------------------------------------------------------------------
   2) Batched update — short transactions, Students PK only (avoids deadlocks)
   ----------------------------------------------------------------------------- */
IF OBJECT_ID('tempdb..#StudentIdBackfill') IS NOT NULL
    DROP TABLE #StudentIdBackfill;

CREATE TABLE #StudentIdBackfill
(
    Id               INT NOT NULL PRIMARY KEY,
    ProposedStudentId VARCHAR(55) NOT NULL
);

-- Snapshot candidates first (no long locks on live tables)
INSERT INTO #StudentIdBackfill (Id, ProposedStudentId)
SELECT
    s.Id,
    'KP/'
        + LTRIM(RTRIM(st.StateCode)) + '/'
        + LTRIM(RTRIM(d.DistrictCode)) + '/'
        + CAST(YEAR(s.EnrollmentDate) AS VARCHAR(4)) + '-'
        + RIGHT(CAST(YEAR(s.EnrollmentDate) + 1 AS VARCHAR(4)), 2) + '/'
        + CAST(s.Id AS VARCHAR(20))
FROM dbo.Students s WITH (NOLOCK)
INNER JOIN dbo.Institutions i WITH (NOLOCK)
    ON i.Id = s.InstitutionId AND ISNULL(i.IsDeleted, 0) = 0
INNER JOIN dbo.States st WITH (NOLOCK)
    ON st.Id = i.StateId
INNER JOIN dbo.Districts d WITH (NOLOCK)
    ON d.Id = i.DistrictId
WHERE ISNULL(s.IsDeleted, 0) = 0
  AND (s.StudentId IS NULL OR LTRIM(RTRIM(s.StudentId)) = '')
  AND s.InstitutionId > 0
  AND s.EnrollmentDate IS NOT NULL
  AND s.EnrollmentDate > '1900-01-01'
  AND NULLIF(LTRIM(RTRIM(st.StateCode)), '') IS NOT NULL
  AND NULLIF(LTRIM(RTRIM(d.DistrictCode)), '') IS NOT NULL;

DECLARE @BatchSize     INT = 100;
DECLARE @TotalUpdated  INT = 0;
DECLARE @BatchCount    INT = 1;
DECLARE @Attempt       INT;
DECLARE @RowsThisBatch INT;
DECLARE @ErrMsg        NVARCHAR(4000);
DECLARE @ErrNum        INT;

WHILE EXISTS (SELECT 1 FROM #StudentIdBackfill) AND @BatchCount > 0
BEGIN
    SET @Attempt = 0;
    SET @BatchCount = 0;
    SET @RowsThisBatch = 0;

    IF OBJECT_ID('tempdb..#BatchIds') IS NOT NULL
        DROP TABLE #BatchIds;

    SELECT TOP (@BatchSize) Id, ProposedStudentId
    INTO #BatchIds
    FROM #StudentIdBackfill
    ORDER BY Id;

    -- Retry a few times if a deadlock still occurs under load
    WHILE @Attempt < 5
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            UPDATE s WITH (ROWLOCK)
            SET s.StudentId = b.ProposedStudentId
            FROM dbo.Students s
            INNER JOIN #BatchIds b ON b.Id = s.Id
            WHERE s.StudentId IS NULL
               OR LTRIM(RTRIM(s.StudentId)) = '';

            SET @RowsThisBatch = @@ROWCOUNT;

            COMMIT TRANSACTION;

            DELETE t
            FROM #StudentIdBackfill t
            INNER JOIN #BatchIds b ON b.Id = t.Id;

            SET @BatchCount = @@ROWCOUNT;
            SET @TotalUpdated += @RowsThisBatch;
            BREAK;
        END TRY
        BEGIN CATCH
            IF XACT_STATE() <> 0
                ROLLBACK TRANSACTION;

            -- 1205 = deadlock victim
            IF ERROR_NUMBER() = 1205
            BEGIN
                SET @Attempt += 1;
                WAITFOR DELAY '00:00:00.250';
            END
            ELSE
            BEGIN
                SET @ErrMsg = ERROR_MESSAGE();
                SET @ErrNum = ERROR_NUMBER();
                RAISERROR('Backfill failed (%d): %s', 16, 1, @ErrNum, @ErrMsg);
                RETURN;
            END
        END CATCH
    END

    IF @Attempt >= 5
    BEGIN
        RAISERROR('Backfill stopped after repeated deadlocks. Re-run Section 2 later.', 16, 1);
        RETURN;
    END

    DROP TABLE #BatchIds;
END

PRINT CONCAT('Total rows updated: ', @TotalUpdated);

IF OBJECT_ID('tempdb..#StudentIdBackfill') IS NOT NULL
    DROP TABLE #StudentIdBackfill;

/* -----------------------------------------------------------------------------
   3) Still missing? (run after Section 2)
   ----------------------------------------------------------------------------- */
SELECT
    s.Id,
    s.FirstName,
    s.LastName,
    s.InstitutionId,
    s.EnrollmentDate,
    i.InstitutionName,
    st.StateCode,
    d.DistrictCode,
    CASE
        WHEN s.InstitutionId IS NULL OR s.InstitutionId <= 0 THEN 'Missing InstitutionId'
        WHEN i.Id IS NULL THEN 'Institution not found / deleted'
        WHEN s.EnrollmentDate IS NULL OR s.EnrollmentDate <= '1900-01-01' THEN 'Missing EnrollmentDate'
        WHEN NULLIF(LTRIM(RTRIM(st.StateCode)), '') IS NULL THEN 'Missing StateCode'
        WHEN NULLIF(LTRIM(RTRIM(d.DistrictCode)), '') IS NULL THEN 'Missing DistrictCode'
        ELSE 'Unknown'
    END AS Reason
FROM dbo.Students s WITH (NOLOCK)
LEFT JOIN dbo.Institutions i WITH (NOLOCK)
    ON i.Id = s.InstitutionId AND ISNULL(i.IsDeleted, 0) = 0
LEFT JOIN dbo.States st WITH (NOLOCK)
    ON st.Id = i.StateId
LEFT JOIN dbo.Districts d WITH (NOLOCK)
    ON d.Id = i.DistrictId
WHERE ISNULL(s.IsDeleted, 0) = 0
  AND (s.StudentId IS NULL OR LTRIM(RTRIM(s.StudentId)) = '')
ORDER BY s.Id;
