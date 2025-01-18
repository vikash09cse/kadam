Create table Projects (
    Id int primary key identity(1,1),
    IsPreApproved BIT NOT NULL DEFAULT 0,
    ProjectName nvarchar(255) not null,
    StartDate DATE,
    EndDate DATE,
    DirectBeneficiaryIds VARCHAR(500),
    DirectBeneficiaryTotalCount INT,
    IndirectBeneficiaryIds VARCHAR(500),
    IndirectBeneficiaryTotalCount INT,
    CurrentStatus INT NOT NULL,
    CreatedBy INT,
    DateCreated DATETIME DEFAULT GETDATE(),
    ModifyBy INT,
    ModifyDate DATETIME,
    DeletedBy INT,
    DeletedDate DATETIME,
    IsDeleted BIT DEFAULT 0
);