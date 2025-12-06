Create Table ThemeActivityGradeSections
(
    Id INT PRIMARY KEY IDENTITY(1,1),
    ThemeActivityId INT NOT NULL,
    GradeId INT NOT NULL,
    Section VARCHAR(100)
)

