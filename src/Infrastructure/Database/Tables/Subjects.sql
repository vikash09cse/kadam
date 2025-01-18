Create Table Subjects (
    Id Int Identity(1,1) Primary Key,
    SubjectName Varchar(255) Not Null,
    CurrentStatus INT NOT NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
)