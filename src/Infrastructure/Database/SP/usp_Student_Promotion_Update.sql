CREATE OR ALTER PROCEDURE usp_Student_Promotion_Update
    @StudentId INT,
    @PromotionDate DATE,
    @GradeId INT,
    @Section Varchar(25),
    @ModifyBy INT
AS
BEGIN
    UPDATE Students
    SET 
        GradeId = @GradeId,
        Section = @Section,
        PromotionDate = @PromotionDate,
        ModifyBy = @ModifyBy,
        ModifyDate = GETDATE()
    WHERE Id = @StudentId
END