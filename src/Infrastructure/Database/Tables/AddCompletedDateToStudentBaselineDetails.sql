-- Add CompletedDate column to StudentBaselineDetails table
-- Default value is NULL as requested

ALTER TABLE StudentBaselineDetails
ADD CompletedDate DATETIME NULL;
