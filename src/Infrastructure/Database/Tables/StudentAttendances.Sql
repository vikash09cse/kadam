CREATE TABLE StudentAttendances (
    Id INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT NOT NULL,
    AttendanceDate DATE NOT NULL,
    AttendanceStatus INT NOT NULL,
    AttendanceNote VARCHAR(255) NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
);