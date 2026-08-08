SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.StudentAttendances', 'DateEntryPoint') IS NULL
BEGIN
    ALTER TABLE dbo.StudentAttendances
        ADD DateEntryPoint TINYINT NOT NULL
            CONSTRAINT DF_StudentAttendances_DateEntryPoint DEFAULT (1) WITH VALUES;
END;

IF COL_LENGTH('dbo.StudentFollowups', 'DateEntryPoint') IS NULL
BEGIN
    ALTER TABLE dbo.StudentFollowups
        ADD DateEntryPoint TINYINT NOT NULL
            CONSTRAINT DF_StudentFollowups_DateEntryPoint DEFAULT (1) WITH VALUES;
END;

IF COL_LENGTH('dbo.ThemeActivities', 'DateEntryPoint') IS NULL
BEGIN
    ALTER TABLE dbo.ThemeActivities
        ADD DateEntryPoint TINYINT NOT NULL
            CONSTRAINT DF_ThemeActivities_DateEntryPoint DEFAULT (1) WITH VALUES;
END;

IF EXISTS
(
    SELECT 1
    FROM dbo.StudentAttendances
    GROUP BY StudentId, AttendanceDate
    HAVING COUNT_BIG(*) > 1
)
BEGIN
    ROLLBACK TRANSACTION;
    THROW 51000, 'Duplicate StudentAttendances rows exist for StudentId and AttendanceDate. Resolve them before applying Phase 3.', 1;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentAttendances')
      AND name = N'UX_StudentAttendances_StudentId_AttendanceDate'
)
BEGIN
    CREATE UNIQUE INDEX UX_StudentAttendances_StudentId_AttendanceDate
        ON dbo.StudentAttendances (StudentId, AttendanceDate);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.ThemeActivities')
      AND name = N'IX_ThemeActivities_WebList'
)
BEGIN
    CREATE INDEX IX_ThemeActivities_WebList
        ON dbo.ThemeActivities (InstitutionId, ThemeActivityDate DESC, Id DESC)
        INCLUDE
        (
            ThemeId,
            TotalStudents,
            StudentAttended,
            DidChildrenDayHappen,
            TotalParentsAttended,
            DateEntryPoint,
            CreatedBy,
            DateCreated
        )
        WHERE IsDeleted = 0;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.ThemeActivityGradeSections')
      AND name = N'IX_ThemeActivityGradeSections_ActivityGrade'
)
BEGIN
    CREATE INDEX IX_ThemeActivityGradeSections_ActivityGrade
        ON dbo.ThemeActivityGradeSections (ThemeActivityId, GradeId)
        INCLUDE (Section);
END;

COMMIT TRANSACTION;
GO
