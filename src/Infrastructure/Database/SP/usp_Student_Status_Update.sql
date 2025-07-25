CREATE PROCEDURE usp_Student_Status_Update
    @StudentId INT,
    @Status INT,
    @Remarks VARCHAR(255),
    @UpdatedBy INT
AS
BEGIN
    UPDATE Students
    SET CurrentStatus = @Status,
        Remarks = @Remarks,
        ModifyBy = @UpdatedBy,
        ModifyDate = GETDATE()
    WHERE Id = @StudentId;
END; 