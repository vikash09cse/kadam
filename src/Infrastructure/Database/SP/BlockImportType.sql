-- Run this once to create the TVP type used by usp_BulkImportBlocks
-- If the type already exists, drop and re-create it.

IF TYPE_ID(N'dbo.BlockImportType') IS NOT NULL
    DROP TYPE dbo.BlockImportType;

CREATE TYPE dbo.BlockImportType AS TABLE
(
    RowNumber    INT            NOT NULL,
    StateName    NVARCHAR(100)  NOT NULL,
    DistrictName NVARCHAR(100)  NOT NULL,
    BlockName    NVARCHAR(100)  NOT NULL
);
