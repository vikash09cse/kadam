Create Or Alter Procedure usp_GetStudentProgressDetail --usp_GetStudentProgressDetail 6
    @StudentId INT
AS
BEGIN
    SELECT 
    GradeEntryLevelId,
    GradeExitLevelId,
    EntryStepId,
    ExitStepId,
    LastCompletedStepId
     FROM StudentGradeStartAndEndDetails WHERE StudentId = @StudentId

    SELECT GradeLevelId FROM StudentGradeTestDetails WHERE StudentId = @StudentId Group By GradeLevelId
     
     SELECT S.ID As StepId, S.StepNameForMobile As StepText, SP.IsCompleted FROM Steps S Left Join StudentProgressSteps SP ON S.ID = SP.StepId and SP.StudentId = @StudentId
     WHERE S.IsDeleted = 0 
END
GO


