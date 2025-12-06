CREATE OR ALTER PROCEDURE usp_GetThemeActivity
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get ThemeActivity details
    SELECT 
        ta.Id,
        ta.ThemeId,
        ta.InstitutionId,
        ta.TotalStudents,
        ta.StudentAttended,
        ta.DidChildrenDayHappen,
        ta.TotalParentsAttended,
        ta.ThemeActivityDate,
        ta.CreatedBy,
        ta.DateCreated,
        ta.ModifyBy,
        ta.ModifyDate
    FROM ThemeActivities ta
    WHERE ta.Id = @Id AND ta.IsDeleted = 0;
    
    -- Get GradeSections for this ThemeActivity
    SELECT 
        tags.Id,
        tags.ThemeActivityId,
        tags.GradeId,
        tags.Section
    FROM ThemeActivityGradeSections tags
    WHERE tags.ThemeActivityId = @Id;
END

