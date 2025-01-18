CREATE Or ALTER PROCEDURE [dbo].[usp_StatesByStatus]
    @CurrentStatus TINYINT
AS
BEGIN
    SELECT Id as [Value], StateName as [Text] FROM States WHERE CurrentStatus = @CurrentStatus and IsDeleted = 0
END
