CREATE TABLE Students (
    Id INT PRIMARY KEY IDENTITY(1,1),
    StudentId VARCHAR(100) NOT NULL,
    EnrollmentDate Date NOT NULL,
    ProfilePicture VARCHAR(155) NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    GenderId INT NULL,
    DateOfBirth DATE NOT NULL,
    Age INT NULL,
    DoYouHaveAadhaarCard BIT NOT NULL,
    AadhaarCardNumber VARCHAR(16) NOT NULL,
    InstitutionId INT NOT NULL,
    GradeId INT NOT NULL,
    Section Varchar(25),
    StudentRegistratioNumber VARCHAR(50) NOT NULL,
    ChildStatudBeforeKadamSTC INT NULL,
    HowLongPlaningToStayThisArea INT NULL,
    Class INT NULL,
    ReasonId INT NULL,
    DropoutClass Varchar(25) NULL,
    DropoutYear INT,
    CurrentStatus INT NOT NULL,
    Remarks VARCHAR(255) NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
)
