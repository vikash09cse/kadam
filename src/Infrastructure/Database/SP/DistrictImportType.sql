-- Run this once to create the TVP type used by usp_BulkImportDistricts
-- If the type already exists, drop and re-create it.

IF TYPE_ID(N'dbo.DistrictImportType') IS NOT NULL
    DROP TYPE dbo.DistrictImportType;

CREATE TYPE dbo.DistrictImportType AS TABLE
(
    RowNumber    INT            NOT NULL,
    StateName    NVARCHAR(100)  NOT NULL,
    DistrictName NVARCHAR(100)  NOT NULL,
    DistrictCode NVARCHAR(50)   NOT NULL
);
