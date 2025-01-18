CREATE TABLE States (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StateName VARCHAR(55) NOT NULL,
    StateCode VARCHAR(10) NOT NULL,
    CurrentStatus INT NOT NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
);