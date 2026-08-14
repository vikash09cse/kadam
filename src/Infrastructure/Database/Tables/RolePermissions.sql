Create Table RolePermissions(
    Id Int Identity(1,1) Primary Key,
    RoleId INT NOT NULL,
    MenuId INT NOT NULL,
	CurrentStatus INT Not NUll Default(1),
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0,
    CanAddEdit BIT NOT NULL DEFAULT 1,
    CanDelete BIT NOT NULL DEFAULT 1
);