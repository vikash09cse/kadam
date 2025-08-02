CREATE PROCEDURE usp_Student_Status_Update
    @StudentId INT,
    @Status INT,
    @InActiveReason Varchar(100) = NULL,
    @InActiveDate DATETIME = NULL,
    @Remarks VARCHAR(255),
    @UpdatedBy INT
AS
BEGIN
    UPDATE Students
    SET CurrentStatus = @Status,
        InActiveReason = @InActiveReason,
        InActiveDate = @InActiveDate,
        Remarks = @Remarks,
        ModifyBy = @UpdatedBy,
        ModifyDate = GETDATE()
    WHERE Id = @StudentId;
END; 
Alter Table Students
ADD InActiveReason Varchar(100) NULL,
    InActiveDate DATETIME NULL;
