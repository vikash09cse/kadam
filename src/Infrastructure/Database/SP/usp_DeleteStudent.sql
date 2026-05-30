CREATE OR ALTER PROCEDURE dbo.usp_DeleteStudent
    @StudentRecordId INT,
    @DeletedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1
            FROM dbo.Users u
            INNER JOIN dbo.Roles r ON u.RoleId = r.Id AND r.IsDeleted = 0
            WHERE u.Id = @DeletedBy
              AND u.IsDeleted = 0
              AND LOWER(LTRIM(RTRIM(r.RoleName))) = 'admin'
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success, 'Only admin users can delete students.' AS Message;
            RETURN;
        END

        IF NOT EXISTS (
            SELECT 1
            FROM dbo.Students
            WHERE Id = @StudentRecordId AND IsDeleted = 0
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success, 'Student not found.' AS Message;
            RETURN;
        END

        DECLARE @Now DATETIME = GETUTCDATE();
        DECLARE @KadamStudentId VARCHAR(50);
        DECLARE @StudentName NVARCHAR(200);
        DECLARE @InstitutionId INT;
        DECLARE @InstitutionName NVARCHAR(200);
        DECLARE @DeletedByName NVARCHAR(200);

        SELECT
            @KadamStudentId = ISNULL(s.StudentId, ''),
            @StudentName = LTRIM(RTRIM(CONCAT(s.FirstName, ' ', s.LastName))),
            @InstitutionId = s.InstitutionId,
            @InstitutionName = ISNULL(i.InstitutionName, '')
        FROM dbo.Students s
        LEFT JOIN dbo.Institutions i ON i.Id = s.InstitutionId
        WHERE s.Id = @StudentRecordId;

        SELECT @DeletedByName = LTRIM(RTRIM(CONCAT(u.FirstName, ' ', u.LastName)))
        FROM dbo.Users u
        WHERE u.Id = @DeletedBy;

        UPDATE sfd
        SET sfd.IsDeleted = 1,
            sfd.DeletedBy = @DeletedBy,
            sfd.DeletedDate = @Now
        FROM dbo.StudentFamilyDetails sfd
        WHERE sfd.StudentId = @StudentRecordId
          AND ISNULL(sfd.IsDeleted, 0) = 0;

        UPDATE sh
        SET sh.IsDeleted = 1,
            sh.DeletedBy = @DeletedBy,
            sh.DeletedDate = @Now
        FROM dbo.StudentHealths sh
        WHERE sh.StudentId = @StudentRecordId
          AND ISNULL(sh.IsDeleted, 0) = 0;

        UPDATE sd
        SET sd.IsDeleted = 1,
            sd.DeletedBy = @DeletedBy,
            sd.DeletedDate = @Now
        FROM dbo.StudentDocuments sd
        WHERE sd.StudentId = @StudentRecordId
          AND ISNULL(sd.IsDeleted, 0) = 0;

        UPDATE sbd
        SET sbd.IsDeleted = 1,
            sbd.DeletedBy = @DeletedBy,
            sbd.DeletedDate = @Now
        FROM dbo.StudentBaselineDetails sbd
        WHERE sbd.StudentId = @StudentRecordId
          AND ISNULL(sbd.IsDeleted, 0) = 0;

        UPDATE sgtd
        SET sgtd.IsDeleted = 1,
            sgtd.DeletedBy = @DeletedBy,
            sgtd.DeletedDate = @Now
        FROM dbo.StudentGradeTestDetails sgtd
        WHERE sgtd.StudentId = @StudentRecordId
          AND ISNULL(sgtd.IsDeleted, 0) = 0;

        UPDATE stt
        SET stt.IsDeleted = 1
        FROM dbo.StudentTrios stt
        WHERE stt.StudentId = @StudentRecordId
          AND ISNULL(stt.IsDeleted, 0) = 0;

        UPDATE sfu
        SET sfu.IsDeleted = 1,
            sfu.DeletedBy = @DeletedBy,
            sfu.DeletedDate = @Now
        FROM dbo.StudentFollowups sfu
        WHERE sfu.StudentId = @StudentRecordId
          AND ISNULL(sfu.IsDeleted, 0) = 0;

        --DELETE FROM dbo.StudentMainstreams
        --WHERE StudentId = @StudentRecordId;

        UPDATE s
        SET s.IsDeleted = 1,
            s.DeletedBy = @DeletedBy,
            s.DeletedDate = @Now
        FROM dbo.Students s
        WHERE s.Id = @StudentRecordId
          AND ISNULL(s.IsDeleted, 0) = 0;

        INSERT INTO dbo.StudentDeleteLogs
        (
            StudentRecordId,
            KadamStudentId,
            StudentName,
            InstitutionId,
            InstitutionName,
            DeletedBy,
            DeletedByName,
            DeletedDate
        )
        VALUES
        (
            @StudentRecordId,
            @KadamStudentId,
            @StudentName,
            @InstitutionId,
            @InstitutionName,
            @DeletedBy,
            @DeletedByName,
            @Now
        );

        COMMIT TRANSACTION;
        SELECT 1 AS Success, 'Record has been deleted successfully.' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
