Create Table InstitutionProjects(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    InstitutionId INT NOT NULL,
    ProjectId INT NOT NULL
)