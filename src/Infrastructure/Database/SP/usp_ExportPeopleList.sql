CREATE OR ALTER PROCEDURE dbo.usp_ExportPeopleList
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY u.Id) AS SrNo,
        u.FirstName,
        u.LastName,
        u.UserName,
        CASE u.Gender
            WHEN 1 THEN 'Male'
            WHEN 2 THEN 'Female'
            WHEN 3 THEN 'Other'
            ELSE ''
        END AS GenderName,
        u.Phone,
        u.AlternatePhone,
        u.Email,
        r.RoleName,
        rr.RoleName AS ReporteeRoleName,
        u.LastGeneratedPassword,
        ISNULL(inst.AssignedInstitutions, '') AS AssignedInstitutions
    FROM Users u
    LEFT JOIN Roles r ON u.RoleId = r.Id
    LEFT JOIN Roles rr ON u.ReporteeRoleId = rr.Id
    OUTER APPLY (
        SELECT STRING_AGG(x.InstitutionName, ', ') WITHIN GROUP (ORDER BY x.InstitutionName) AS AssignedInstitutions
        FROM (
            SELECT DISTINCT i.InstitutionName
            FROM dbo.PeopleInstitutions pi
            CROSS APPLY dbo.SplitString(pi.InstitutionIds, ',') s
            INNER JOIN Institutions i
                ON i.Id = TRY_CAST(LTRIM(RTRIM(s.Item)) AS INT)
                AND i.IsDeleted = 0
            WHERE pi.UserId = u.Id
              AND LTRIM(RTRIM(ISNULL(pi.InstitutionIds, ''))) <> ''
        ) x
    ) inst
    WHERE u.IsDeleted = 0
    ORDER BY u.Id;
END
GO
