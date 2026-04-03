-- Kadam Programme Report - Matches Report Format Excel
-- Returns student data with institution, location, family, baseline, endline, mainstream details
CREATE OR ALTER PROCEDURE [dbo].[usp_KadamProgrammeReport]
    @UserId INT = NULL  -- Filter by user's institutions if provided
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @InstitutionIds VARCHAR(2000) = NULL;
    IF @UserId IS NOT NULL AND @UserId > 0
        SELECT @InstitutionIds = InstitutionIds FROM PeopleInstitutions WHERE UserId = @UserId;

    SELECT
        ROW_NUMBER() OVER (ORDER BY s.DateCreated, s.Id) AS SrNo,
        u.FirstName + ' ' + u.LastName AS CreatedBy,
        u.Email AS UserId,
        st.StateName AS [State],
        d.DivisionName AS [Division],
        dist.DistrictName AS [District],
        b.BlockName AS [Block],
        v.VillageName AS [Village],
        CASE i.InstitutionType WHEN 1 THEN 'Primary School' WHEN 2 THEN 'Middle School' WHEN 3 THEN 'High School' WHEN 4 THEN 'Pre School' WHEN 5 THEN 'DIET' ELSE '' END AS TypeOfInstitution,
        CASE i.InstitutionBuilding WHEN 1 THEN 'Public' WHEN 2 THEN 'Private' WHEN 3 THEN 'Govt. School' ELSE '' END AS InstitutionBuilding,
        i.InstitutionName,
        i.InstitutionCode AS InstitutionDISECode,
        i.InstitutionHeadMasterName AS TeachersName,
        s.StudentId AS StudentsKadamId,
        s.FirstName AS StudentsFirstName,
        s.LastName AS StudentsLastName,
        CONVERT(VARCHAR(10), s.DateOfBirth, 120) AS StudentsDateOfBirth,
        s.Age AS StudentsAge,
        CASE s.GenderId WHEN 1 THEN 'Male' WHEN 2 THEN 'Female' WHEN 3 THEN 'Other' ELSE '' END AS StudentsGender,
        CASE WHEN s.DoYouHaveAadhaarCard = 1 THEN 'Yes' ELSE 'No' END AS DoYouHaveAadhaarCard,
        s.AadhaarCardNumber AS StudentAadharNo,
        sf.FatherName AS FathersName,
        CAST(sf.FatherAge AS VARCHAR) AS FathersAge,
        CASE sf.FatherOccupationId WHEN 1 THEN 'Self-employed' WHEN 2 THEN 'Daily Wages Labourer' WHEN 3 THEN 'Govt. Service' WHEN 4 THEN 'Non Govt. Service' WHEN 5 THEN 'Business' WHEN 6 THEN 'Unemployed' WHEN 9 THEN 'Housewife' WHEN 10 THEN 'Farmer' WHEN 11 THEN 'Driver' WHEN 7 THEN 'Other' WHEN 8 THEN 'NA' ELSE '' END AS FathersOccupation,
        CASE sf.FatherEducationId WHEN 1 THEN 'Below 7th' WHEN 2 THEN '7th' WHEN 3 THEN '8th' WHEN 4 THEN '9th' WHEN 5 THEN '10th' WHEN 6 THEN 'HSC' WHEN 7 THEN 'Graduation' WHEN 8 THEN 'No Formal Education' WHEN 9 THEN 'NA' ELSE '' END AS FathersEducation,
        sf.MotherName AS MothersName,
        CAST(sf.MotherAge AS VARCHAR) AS MothersAge,
        CASE sf.MotherOccupationId WHEN 1 THEN 'Self-employed' WHEN 2 THEN 'Daily Wages Labourer' WHEN 3 THEN 'Govt. Service' WHEN 4 THEN 'Non Govt. Service' WHEN 5 THEN 'Business' WHEN 6 THEN 'Unemployed' WHEN 9 THEN 'Housewife' WHEN 10 THEN 'Farmer' WHEN 11 THEN 'Driver' WHEN 7 THEN 'Other' WHEN 8 THEN 'NA' ELSE '' END AS MothersOccupation,
        CASE sf.MotherEducationId WHEN 1 THEN 'Below 7th' WHEN 2 THEN '7th' WHEN 3 THEN '8th' WHEN 4 THEN '9th' WHEN 5 THEN '10th' WHEN 6 THEN 'HSC' WHEN 7 THEN 'Graduation' WHEN 8 THEN 'No Formal Education' WHEN 9 THEN 'NA' ELSE '' END AS MothersEducation,
        CONVERT(VARCHAR(10), s.EnrollmentDate, 120) AS EnrolmentDate,
        ge.GradeName AS EnrolmentGrade,
        s.Section,
        g.GradeName AS CurrentGrade,
        s.Section AS CurrentSection,
        CONVERT(VARCHAR(10), s.PromotionDate, 120) AS PromotionDate,
        s.StudentRegistratioNumber AS StudentsSRGivenByInstitution,
        CASE s.ChildStatudBeforeKadamSTC WHEN 1 THEN 'Never Enrolled in School' WHEN 2 THEN 'Enrolled but not Attending' WHEN 3 THEN 'Dropped-Out' ELSE '' END AS SchoolingStatus,
        s.DropoutClass AS DroppedOutClass,
        CAST(s.DropoutYear AS VARCHAR) AS DroppedOutYear,
        CASE s.ReasonId WHEN 1 THEN 'Migration of the family' WHEN 2 THEN 'Financial Constraints' WHEN 3 THEN 'Obligation to do household chores' WHEN 4 THEN 'Child not comfortable in school environment' WHEN 5 THEN 'Health related issues' WHEN 6 THEN 'Others' ELSE '' END AS Reasons,
        CASE s.HowLongPlaningToStayThisArea WHEN 1 THEN 'Less than 3 Months' WHEN 2 THEN '3-6 months' WHEN 3 THEN '6-12 months' WHEN 4 THEN '1-3 years' WHEN 5 THEN 'More than 4 years' ELSE '' END AS HowLongPlanningToStay,
        sf.PrimaryContactNumber AS ContactNo,
        sf.AlternateContactNumber AS AlternateContactNumber,
        sf.HouseAddress,
        sf.PinCode AS Pincode,
        CASE sf.PeopleInHouseId WHEN 2 THEN '2' WHEN 3 THEN '3' WHEN 4 THEN '4' WHEN 5 THEN '5' WHEN 6 THEN '6' WHEN 7 THEN '7' WHEN 8 THEN '8' WHEN 9 THEN '9' WHEN 10 THEN '10' WHEN 11 THEN '11' WHEN 12 THEN '12' WHEN 13 THEN '13' WHEN 14 THEN '14' WHEN 15 THEN '15' WHEN 16 THEN '>15' ELSE '' END AS PeopleLivingInHouse,
        CASE sf.CasteId WHEN 1 THEN 'SC' WHEN 2 THEN 'ST' WHEN 3 THEN 'NT' WHEN 4 THEN 'OBC' WHEN 5 THEN 'General' ELSE '' END AS Cast,
        CASE sf.ReligionId WHEN 1 THEN 'Hindu' WHEN 2 THEN 'Muslim' WHEN 3 THEN 'Christian' WHEN 4 THEN 'Buddhism' WHEN 5 THEN 'Jain' WHEN 6 THEN 'Sikh' WHEN 7 THEN 'Other Religion' ELSE '' END AS Religion,
        CAST(sf.ParentMonthlyIncome AS VARCHAR) AS ParentsMonthlyIncome,
        CAST(sf.ParentMontlyExpenditure AS VARCHAR) AS ParentsMonthlyExpenditure,
        '' AS IsChildPhysicallyChallenged,
        '' AS PhysicallyChallengedType,
        '' AS PhysicallyChallengedPercent,
        '' AS DisabilityCertificateAvailable,
        '' AS TypeOfDocument,
        '' AS DocumentNumber,
        '' AS DocumentAvailable,
        '' AS TrioNo,
        CAST(baselineMath.ObtainedMarks AS VARCHAR) AS BaselineMath,
        CAST(baselineEng.ObtainedMarks AS VARCHAR) AS BaselineEnglish,
        CAST(baselineEVS.ObtainedMarks AS VARCHAR) AS BaselineEVS,
        CAST(baselineHindi.ObtainedMarks AS VARCHAR) AS BaselineHindi,
        CAST(baselineTotal.TotalMarks AS VARCHAR) AS BaselineTotal,
        CAST(baselineTotal.Pct AS VARCHAR) AS BaselinePercentage,
        CONVERT(VARCHAR(10), baselineTotal.CompletedDate, 120) AS BaselineDate,
        CAST(sge.EntryStepId AS VARCHAR) AS EntryLevelStep,
        CAST(sge.ExitStepId AS VARCHAR) AS ExitLevelStep,
        CAST(ongoing.LastStepId AS VARCHAR) AS OngoingStep,
        CAST(ongoing.StepCount AS VARCHAR) AS NoOfStepsCompleted,
        '' AS GradeTest1,
        '' AS GradeTest2,
        '' AS GradeTest3,
        '' AS GradeTest4,
        '' AS GradeTest5,
        '' AS ProgressSpeed,
        '' AS ProgressColor,
        '' AS MonthsRequiredToReachAgeAppropriateLevel,
        '' AS NoOfSchoolUniformReceived,
        '' AS SchoolUniformReceivedDate,
        '' AS NoOfStationeryKitReceived,
        '' AS StationeryKitReceivedLastDate,
        '' AS AttendancePercent,
        '' AS MidDayMealPercent,
        CASE s.CurrentStatus WHEN 1 THEN 'Active' WHEN 2 THEN 'Inactive' WHEN 3 THEN 'Closed' ELSE '' END AS StudentsStatus,
        CAST(endlineMath.ObtainedMarks AS VARCHAR) AS EndlineMath,
        CAST(endlineEng.ObtainedMarks AS VARCHAR) AS EndlineEnglish,
        CAST(endlineEVS.ObtainedMarks AS VARCHAR) AS EndlineEVS,
        CAST(endlineHindi.ObtainedMarks AS VARCHAR) AS EndlineHindi,
        CAST(endlineTotal.TotalMarks AS VARCHAR) AS EndlineTotal,
        CAST(endlineTotal.Pct AS VARCHAR) AS EndlinePercentage,
        CONVERT(VARCHAR(10), endlineTotal.CompletedDate, 120) AS EndlineDate,
        CASE WHEN sm.Id IS NOT NULL THEN (CASE WHEN sm.IsMainstreamInstitutionSame = 1 THEN 'Yes' ELSE 'No' END) ELSE '' END AS IsMainstreamInstitutionSame,
        sm.MainstreamInstitutionName,
        sm.SchoolDISECode,
        gm.GradeName AS MainstreamGrade,
        sm.Section AS MainstreamSection,
        sm.ChildSRNumber AS ChildSRGivenByInstitution,
        CONVERT(VARCHAR(10), sm.MainstreamDate, 120) AS MainstreamDate,
        '' AS CurrentGradeAfterMainstream,
        CONVERT(VARCHAR(10), s.InActiveDate, 120) AS InactiveDate,
        s.InActiveReason AS InactiveReasons,
        s.Remarks AS InactiveRemarks,
        '' AS BroughtBackDate,
        '' AS BroughtBackReason,
        '' AS LastDateOfInactiveFollowUp,
        '' AS ResponseToPhoneCall,
        '' AS EnrolledInSchool,
        '' AS SchoolName,
        '' AS EnrolmentGradeInactiveFollowUp,
        '' AS CurrentAddressState,
        '' AS StatusAfterMainstream,
        '' AS StudentAttendanceToday,
        '' AS StudentAttendancePercentLastMonth,
        '' AS LastExamPercent
    FROM Students s
    INNER JOIN Institutions i ON s.InstitutionId = i.Id AND i.IsDeleted = 0
    LEFT JOIN Users u ON s.CreatedBy = u.Id
    LEFT JOIN States st ON i.StateId = st.Id
    LEFT JOIN Divisions d ON i.DivisionId = d.Id
    LEFT JOIN Districts dist ON i.DistrictId = dist.Id
    LEFT JOIN Blocks b ON i.BlockId = b.Id
    LEFT JOIN Villages v ON i.VillageId = v.Id
    LEFT JOIN Grades g ON s.GradeId = g.Id
    LEFT JOIN Grades ge ON s.GradeId = ge.Id
    LEFT JOIN StudentFamilyDetails sf ON s.Id = sf.StudentId AND sf.IsDeleted = 0
    LEFT JOIN StudentGradeStartAndEndDetails sge ON s.Id = sge.StudentId
    LEFT JOIN (
        SELECT StudentId, MAX(StepId) AS LastStepId, COUNT(*) AS StepCount
        FROM StudentProgressSteps
        GROUP BY StudentId
    ) ongoing ON s.Id = ongoing.StudentId
    LEFT JOIN (
        SELECT StudentId, SubjectId, ObtainedMarks
        FROM StudentBaselineDetails
        WHERE BaselineType = 'baselinepreAssessment' AND IsDeleted = 0
    ) baselineMath ON s.Id = baselineMath.StudentId AND baselineMath.SubjectId = (SELECT TOP 1 Id FROM Subjects WHERE SubjectName LIKE '%Math%' AND IsDeleted = 0)
    LEFT JOIN (
        SELECT StudentId, SubjectId, ObtainedMarks
        FROM StudentBaselineDetails
        WHERE BaselineType = 'baselinepreAssessment' AND IsDeleted = 0
    ) baselineEng ON s.Id = baselineEng.StudentId AND baselineEng.SubjectId = (SELECT TOP 1 Id FROM Subjects WHERE SubjectName LIKE '%English%' AND IsDeleted = 0)
    LEFT JOIN (
        SELECT StudentId, SubjectId, ObtainedMarks
        FROM StudentBaselineDetails
        WHERE BaselineType = 'baselinepreAssessment' AND IsDeleted = 0
    ) baselineEVS ON s.Id = baselineEVS.StudentId AND baselineEVS.SubjectId = (SELECT TOP 1 Id FROM Subjects WHERE SubjectName LIKE '%EVS%' AND IsDeleted = 0)
    LEFT JOIN (
        SELECT StudentId, SubjectId, ObtainedMarks
        FROM StudentBaselineDetails
        WHERE BaselineType = 'baselinepreAssessment' AND IsDeleted = 0
    ) baselineHindi ON s.Id = baselineHindi.StudentId AND baselineHindi.SubjectId = (SELECT TOP 1 Id FROM Subjects WHERE SubjectName LIKE '%Hindi%' AND IsDeleted = 0)
    LEFT JOIN (
        SELECT StudentId, SUM(ObtainedMarks) AS TotalMarks, MAX(PercentageMarks) AS Pct, MAX(CompletedDate) AS CompletedDate
        FROM StudentBaselineDetails
        WHERE BaselineType = 'baselinepreAssessment' AND IsDeleted = 0
        GROUP BY StudentId
    ) baselineTotal ON s.Id = baselineTotal.StudentId
    LEFT JOIN (
        SELECT StudentId, SubjectId, ObtainedMarks
        FROM StudentBaselineDetails
        WHERE BaselineType = 'endlinepreAssessment' AND IsDeleted = 0
    ) endlineMath ON s.Id = endlineMath.StudentId AND endlineMath.SubjectId = (SELECT TOP 1 Id FROM Subjects WHERE SubjectName LIKE '%Math%' AND IsDeleted = 0)
    LEFT JOIN (
        SELECT StudentId, SubjectId, ObtainedMarks
        FROM StudentBaselineDetails
        WHERE BaselineType = 'endlinepreAssessment' AND IsDeleted = 0
    ) endlineEng ON s.Id = endlineEng.StudentId AND endlineEng.SubjectId = (SELECT TOP 1 Id FROM Subjects WHERE SubjectName LIKE '%English%' AND IsDeleted = 0)
    LEFT JOIN (
        SELECT StudentId, SubjectId, ObtainedMarks
        FROM StudentBaselineDetails
        WHERE BaselineType = 'endlinepreAssessment' AND IsDeleted = 0
    ) endlineEVS ON s.Id = endlineEVS.StudentId AND endlineEVS.SubjectId = (SELECT TOP 1 Id FROM Subjects WHERE SubjectName LIKE '%EVS%' AND IsDeleted = 0)
    LEFT JOIN (
        SELECT StudentId, SubjectId, ObtainedMarks
        FROM StudentBaselineDetails
        WHERE BaselineType = 'endlinepreAssessment' AND IsDeleted = 0
    ) endlineHindi ON s.Id = endlineHindi.StudentId AND endlineHindi.SubjectId = (SELECT TOP 1 Id FROM Subjects WHERE SubjectName LIKE '%Hindi%' AND IsDeleted = 0)
    LEFT JOIN (
        SELECT StudentId, SUM(ObtainedMarks) AS TotalMarks, MAX(PercentageMarks) AS Pct, MAX(CompletedDate) AS CompletedDate
        FROM StudentBaselineDetails
        WHERE BaselineType = 'endlinepreAssessment' AND IsDeleted = 0
        GROUP BY StudentId
    ) endlineTotal ON s.Id = endlineTotal.StudentId
    LEFT JOIN StudentMainstreams sm ON s.Id = sm.StudentId
    LEFT JOIN Grades gm ON sm.GradeId = gm.Id
    WHERE s.IsDeleted = 0
    AND (@InstitutionIds IS NULL OR i.Id IN (SELECT Item FROM dbo.SplitString(@InstitutionIds, ',')))
    ORDER BY s.DateCreated, s.Id;
END
GO
