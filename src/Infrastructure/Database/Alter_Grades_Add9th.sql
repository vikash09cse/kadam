-- Add 9th grade to master Grades if it is missing.
-- Do not assume Id = 9; existing databases already use 9 for Kadam STC.
IF NOT EXISTS (
    SELECT 1
    FROM dbo.Grades
    WHERE GradeName = N'9th'
      AND ISNULL(IsDeleted, 0) = 0
)
BEGIN
    INSERT INTO dbo.Grades (GradeName, CurrentStatus, CreatedBy)
    VALUES (N'9th', 1, 1);
END
GO
