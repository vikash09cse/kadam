Create Or Alter Procedure usp_StudentProgressStep_Save
    @StudentId INT,
    @StepId INT,
    @IsCompleted BIT,
    @CreatedBy INT
AS
BEGIN
    

    IF EXISTS (SELECT 1 FROM StudentProgressSteps WHERE StudentId = @StudentId AND StepId = @StepId)
    BEGIN
        UPDATE StudentProgressSteps
        SET IsCompleted = @IsCompleted,
            CreatedBy = @CreatedBy,
            CreatedDate = GETDATE()
        WHERE StudentId = @StudentId AND StepId = @StepId;
    END
    ELSE
    BEGIN
        INSERT INTO StudentProgressSteps (StudentId, StepId, IsCompleted, CreatedBy, CreatedDate)
        VALUES (@StudentId, @StepId, @IsCompleted, @CreatedBy, GETDATE());
    END
END;