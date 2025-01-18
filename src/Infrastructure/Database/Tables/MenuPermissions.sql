Create Table MenuPermissions(
    Id Int Identity(1,1) Primary Key,
    ParentId INT,
    MenuName NVARCHAR(255) NOT NULL,
    CurrentStatus INT NOT NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
);