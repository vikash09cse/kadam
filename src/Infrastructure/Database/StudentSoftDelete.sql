/* Soft-delete students and related rows for test institutions 1–4 */
SET NOCOUNT ON;

DECLARE @Now       DATETIME = GETDATE();
DECLARE @DeletedBy INT      = 0;   /* adjust if you track a user id */

BEGIN TRANSACTION;

/* --- Child tables (via Students.Id = StudentId) --- */

UPDATE sfd
SET    sfd.IsDeleted = 1,
       sfd.DeletedBy = @DeletedBy,
       sfd.DeletedDate = @Now
FROM   StudentFamilyDetails AS sfd
INNER JOIN Students AS s ON s.Id = sfd.StudentId
WHERE  s.InstitutionId IN (1, 2, 3, 4)
       AND ISNULL(sfd.IsDeleted, 0) = 0;

UPDATE sh
SET    sh.IsDeleted = 1,
       sh.DeletedBy = @DeletedBy,
       sh.DeletedDate = @Now
FROM   StudentHealths AS sh
INNER JOIN Students AS s ON s.Id = sh.StudentId
WHERE  s.InstitutionId IN (1, 2, 3, 4)
       AND ISNULL(sh.IsDeleted, 0) = 0;

UPDATE sd
SET    sd.IsDeleted = 1,
       sd.DeletedBy = @DeletedBy,
       sd.DeletedDate = @Now
FROM   StudentDocuments AS sd
INNER JOIN Students AS s ON s.Id = sd.StudentId
WHERE  s.InstitutionId IN (1, 2, 3, 4)
       AND ISNULL(sd.IsDeleted, 0) = 0;

UPDATE sbd
SET    sbd.IsDeleted = 1,
       sbd.DeletedBy = @DeletedBy,
       sbd.DeletedDate = @Now
FROM   StudentBaselineDetails AS sbd
INNER JOIN Students AS s ON s.Id = sbd.StudentId
WHERE  s.InstitutionId IN (1, 2, 3, 4)
       AND ISNULL(sbd.IsDeleted, 0) = 0;

UPDATE sgtd
SET    sgtd.IsDeleted = 1,
       sgtd.DeletedBy = @DeletedBy,
       sgtd.DeletedDate = @Now
FROM   StudentGradeTestDetails AS sgtd
INNER JOIN Students AS s ON s.Id = sgtd.StudentId
WHERE  s.InstitutionId IN (1, 2, 3, 4)
       AND ISNULL(sgtd.IsDeleted, 0) = 0;

/* StudentTrios: only IsDeleted in table DDL (no DeletedBy/DeletedDate) */
UPDATE stt
SET    stt.IsDeleted = 1
FROM   StudentTrios AS stt
INNER JOIN Students AS s ON s.Id = stt.StudentId
WHERE  s.InstitutionId IN (1, 2, 3, 4)
       AND ISNULL(stt.IsDeleted, 0) = 0;

/* StudentFollowups: by institution and/or student in those institutions */
UPDATE sfu
SET    sfu.IsDeleted = 1,
       sfu.DeletedBy = @DeletedBy,
       sfu.DeletedDate = @Now
FROM   StudentFollowups AS sfu
LEFT JOIN Students AS s ON s.Id = sfu.StudentId
WHERE  sfu.InstitutionId IN (1, 2, 3, 4)
        OR (sfu.StudentId IS NOT NULL AND s.InstitutionId IN (1, 2, 3, 4))
       AND ISNULL(sfu.IsDeleted, 0) = 0;

/* --- Students last --- */
UPDATE s
SET    s.IsDeleted = 1,
       s.DeletedBy = @DeletedBy,
       s.DeletedDate = @Now
FROM   Students AS s
WHERE  s.InstitutionId IN (1, 2, 3, 4)
       AND ISNULL(s.IsDeleted, 0) = 0;

COMMIT TRANSACTION;