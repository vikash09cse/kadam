CREATE TABLE Districts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
	StateId	INT NOT NULL,
    DistrictName VARCHAR(55) NOT NULL,
    DistrictCode VARCHAR(25) NOT NULL,
    CurrentStatus INT NOT NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
);