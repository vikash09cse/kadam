-- One-time: add Student Attendance Report menu under Reports (if missing)
IF NOT EXISTS (
    SELECT 1
    FROM dbo.MenuPermissions
    WHERE IsDeleted = 0
      AND MenuName = N'Student Attendance Report'
)
BEGIN
    DECLARE @ReportsId INT =
    (
        SELECT TOP 1 Id
        FROM dbo.MenuPermissions
        WHERE IsDeleted = 0
          AND MenuName = N'Reports'
        ORDER BY Id
    );

    IF @ReportsId IS NOT NULL
    BEGIN
        INSERT INTO dbo.MenuPermissions
            (MenuName, ParentId, MenuUrl, IconClass, SortOrder, MenuKey, CurrentStatus, IsDeleted, DateCreated)
        VALUES
            (N'Student Attendance Report', @ReportsId, N'/Admin/AttendanceReport', NULL, 21, NULL, 1, 0, GETDATE());
    END
END
GO
