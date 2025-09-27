/*
================================================================================
Procedure: usp_CheckDuplicateStudent
Purpose: Check for duplicate students based on various criteria
Author: System Generated
Created: 2024
================================================================================

Usage Examples:
1. Check all criteria (name, fathername, mothername, age, mobilenumber, schoolname):
   EXEC usp_CheckDuplicateStudent 
        @QueryType = 1,
        @FirstName = 'John',
        @LastName = 'Doe',
        @FatherName = 'Robert Doe',
        @MotherName = 'Jane Doe',
        @Age = 10,
        @MobileNumber = '9876543210',
        @SchoolName = 'ABC School'

2. Check by name only:
   EXEC usp_CheckDuplicateStudent 
        @QueryType = 2,
        @FirstName = 'John',
        @LastName = 'Doe'

3. Check by mobile number only:
   EXEC usp_CheckDuplicateStudent 
        @QueryType = 3,
        @MobileNumber = '9876543210'

4. Check by Aadhaar card number only:
   EXEC usp_CheckDuplicateStudent 
        @QueryType = 4,
        @AadhaarCardNumber = '123456789012'

5. Check all criteria excluding current student (for updates):
   EXEC usp_CheckDuplicateStudent 
        @QueryType = 1,
        @FirstName = 'John',
        @LastName = 'Doe',
        @FatherName = 'Robert Doe',
        @MotherName = 'Jane Doe',
        @Age = 10,
        @MobileNumber = '9876543210',
        @SchoolName = 'ABC School',
        @StudentId = 123

Returns:
- First result set: DuplicateCount, Id, StudentId, FullName, InstitutionName, AadhaarCardNumber
- Second result set: HasDuplicates (1 if duplicates found, 0 if no duplicates)
================================================================================
*/

CREATE OR ALTER PROCEDURE [dbo].[usp_CheckDuplicateStudent]
(
    @QueryType INT, -- 1: Check all criteria, 2: Check by name only, 3: Check by mobile only, 4: Check by Aadhaar only
    @FirstName VARCHAR(50) = NULL,
    @LastName VARCHAR(50) = NULL,
    @FatherName VARCHAR(50) = NULL,
    @MotherName VARCHAR(50) = NULL,
    @Age INT = NULL,
    @MobileNumber VARCHAR(20) = NULL,
    @SchoolName VARCHAR(100) = NULL,
    @AadhaarCardNumber VARCHAR(16) = NULL,
    @StudentId INT = 0 -- Optional: to exclude current student when updating
)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DuplicateCount INT = 0;
    DECLARE @DuplicateStudentIds TABLE (Id INT, StudentId VARCHAR(100), FullName VARCHAR(101), InstitutionName VARCHAR(100), AadhaarCardNumber VARCHAR(16));
    
    IF @QueryType = 1 -- Check all criteria (name, fathername, mothername, age, mobilenumber, schoolname)
    BEGIN
        INSERT INTO @DuplicateStudentIds
        SELECT 
            s.Id,
            s.StudentId,
            CONCAT(s.FirstName, ' ', s.LastName) AS FullName,
            i.InstitutionName,
            s.AadhaarCardNumber
        FROM Students s
        INNER JOIN StudentFamilyDetails sfd ON s.Id = sfd.StudentId
        INNER JOIN Institutions i ON s.InstitutionId = i.Id
        WHERE s.IsDeleted = 0
            AND sfd.IsDeleted = 0
            AND i.IsDeleted = 0
            AND s.FirstName = @FirstName
            AND s.LastName = @LastName
            AND sfd.FatherName = @FatherName
            AND sfd.MotherName = @MotherName
            AND s.Age = @Age
            AND sfd.PrimaryContactNumber = @MobileNumber
            AND i.InstitutionName = @SchoolName
            AND (@StudentId IS NULL OR s.Id != @StudentId);
    END
    ELSE IF @QueryType = 2 -- Check by name only (FirstName + LastName)
    BEGIN
        INSERT INTO @DuplicateStudentIds
        SELECT 
            s.Id,
            s.StudentId,
            CONCAT(s.FirstName, ' ', s.LastName) AS FullName,
            i.InstitutionName,
            s.AadhaarCardNumber
        FROM Students s
        INNER JOIN StudentFamilyDetails sfd ON s.Id = sfd.StudentId
        INNER JOIN Institutions i ON s.InstitutionId = i.Id
        WHERE s.IsDeleted = 0
            AND sfd.IsDeleted = 0
            AND i.IsDeleted = 0
            AND s.FirstName = @FirstName
            AND s.LastName = @LastName
            AND (@StudentId IS NULL OR s.Id != @StudentId);
    END
    ELSE IF @QueryType = 3 -- Check by mobile number only
    BEGIN
        INSERT INTO @DuplicateStudentIds
        SELECT 
            s.Id,
            s.StudentId,
            CONCAT(s.FirstName, ' ', s.LastName) AS FullName,
            i.InstitutionName,
            s.AadhaarCardNumber
        FROM Students s
        INNER JOIN StudentFamilyDetails sfd ON s.Id = sfd.StudentId
        INNER JOIN Institutions i ON s.InstitutionId = i.Id
        WHERE s.IsDeleted = 0
            AND sfd.IsDeleted = 0
            AND i.IsDeleted = 0
            AND sfd.PrimaryContactNumber = @MobileNumber
            AND (@StudentId IS NULL OR s.Id != @StudentId);
    END
    ELSE IF @QueryType = 4 -- Check by Aadhaar card number only
    BEGIN
        INSERT INTO @DuplicateStudentIds
        SELECT 
            s.Id,
            s.StudentId,
            CONCAT(s.FirstName, ' ', s.LastName) AS FullName,
            i.InstitutionName,
            s.AadhaarCardNumber
        FROM Students s
        INNER JOIN StudentFamilyDetails sfd ON s.Id = sfd.StudentId
        INNER JOIN Institutions i ON s.InstitutionId = i.Id
        WHERE s.IsDeleted = 0
            AND sfd.IsDeleted = 0
            AND i.IsDeleted = 0
            AND s.AadhaarCardNumber = @AadhaarCardNumber
            AND (@StudentId IS NULL OR s.Id != @StudentId);
    END
    
    SELECT @DuplicateCount = COUNT(*) FROM @DuplicateStudentIds;
    
    -- Return the count and details of duplicate students
    SELECT 
        @DuplicateCount AS DuplicateCount,
        Id,
        StudentId,
        FullName,
        InstitutionName,
        AadhaarCardNumber
    FROM @DuplicateStudentIds
    ORDER BY Id;
    
    -- Return a simple boolean result for easy checking
    SELECT 
        CASE 
            WHEN @DuplicateCount > 0 THEN 1 
            ELSE 0 
        END AS HasDuplicates;
        
END;
