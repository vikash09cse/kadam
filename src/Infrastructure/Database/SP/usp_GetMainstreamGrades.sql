CREATE OR ALTER PROCEDURE dbo.usp_GetMainstreamGrades
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Sections NVARCHAR(200) = N'';

    SELECT @Sections = ISNULL(STRING_AGG(s.SectionName, ',') WITHIN GROUP (ORDER BY s.Id), N'')
    FROM dbo.Sections s
    WHERE ISNULL(s.IsDeleted, 0) = 0;

    SELECT
        g.Id,
        g.GradeName,
        g.GradeName AS Text,
        @Sections AS Sections
    FROM dbo.Grades g
    WHERE ISNULL(g.IsDeleted, 0) = 0
      AND g.GradeName IN (N'1st', N'2nd', N'3rd', N'4th', N'5th', N'6th', N'7th', N'8th', N'9th')
    ORDER BY
        CASE g.GradeName
            WHEN N'1st' THEN 1
            WHEN N'2nd' THEN 2
            WHEN N'3rd' THEN 3
            WHEN N'4th' THEN 4
            WHEN N'5th' THEN 5
            WHEN N'6th' THEN 6
            WHEN N'7th' THEN 7
            WHEN N'8th' THEN 8
            WHEN N'9th' THEN 9
            ELSE 99
        END;
END
GO
