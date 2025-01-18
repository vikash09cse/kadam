Create Or Alter Procedure usp_GetDivisionsByStatus
(
    @CurrentStatus Int
)
As
Begin
    Select Id as [Value], DivisionName as [Text], DivisionStatus From Divisions Where 1=1 
	--AND DivisionStatus = @CurrentStatus 
	And IsDeleted = 0
End