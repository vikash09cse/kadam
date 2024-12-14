Create Or Alter Procedure [dbo].[usp_UpsertUser]
(
    @Id                 Int = Null,
    @Email              Varchar(100),
    @UserName           Varchar(100),
    @PasswordHash       Varbinary(64),
    @PasswordSalt       Varbinary(32),
    @FirstName          Varchar(50),
    @LastName           Varchar(50),
    @Phone              Varchar(25),
    @AlternatePhone     Varchar(25),
    @Gender             TinyInt = Null,
    @Grade              Varchar(100) = Null,
    @Section            Varchar(100) = Null,
    @GradeSection       Varchar(2000) = Null,
    @DivisionId         Int = Null,
    @RoleId             Int,
    @ReporteeRoleId     Int,
    @UserStatus         TinyInt,
    @ActivityType       Varchar(50) = Null,
    @DateCreated        Datetime = Null,
    @CreatedBy          Int = Null,
    @ModifyDate         Datetime = Null,
    @ModifyBy           Int = Null
)
AS
BEGIN
    IF @Id = 0
    BEGIN
        -- Insert new user
        INSERT INTO Users (
            Email, UserName, PasswordHash, PasswordSalt, FirstName, LastName, 
            Phone, AlternatePhone, Gender, Grade, Section, GradeSection, 
            DivisionId, RoleId, ReporteeRoleId, UserStatus, ActivityType, 
            IsDeleted, DateCreated, CreatedBy
        )
        VALUES (
            @Email, @UserName, @PasswordHash, @PasswordSalt, @FirstName, @LastName, 
            @Phone, @AlternatePhone, @Gender, @Grade, @Section, @GradeSection, 
            @DivisionId, @RoleId, @ReporteeRoleId, @UserStatus, @ActivityType, 
            0, GETDATE(), @CreatedBy
        )
    END
    ELSE
    BEGIN
        -- Update existing user
        UPDATE Users
        SET 
            Email = @Email,
            UserName = @UserName,
            PasswordHash = @PasswordHash,
            PasswordSalt = @PasswordSalt,
            FirstName = @FirstName,
            LastName = @LastName,
            Phone = @Phone,
            AlternatePhone = @AlternatePhone,
            Gender = @Gender,
            Grade = @Grade,
            Section = @Section,
            GradeSection = @GradeSection,
            DivisionId = @DivisionId,
            RoleId = @RoleId,
            ReporteeRoleId = @ReporteeRoleId,
            UserStatus = @UserStatus,
            ActivityType = @ActivityType,
            ModifyDate = GETDATE(),
            ModifyBy = @ModifyBy
        WHERE Id = @Id
    END
END 