Create Table InstitutionPartners(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    InstitutionId INT NOT NULL,
    PartnerId INT NOT NULL
)