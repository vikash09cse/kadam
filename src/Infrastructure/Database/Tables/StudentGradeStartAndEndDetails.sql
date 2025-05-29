CREATE TABLE StudentGradeStartAndEndDetails
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    GradeEntryLevelId INT NOT NULL,
    GradeExitLevelId INT NOT NULL,
    EntryStepId INT NOT NULL,
    ExitStepId INT NOT NULL,
    LastCompletedStepId INT NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
);