CREATE OR ALTER PROCEDURE dbo.usp_ExportInstitutionList
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY inst.Id) AS SrNo,
        ISNULL(d.DivisionName, '') AS DivisionName,
        ISNULL(st.StateName, '') AS StateName,
        ISNULL(dist.DistrictName, '') AS DistrictName,
        ISNULL(b.BlockName, '') AS BlockName,
        ISNULL(v.VillageName, '') AS VillageName,
        CASE inst.InstitutionType
            WHEN 1 THEN 'Primary School'
            WHEN 2 THEN 'Middle School'
            WHEN 3 THEN 'High School'
            WHEN 4 THEN 'Pre School'
            WHEN 5 THEN 'DIET'
            ELSE ''
        END AS InstitutionTypeName,
        CASE inst.InstitutionBuilding
            WHEN 1 THEN 'Public'
            WHEN 2 THEN 'Private'
            WHEN 3 THEN 'Govt. School'
            ELSE ''
        END AS InstitutionBuildingName,
        inst.InstitutionName,
        inst.InstitutionCode,
        inst.InstitutionId AS InstitutionBusinessId,
        inst.InstitutionHeadMasterName,
        inst.InstitutionPhone,
        inst.InstitutionEmail,
        inst.InstitutionWebsite,
        inst.InstitutionAddress,
        inst.InstitutionMaleTeacherCount,
        inst.InstitutionFemaleTeacherCount,
        inst.InstitutionTotalTeacherCount,
        inst.InstitutionTotalStudentCount,
        inst.FinancialYearStart,
        inst.FinancialYearEnd,
        CASE inst.CurrentStatus
            WHEN 1 THEN 'Active'
            WHEN 2 THEN 'Inactive'
            WHEN 3 THEN 'Closed'
            ELSE ''
        END AS CurrentStatusName,
        ISNULL(gs.GradeSections, '') AS GradeSections
    FROM dbo.Institutions inst
    LEFT JOIN dbo.Divisions d ON inst.DivisionId = d.Id
    LEFT JOIN dbo.States st ON inst.StateId = st.Id
    LEFT JOIN dbo.Districts dist ON inst.DistrictId = dist.Id
    LEFT JOIN dbo.Blocks b ON inst.BlockId = b.Id
    LEFT JOIN dbo.Villages v ON inst.VillageId = v.Id
    OUTER APPLY (
        SELECT STRING_AGG(CONCAT(g.GradeName, ' (', igs.Sections, ')'), ', ') WITHIN GROUP (ORDER BY g.GradeName) AS GradeSections
        FROM dbo.InstitutionGradeSections igs
        INNER JOIN dbo.Grades g ON g.Id = igs.GradeId
        WHERE igs.InstitutionId = inst.Id
    ) gs
    WHERE inst.IsDeleted = 0
    ORDER BY inst.Id;
END
GO
