Create Procedure usp_GetUserPrograms
(
    @UserId Int=0
)
As
Begin
    Select Id As ProgramId, ProgramName 
    ,IsSelected = Case When Id In (Select ProgramId From UserPrograms Where UserId = @UserId) Then 1 Else 0 End
    From Programs Where IsDeleted = 0
End