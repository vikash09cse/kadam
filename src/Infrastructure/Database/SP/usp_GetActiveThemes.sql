CREATE PROCEDURE [dbo].[usp_GetActiveThemes]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id AS [Value],
        ThemeName AS [Text]
    FROM Themes
    WHERE IsDeleted = 0
        AND CurrentStatus = 1 -- Active status
    ORDER BY ThemeName ASC;
END