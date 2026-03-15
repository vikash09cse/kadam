-- Run this once to create the TVP type used by usp_BulkImportVillages
-- If the type already exists, drop and re-create it.

IF TYPE_ID(N'dbo.VillageImportType') IS NOT NULL
    DROP TYPE dbo.VillageImportType;

CREATE TYPE dbo.VillageImportType AS TABLE
(
    RowNumber    INT            NOT NULL,
    StateName    NVARCHAR(100)  NOT NULL,
    DistrictName NVARCHAR(100)  NOT NULL,
    BlockName    NVARCHAR(100)  NOT NULL,
    VillageName  NVARCHAR(100)  NOT NULL
);
