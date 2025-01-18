Create Table Sections(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SectionName NVARCHAR(50) NOT NULL,
    CurrentStatus INT NOT NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
)

INSERT INTO Sections (SectionName, CurrentStatus, CreatedBy)
VALUES 
    ('A', 1, 1),
    ('B', 1, 1),
    ('C', 1, 1),
    ('D', 1, 1),
    ('E', 1, 1),
    ('NA', 1, 1);
