-- One-time backfill: copy each institution's configured InstitutionGradeSections
-- into PeopleInstitutions.GradeAndSection (only selected grades/sections, not full catalog).
-- Run after Alter_PeopleInstitutions_AddGradeAndSection.sql

UPDATE pi
SET GradeAndSection = (
    SELECT igs.GradeId AS GradeId, igs.Sections AS Sections
    FROM dbo.InstitutionGradeSections igs
    WHERE igs.InstitutionId = TRY_CAST(LTRIM(RTRIM(pi.InstitutionIds)) AS INT)
    FOR JSON PATH
)
FROM dbo.PeopleInstitutions pi
WHERE (pi.GradeAndSection IS NULL OR LTRIM(RTRIM(pi.GradeAndSection)) = '')
  AND TRY_CAST(LTRIM(RTRIM(pi.InstitutionIds)) AS INT) IS NOT NULL;
GO
