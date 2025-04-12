CREATE TABLE StudentHealths (
    Id INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT NOT NULL,
    PhysicallyChallenged BIT NOT NULL DEFAULT 0,
    PhysicallyChallengedType INT,
    PercentagePhysicallyChallenged DECIMAL(5,2),
    DisabilityCertificatePath VARCHAR(255),
    CurrentStatus INT NOT NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
); 