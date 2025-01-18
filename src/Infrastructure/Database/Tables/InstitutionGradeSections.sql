Create Table InstitutionGradeSections(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    InstitutionId INT NOT NULL,
    GradeId INT NOT NULL,
    Sections VARCHAR(55) NOT NULL,
)