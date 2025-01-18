CREATE TABLE Blocks (
    Id INT IDENTITY(1,1) PRIMARY KEY,
	StateId	INT NOT NULL,
	DistrictId	INT NOT NULL,
    BlockName VARCHAR(55) NOT NULL,
    CurrentStatus INT NOT NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
);