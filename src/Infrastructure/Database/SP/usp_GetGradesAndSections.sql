Create Or Alter Procedure [dbo].[usp_GetGradesAndSections]
AS
Begin
    Select Id, GradeName from Grades Where IsDeleted = 0
    Select Id, SectionName from Sections Where IsDeleted = 0
End