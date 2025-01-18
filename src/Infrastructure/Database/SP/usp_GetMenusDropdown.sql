Create Or Alter Procedure usp_GetMenusDropdown
As
Begin
    Select Id as [Value], MenuName as [Text] From MenuPermissions Where IsDeleted = 0 AND ISNULL(ParentId,0)=0
End