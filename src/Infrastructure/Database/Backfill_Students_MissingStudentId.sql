-- =============================================================================
-- Update missing Students.StudentId
-- Format: KP/{StateCode}/{DistrictCode}/{YYYY}-{YY+1}/{Students.Id}
-- Example: KP/UP/GZB/2025-26/2793
-- =============================================================================

SET NOCOUNT ON;

/* -----------------------------------------------------------------------------
   1) Preview rows that will be updated (and proposed StudentId)
   ----------------------------------------------------------------------------- */
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
FROM dbo.Students s
INNER JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND ISNULL(i.IsDeleted, 0) = 0
INNER JOIN dbo.States st      ON st.Id = i.StateId
INNER JOIN dbo.Districts d    ON d.Id = i.DistrictId
WHERE ISNULL(s.IsDeleted, 0) = 0
  AND (s.StudentId IS NULL OR LTRIM(RTRIM(s.StudentId)) = '')
  AND s.InstitutionId > 0
  AND s.EnrollmentDate IS NOT NULL
  AND s.EnrollmentDate > '1900-01-01'
  AND NULLIF(LTRIM(RTRIM(st.StateCode)), '') IS NOT NULL
  AND NULLIF(LTRIM(RTRIM(d.DistrictCode)), '') IS NOT NULL
ORDER BY s.Id;

/* -----------------------------------------------------------------------------
   2) Update missing StudentId
   ----------------------------------------------------------------------------- */
BEGIN TRANSACTION;

UPDATE s
SET StudentId =
      'KP/'
    + LTRIM(RTRIM(st.StateCode)) + '/'
    + LTRIM(RTRIM(d.DistrictCode)) + '/'
    + CAST(YEAR(s.EnrollmentDate) AS VARCHAR(4)) + '-'
    + RIGHT(CAST(YEAR(s.EnrollmentDate) + 1 AS VARCHAR(4)), 2) + '/'
    + CAST(s.Id AS VARCHAR(20))
FROM dbo.Students s
INNER JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND ISNULL(i.IsDeleted, 0) = 0
INNER JOIN dbo.States st      ON st.Id = i.StateId
INNER JOIN dbo.Districts d    ON d.Id = i.DistrictId
WHERE ISNULL(s.IsDeleted, 0) = 0
  AND (s.StudentId IS NULL OR LTRIM(RTRIM(s.StudentId)) = '')
  AND s.InstitutionId > 0
  AND s.EnrollmentDate IS NOT NULL
  AND s.EnrollmentDate > '1900-01-01'
  AND NULLIF(LTRIM(RTRIM(st.StateCode)), '') IS NOT NULL
  AND NULLIF(LTRIM(RTRIM(d.DistrictCode)), '') IS NOT NULL;

PRINT CONCAT('Rows updated: ', @@ROWCOUNT);

/* -----------------------------------------------------------------------------
   3) Still missing? (need StateCode / DistrictCode / EnrollmentDate fixes)
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
FROM dbo.Students s
LEFT JOIN dbo.Institutions i ON i.Id = s.InstitutionId AND ISNULL(i.IsDeleted, 0) = 0
LEFT JOIN dbo.States st      ON st.Id = i.StateId
LEFT JOIN dbo.Districts d    ON d.Id = i.DistrictId
WHERE ISNULL(s.IsDeleted, 0) = 0
  AND (s.StudentId IS NULL OR LTRIM(RTRIM(s.StudentId)) = '')
ORDER BY s.Id;

-- Review results, then uncomment ONE of these:
-- COMMIT TRANSACTION;
-- ROLLBACK TRANSACTION;
