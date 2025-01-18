Create Or Alter Procedure usp_GetVillageByBlockId
(
    @BlockId Int
)
As
Begin
    Select Id as [Value], VillageName as [Text] From Villages Where BlockId = @BlockId and IsDeleted = 0
End