CREATE Or ALTER PROCEDURE [dbo].[usp_GetDistrictByStateId]
    @StateId INT
AS
BEGIN
    SELECT Id as [value], DistrictName as [text] FROM District WHERE StateId = @StateId and IsDeleted = 0 and CurrentStatus = 1
END
