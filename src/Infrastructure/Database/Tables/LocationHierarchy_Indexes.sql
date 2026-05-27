-- Performance indexes for location cascade queries (idempotent)

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Villages_BlockId'
      AND object_id = OBJECT_ID('dbo.Villages')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Villages_BlockId
        ON dbo.Villages (BlockId)
        WHERE IsDeleted = 0;
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Villages_StateId_BlockId'
      AND object_id = OBJECT_ID('dbo.Villages')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Villages_StateId_BlockId
        ON dbo.Villages (StateId, BlockId)
        WHERE IsDeleted = 0;
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Blocks_DistrictId'
      AND object_id = OBJECT_ID('dbo.Blocks')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Blocks_DistrictId
        ON dbo.Blocks (DistrictId)
        WHERE IsDeleted = 0;
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_DivisionLocations_DivisionId'
      AND object_id = OBJECT_ID('dbo.DivisionLocations')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_DivisionLocations_DivisionId
        ON dbo.DivisionLocations (DivisionId)
        WHERE IsDeleted = 0;
END
