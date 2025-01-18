Create Or Alter Procedure usp_GetBlockByDistrictId
(
	@DistrictId Int
)
As
Begin
	Select Id as [Value], BlockName as [Text] From Blocks Where DistrictId = @DistrictId and CurrentStatus = 1 and IsDeleted = 0
End