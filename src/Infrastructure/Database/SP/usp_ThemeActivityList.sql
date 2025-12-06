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
        ISNULL(STRING_AGG(CAST(tags.GradeId AS VARCHAR(10)), ','), '') AS GradeId,
        ISNULL(STRING_AGG(g.GradeName, ','), '') AS GradeName,
        ISNULL(STRING_AGG(
            CASE 
                WHEN tags.Section IS NULL OR tags.Section = '' 
                THEN g.GradeName 
                ELSE g.GradeName + ' - ' + tags.Section 
            END, 
            ', '
        ), '') AS Section,
        ta.TotalStudents,
        ta.StudentAttended,
        ta.DidChildrenDayHappen,
        ta.TotalParentsAttended,
        ta.ThemeActivityDate As DateCreated,
        ta.CreatedBy
    FROM ThemeActivities ta
        INNER JOIN Themes t ON ta.ThemeId = t.Id
        INNER JOIN Institutions i ON ta.InstitutionId = i.Id
        LEFT JOIN ThemeActivityGradeSections tags ON ta.Id = tags.ThemeActivityId
        LEFT JOIN Grades g ON tags.GradeId = g.Id
    WHERE ta.IsDeleted = 0
        AND (@InstitutionId IS NULL OR ta.InstitutionId = @InstitutionId)
        AND (@ThemeId IS NULL OR ta.ThemeId = @ThemeId)
        AND (@GradeId IS NULL OR EXISTS (
            SELECT 1 FROM ThemeActivityGradeSections tags_filter 
            WHERE tags_filter.ThemeActivityId = ta.Id AND tags_filter.GradeId = @GradeId
        ))
        AND (@Section IS NULL OR EXISTS (
            SELECT 1 FROM ThemeActivityGradeSections tags_filter 
            WHERE tags_filter.ThemeActivityId = ta.Id AND tags_filter.Section = @Section
        ))
        AND (@FromDate IS NULL OR CAST(ta.ThemeActivityDate AS DATE) >= CAST(@FromDate AS DATE))
        AND (@ToDate IS NULL OR CAST(ta.ThemeActivityDate AS DATE) <= CAST(@ToDate AS DATE))
        AND (@CreatedBy = 0 OR ta.CreatedBy = @CreatedBy)
    GROUP BY 
        ta.Id,
        ta.ThemeId,
        t.ThemeName,
        ta.InstitutionId,
        i.InstitutionName,
        ta.TotalStudents,
        ta.StudentAttended,
        ta.DidChildrenDayHappen,
        ta.TotalParentsAttended,
        ta.ThemeActivityDate,
        ta.CreatedBy
    ORDER BY ta.ThemeActivityDate DESC;
END