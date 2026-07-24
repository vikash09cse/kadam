CREATE OR ALTER PROCEDURE [dbo].[usp_GetGradeSectionsByInstitutionId]
    @InstitutionId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        g.Id,
        igs.InstitutionId,
        g.GradeName,
        ISNULL(igs.Sections, '') AS Sections
    FROM dbo.InstitutionGradeSections igs
    INNER JOIN dbo.Grades g ON g.Id = igs.GradeId AND ISNULL(g.IsDeleted, 0) = 0
    WHERE igs.InstitutionId = @InstitutionId
    ORDER BY g.Id;
END;
GO
