Create Or Alter Procedure usp_GetInstitutionByVillageId
@VillageId Int,
@InstitutionTypeId Int
As
Begin
	Select Id as [value], InstitutionName as [text] From Institutions Where IsDeleted = 0 
	And VillageId = @VillageId And InstitutionType = @InstitutionTypeId
End
