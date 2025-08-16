CREATE OR ALTER PROCEDURE [dbo].[usp_ThemeActivityList]
(
    @InstitutionId INT = NULL,
    @ThemeId INT = NULL,
    @GradeId INT = NULL,
    @Section VARCHAR(25) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @CreatedBy INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ta.Id,
        ta.ThemeId,
        t.ThemeName AS ThemeName,
        ta.InstitutionId,
        i.InstitutionName AS InstitutionName,
        ta.GradeId,
        g.GradeName AS GradeName,
        ta.Section,
        ta.TotalStudents,
        ta.StudentAttended,
        ta.DidChildrenDayHappen,
        ta.TotalParentsAttended,
        ta.DateCreated,
        ta.CreatedBy
    FROM ThemeActivities ta
        INNER JOIN Themes t ON ta.ThemeId = t.Id
        INNER JOIN Institutions i ON ta.InstitutionId = i.Id
        INNER JOIN Grades g ON ta.GradeId = g.Id
    WHERE ta.IsDeleted = 0
        AND (@InstitutionId IS NULL OR ta.InstitutionId = @InstitutionId)
        AND (@ThemeId IS NULL OR ta.ThemeId = @ThemeId)
        AND (@GradeId IS NULL OR ta.GradeId = @GradeId)
        AND (@Section IS NULL OR ta.Section = @Section)
        AND (@FromDate IS NULL OR ta.ThemeActivityDate >= @FromDate)
        AND (@ToDate IS NULL OR ta.ThemeActivityDate <= @ToDate)
        AND (@CreatedBy = 0 OR ta.CreatedBy = @CreatedBy)
    ORDER BY ta.DateCreated DESC;
END