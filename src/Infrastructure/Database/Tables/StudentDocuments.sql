CREATE TABLE StudentDocuments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT NOT NULL,
    DocumentTypeId INT NOT NULL,
    DocumentNumber VARCHAR(55),
    DocumentPath VARCHAR(255),
    DocumentFileName VARCHAR(255),
    CurrentStatus INT NOT NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
); 