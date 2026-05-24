IF OBJECT_ID('dbo.InstitutionImportStaging', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.InstitutionImportStaging
    (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        ImportId UNIQUEIDENTIFIER NOT NULL,
        RowNumber INT NOT NULL,
        DivisionName NVARCHAR(100) NOT NULL DEFAULT '',
        StateName NVARCHAR(100) NOT NULL DEFAULT '',
        DistrictName NVARCHAR(100) NOT NULL DEFAULT '',
        BlockName NVARCHAR(100) NOT NULL DEFAULT '',
        VillageName NVARCHAR(100) NOT NULL DEFAULT '',
        InstitutionTypeName NVARCHAR(100) NOT NULL DEFAULT '',
        InstitutionBuildingName NVARCHAR(100) NOT NULL DEFAULT '',
        InstitutionName NVARCHAR(100) NOT NULL DEFAULT '',
        InstitutionCode NVARCHAR(100) NOT NULL DEFAULT '',
        InstitutionBusinessId NVARCHAR(100) NOT NULL DEFAULT '',
        InstitutionHeadMasterName NVARCHAR(100) NOT NULL DEFAULT '',
        InstitutionPhone NVARCHAR(25) NOT NULL DEFAULT '',
        MaleTeachers NVARCHAR(20) NOT NULL DEFAULT '',
        FemaleTeachers NVARCHAR(20) NOT NULL DEFAULT '',
        TotalStudents NVARCHAR(20) NOT NULL DEFAULT '',
        FinancialYearStart NVARCHAR(25) NOT NULL DEFAULT '',
        FinancialYearEnd NVARCHAR(25) NOT NULL DEFAULT '',
        GradeSections NVARCHAR(1000) NOT NULL DEFAULT '',
        DivisionId INT NULL,
        StateId INT NULL,
        DistrictId INT NULL,
        BlockId INT NULL,
        VillageId INT NULL,
        InstitutionType INT NULL,
        InstitutionBuilding INT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );

    CREATE NONCLUSTERED INDEX IX_InstitutionImportStaging_ImportId
        ON dbo.InstitutionImportStaging (ImportId)
        INCLUDE (RowNumber);
END;
