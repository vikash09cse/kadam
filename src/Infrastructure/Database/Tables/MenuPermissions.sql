Create Table MenuPermissions(
    Id Int Identity(1,1) Primary Key,
    ParentId INT,
    MenuName NVARCHAR(255) NOT NULL,
    MenuUrl VARCHAR(255) NULL,
    IconClass VARCHAR(100) NULL,
    SortOrder INT NOT NULL DEFAULT(0),
    MenuKey VARCHAR(100) NULL,
    CurrentStatus INT NOT NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
);