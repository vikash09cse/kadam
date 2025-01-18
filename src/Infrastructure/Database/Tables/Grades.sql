Create Table Grades(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    GradeName NVARCHAR(50) NOT NULL,
    CurrentStatus INT NOT NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
)

INSERT INTO Grades (GradeName, CurrentStatus, CreatedBy)
VALUES 
    ('1st', 1, 1),
    ('2nd', 1, 1),
    ('3rd', 1, 1),
    ('4th', 1, 1),
    ('5th', 1, 1),
    ('6th', 1, 1),
    ('7th', 1, 1),
    ('8th', 1, 1),
    ('Kadam STC', 1, 1);

