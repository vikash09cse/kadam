-- Run once: creates (or upgrades) the staging table used by the bulk village import.
-- Each import session is isolated by ImportId (GUID).

IF OBJECT_ID('dbo.VillageImportStaging', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.VillageImportStaging
    (
        Id           BIGINT           IDENTITY(1,1) PRIMARY KEY,
        ImportId     UNIQUEIDENTIFIER NOT NULL,
        RowNumber    INT              NOT NULL,
        StateName    NVARCHAR(100)    NOT NULL DEFAULT '',
        DistrictName NVARCHAR(100)    NOT NULL DEFAULT '',
        BlockName    NVARCHAR(100)    NOT NULL DEFAULT '',
        VillageName  NVARCHAR(100)    NOT NULL DEFAULT '',
        -- Resolved by the SP in a single pass each; NULL = not found / not yet resolved
        StateId      INT              NULL,
        DistrictId   INT              NULL,
        BlockId      INT              NULL,
        CreatedAt    DATETIME         NOT NULL DEFAULT GETDATE()
    );

    CREATE NONCLUSTERED INDEX IX_VillageImportStaging_ImportId
        ON dbo.VillageImportStaging (ImportId)
        INCLUDE (RowNumber, StateId, DistrictId, BlockId);
END
ELSE
BEGIN
    -- Upgrade path: add ID columns if table was created before this version
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.VillageImportStaging') AND name = 'StateId')
        ALTER TABLE dbo.VillageImportStaging ADD StateId INT NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.VillageImportStaging') AND name = 'DistrictId')
        ALTER TABLE dbo.VillageImportStaging ADD DistrictId INT NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.VillageImportStaging') AND name = 'BlockId')
        ALTER TABLE dbo.VillageImportStaging ADD BlockId INT NULL;
END;
