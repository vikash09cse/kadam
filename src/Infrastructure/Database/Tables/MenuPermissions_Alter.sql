-- Extend MenuPermissions as navigation menu master
IF COL_LENGTH('MenuPermissions', 'MenuUrl') IS NULL
    ALTER TABLE MenuPermissions ADD MenuUrl VARCHAR(255) NULL;

IF COL_LENGTH('MenuPermissions', 'IconClass') IS NULL
    ALTER TABLE MenuPermissions ADD IconClass VARCHAR(100) NULL;

IF COL_LENGTH('MenuPermissions', 'SortOrder') IS NULL
    ALTER TABLE MenuPermissions ADD SortOrder INT NOT NULL DEFAULT(0);

IF COL_LENGTH('MenuPermissions', 'MenuKey') IS NULL
    ALTER TABLE MenuPermissions ADD MenuKey VARCHAR(100) NULL;
