Create Table Roles(
    Id Int Identity(1,1) Primary Key,
    RoleName Varchar(255) Not Null,
    AllowMultipleDivision BIT NOT NULL DEFAULT 0,
    CurrentStatus INT NOT NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
);