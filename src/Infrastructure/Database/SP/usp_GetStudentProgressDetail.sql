ALTER   Procedure [dbo].[usp_GetStudentProgressDetail] --usp_GetStudentProgressDetail 6
    @StudentId INT
AS
BEGIN

    Declare @GradeName VARCHAR(50), @BaselineCompletedDate DATETIME;


    SELECT @GradeName = GradeName FROM Grades WHERE Id = (SELECT GradeId FROM Students WHERE Id = @StudentId);
    SELECT Top 1 @BaselineCompletedDate = CompletedDate FROM StudentBaselineDetails WHERE StudentId = @StudentId AND BaselineType = 'baselinepreAssessment';
    SELECT 
    GradeEntryLevelId,
    GradeExitLevelId,
    EntryStepId,
    ExitStepId,
    LastCompletedStepId,
    @GradeName As GradeName,
    @BaselineCompletedDate As BaselineCompletedDate
     FROM StudentGradeStartAndEndDetails WHERE StudentId = @StudentId

    SELECT GradeLevelId FROM StudentGradeTestDetails WHERE StudentId = @StudentId Group By GradeLevelId
     
     SELECT S.ID As StepId, S.StepNameForMobile As StepText, SP.IsCompleted FROM Steps S Left Join StudentProgressSteps SP ON S.ID = SP.StepId and SP.StudentId = @StudentId
     WHERE S.IsDeleted = 0 
END


