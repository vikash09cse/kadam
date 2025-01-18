Create Or Alter Procedure usp_UpsertInstitution
    @Id INT = NULL,
    @DivisionId INT,
    @StateId INT,
    @DistrictId INT,
    @BlockId INT,
    @VillageId INT,
    @InstitutionType INT,
    @InstitutionBuilding INT,
    @InstitutionName VARCHAR(100),
    @InstitutionCode VARCHAR(100),
    @InstitutionId VARCHAR(100),
    @InstitutionHeadMasterName VARCHAR(100),
    @InstitutionPhone VARCHAR(25),
    @InstitutionEmail VARCHAR(100) = NULL,
    @InstitutionWebsite VARCHAR(155) = NULL,
    @InstitutionLogo VARCHAR(255) = NULL,
    @InstitutionAddress VARCHAR(255) = NULL,
    @InstitutionMaleTeacherCount INT,
    @InstitutionFemaleTeacherCount INT,
    @InstitutionTotalTeacherCount INT,
    @InstitutionTotalStudentCount INT,
    @FinancialYearStart VARCHAR(25),
    @FinancialYearEnd VARCHAR(25),
    @CurrentStatus INT,
    @CreatedBy INT,
    @ModifyBy INT = NULL,
    @jsonGradeSection VARCHAR(4000) = NULL
AS
BEGIN
    --SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @Id IS NULL OR @Id = 0
        BEGIN
            INSERT INTO dbo.Institutions (
                DivisionId, 
                StateId, 
                DistrictId, 
                BlockId, 
                VillageId, 
                InstitutionType, 
                InstitutionBuilding, 
                InstitutionName, 
                InstitutionCode, 
                InstitutionId, 
                InstitutionHeadMasterName, 
                InstitutionPhone, 
                InstitutionEmail, 
                InstitutionWebsite, 
                InstitutionLogo, 
                InstitutionAddress, 
                InstitutionMaleTeacherCount, 
                InstitutionFemaleTeacherCount, 
                InstitutionTotalTeacherCount, 
                InstitutionTotalStudentCount, 
                FinancialYearStart, 
                FinancialYearEnd, 
                CurrentStatus, 
                CreatedBy
            )
            VALUES (
                @DivisionId, 
                @StateId, 
                @DistrictId, 
                @BlockId, 
                @VillageId, 
                @InstitutionType, 
                @InstitutionBuilding, 
                @InstitutionName, 
                @InstitutionCode, 
                @InstitutionId, 
                @InstitutionHeadMasterName, 
                @InstitutionPhone, 
                @InstitutionEmail, 
                @InstitutionWebsite, 
                @InstitutionLogo, 
                @InstitutionAddress, 
                @InstitutionMaleTeacherCount, 
                @InstitutionFemaleTeacherCount, 
                @InstitutionTotalTeacherCount, 
                @InstitutionTotalStudentCount, 
                @FinancialYearStart, 
                @FinancialYearEnd, 
                @CurrentStatus, 
                @CreatedBy
            );

            SELECT @Id = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE dbo.Institutions
            SET 
                DivisionId = @DivisionId,
                StateId = @StateId,
                DistrictId = @DistrictId,
                BlockId = @BlockId,
                VillageId = @VillageId,
                InstitutionType = @InstitutionType,
                InstitutionBuilding = @InstitutionBuilding,
                InstitutionName = @InstitutionName,
                InstitutionCode = @InstitutionCode,
                InstitutionId = @InstitutionId,
                InstitutionHeadMasterName = @InstitutionHeadMasterName,
                InstitutionPhone = @InstitutionPhone,
                InstitutionEmail = @InstitutionEmail,
                InstitutionWebsite = @InstitutionWebsite,
                InstitutionLogo = @InstitutionLogo,
                InstitutionAddress = @InstitutionAddress,
                InstitutionMaleTeacherCount = @InstitutionMaleTeacherCount,
                InstitutionFemaleTeacherCount = @InstitutionFemaleTeacherCount,
                InstitutionTotalTeacherCount = @InstitutionTotalTeacherCount,
                InstitutionTotalStudentCount = @InstitutionTotalStudentCount,
                FinancialYearStart = @FinancialYearStart,
                FinancialYearEnd = @FinancialYearEnd,
                CurrentStatus = @CurrentStatus,
                ModifyBy = @ModifyBy,
                ModifyDate = GETDATE()
            WHERE Id = @Id;

            
        END

		DELETE FROM dbo.InstitutionGradeSections 
            WHERE InstitutionId = @Id;

        INSERT INTO dbo.InstitutionGradeSections (InstitutionId, GradeId, Sections)
        SELECT @Id, GradeId, Sections
        FROM OPENJSON(@jsonGradeSection) 
        WITH (
            GradeId INT, 
            Sections VARCHAR(55)
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        -- Optionally, you can handle the error or re-throw it
        THROW;
    END CATCH
END