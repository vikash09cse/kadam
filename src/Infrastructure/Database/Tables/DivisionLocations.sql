IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DivisionLocations' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.DivisionLocations
    (
        Id          INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
        DivisionId  INT NOT NULL,
        StateId     INT NOT NULL,
        DistrictId  INT NOT NULL,
        BlockId     INT NOT NULL,
        VillageId   INT NOT NULL,
        IsDeleted   BIT NOT NULL DEFAULT(0),
        DateCreated DATETIME NULL,
        CreatedBy   INT NULL,
        ModifyDate  DATETIME NULL,
        ModifyBy    INT NULL,
        DeletedDate DATETIME NULL,
        DeletedBy   INT NULL
    );

    CREATE NONCLUSTERED INDEX IX_DivisionLocations_DivisionId
        ON dbo.DivisionLocations (DivisionId)
        WHERE IsDeleted = 0;
END
