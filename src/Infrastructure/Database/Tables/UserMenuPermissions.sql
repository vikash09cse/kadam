CREATE TABLE UserMenuPermissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    MenuId INT NOT NULL,
    CurrentStatus INT NOT NULL DEFAULT(1),
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0,
    CanAddEdit BIT NOT NULL DEFAULT 1,
    CanDelete BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_UserMenuPermissions_User_Menu UNIQUE (UserId, MenuId)
);
