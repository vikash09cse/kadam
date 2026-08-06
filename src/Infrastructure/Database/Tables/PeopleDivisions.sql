CREATE TABLE dbo.PeopleDivisions
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    DivisionId INT NOT NULL,
    CONSTRAINT UQ_PeopleDivisions_UserId_DivisionId UNIQUE (UserId, DivisionId)
);
GO
