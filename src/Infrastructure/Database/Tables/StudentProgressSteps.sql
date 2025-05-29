Create Table StudentProgressSteps
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    StepId INT NOT NULL,
    IsCompleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
)
