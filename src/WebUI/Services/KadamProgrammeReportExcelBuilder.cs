using ClosedXML.Excel;
using Core.DTOs;

namespace WebUI.Services;

public static class KadamProgrammeReportExcelBuilder
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static string CreateFileName(DateTime? timestamp = null) =>
        $"Kadam_Programme_Report_{(timestamp ?? DateTime.Now):yyyyMMdd_HHmmss}.xlsx";

    public static byte[] Build(IEnumerable<KadamProgrammeReportDTO> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Kadam Programme Report");
        var headerColumns = GetReportColumns();

        for (var col = 1; col <= headerColumns.Count; col++)
        {
            worksheet.Cell(1, col).Value = headerColumns[col - 1].Header;
        }

        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        foreach (var item in data)
        {
            for (var col = 1; col <= headerColumns.Count; col++)
            {
                worksheet.Cell(row, col).Value = headerColumns[col - 1].Getter(item) ?? "";
            }

            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream, false);
        return stream.ToArray();
    }

    private static List<(string Header, Func<KadamProgrammeReportDTO, string?> Getter)> GetReportColumns() =>
    [
        ("Sr. No.", x => x.SrNo.ToString()),
        ("Created By", x => x.CreatedBy),
        ("User ID", x => x.UserId),
        ("State", x => x.State),
        ("Division", x => x.Division),
        ("District", x => x.District),
        ("Block", x => x.Block),
        ("Village", x => x.Village),
        ("Type Of Institution", x => x.TypeOfInstitution),
        ("Institution Building", x => x.InstitutionBuilding),
        ("Institution Name", x => x.InstitutionName),
        ("Institution DISE Code", x => x.InstitutionDISECode),
        ("Teacher's Name", x => x.TeachersName),
        ("Student's Kadam Id", x => x.StudentsKadamId),
        ("Student's First Name", x => x.StudentsFirstName),
        ("Student's Last Name", x => x.StudentsLastName),
        ("Student's Date of Birth", x => x.StudentsDateOfBirth),
        ("Student's Age", x => x.StudentsAge),
        ("Student's Gender", x => x.StudentsGender),
        ("Do you have Aadhaar Card ?", x => x.DoYouHaveAadhaarCard),
        ("Student Aadhar No.", x => x.StudentAadharNo),
        ("Father's Name", x => x.FathersName),
        ("Father's Age", x => x.FathersAge),
        ("Father's Occupation", x => x.FathersOccupation),
        ("Father's Education", x => x.FathersEducation),
        ("Mother's Name", x => x.MothersName),
        ("Mother's Age", x => x.MothersAge),
        ("Mother's Occupation", x => x.MothersOccupation),
        ("Mother's Education", x => x.MothersEducation),
        ("Enrolment Date", x => x.EnrolmentDate),
        ("Enrolment Grade", x => x.EnrolmentGrade),
        ("Section", x => x.Section),
        ("Current Grade", x => x.CurrentGrade),
        ("Current Section", x => x.CurrentSection),
        ("Promotion Date", x => x.PromotionDate),
        ("Student's SR Given By The Institution", x => x.StudentsSRGivenByInstitution),
        ("Schooling Status", x => x.SchoolingStatus),
        ("Dropped Out Class", x => x.DroppedOutClass),
        ("Dropped-out Year", x => x.DroppedOutYear),
        ("Reasons", x => x.Reasons),
        ("How long are you planning to stay in this area?", x => x.HowLongPlanningToStay),
        ("Contact No.", x => x.ContactNo),
        ("Alternate Contact Number", x => x.AlternateContactNumber),
        ("House Address", x => x.HouseAddress),
        ("Pincode", x => x.Pincode),
        ("People living in house", x => x.PeopleLivingInHouse),
        ("Cast", x => x.Cast),
        ("Religion", x => x.Religion),
        ("Parents' Monthly Income", x => x.ParentsMonthlyIncome),
        ("Parents' Monthly Expenditure", x => x.ParentsMonthlyExpenditure),
        ("Is the child physically challenged ?", x => x.IsChildPhysicallyChallenged),
        ("Physically Challenged Type", x => x.PhysicallyChallengedType),
        ("% of Physically Challenged", x => x.PhysicallyChallengedPercent),
        ("Disability Certificate Available", x => x.DisabilityCertificateAvailable),
        ("Type of Document", x => x.TypeOfDocument),
        ("Document Number", x => x.DocumentNumber),
        ("Document Available", x => x.DocumentAvailable),
        ("Trio No.", x => x.TrioNo),
        ("Baseline Math", x => FormatSubjectValue(x.BaselineMath, HasBaseline(x))),
        ("Baseline English", x => FormatSubjectValue(x.BaselineEnglish, HasBaseline(x))),
        ("Baseline EVS", x => FormatSubjectValue(x.BaselineEVS, HasBaseline(x))),
        ("Baseline Hindi", x => FormatSubjectValue(x.BaselineHindi, HasBaseline(x))),
        ("Baseline Total", x => x.BaselineTotal),
        ("Total Marks", x => x.BaselineTotalMarks),
        ("Baseline Percentage", x => x.BaselinePercentage),
        ("Baseline Date", x => x.BaselineDate),
        ("Entry-Level Step", x => x.EntryLevelStep),
        ("Exit Level Step", x => x.ExitLevelStep),
        ("Ongoing Step", x => FormatOngoingStep(x.OngoingStep, x.ExitLevelStep)),
        ("No. of Steps Completed", x => x.NoOfStepsCompleted),
        ("Grade Test 1", x => x.GradeTest1),
        ("Grade Test 1 %", x => x.GradeTest1Percentage),
        ("Grade Test 1 Date", x => x.GradeTest1Date),
        ("Grade Test 2", x => x.GradeTest2),
        ("Grade Test 2 %", x => x.GradeTest2Percentage),
        ("Grade Test 2 Date", x => x.GradeTest2Date),
        ("Grade Test 3", x => x.GradeTest3),
        ("Grade Test 3 %", x => x.GradeTest3Percentage),
        ("Grade Test 3 Date", x => x.GradeTest3Date),
        ("Grade Test 4", x => x.GradeTest4),
        ("Grade Test 4 %", x => x.GradeTest4Percentage),
        ("Grade Test 4 Date", x => x.GradeTest4Date),
        ("Grade Test 5", x => x.GradeTest5),
        ("Grade Test 5 %", x => x.GradeTest5Percentage),
        ("Grade Test 5 Date", x => x.GradeTest5Date),
        ("Progress Speed", x => x.ProgressSpeed),
        ("Progress Color", x => x.ProgressColor),
        ("Months required to reach Age appropriate level", x => x.MonthsRequiredToReachAgeAppropriateLevel),
        ("No. of School Uniform Received", x => x.NoOfSchoolUniformReceived),
        ("School Uniform Received Date", x => x.SchoolUniformReceivedDate),
        ("No. of Stationery Kit Received", x => x.NoOfStationeryKitReceived),
        ("Stationery Kit Received Last Date", x => x.StationeryKitReceivedLastDate),
        ("Attendance %", x => x.AttendancePercent),
        ("Mid-Day-Meal %", x => x.MidDayMealPercent),
        ("Student's Status", x => x.StudentsStatus),
        ("Endline Math", x => FormatSubjectValue(x.EndlineMath, HasEndline(x))),
        ("Endline English", x => FormatSubjectValue(x.EndlineEnglish, HasEndline(x))),
        ("Endline EVS", x => FormatSubjectValue(x.EndlineEVS, HasEndline(x))),
        ("Endline Hindi", x => FormatSubjectValue(x.EndlineHindi, HasEndline(x))),
        ("Endline Total", x => x.EndlineTotal),
        ("Endline Total Marks", x => x.EndlineTotalMarks),
        ("Endline Percentage", x => x.EndlinePercentage),
        ("Endline Date", x => x.EndlineDate),
        ("Is Mainstream Institution Same?", x => x.IsMainstreamInstitutionSame),
        ("Mainstream Institution Name", x => x.MainstreamInstitutionName),
        ("School DISE Code", x => x.SchoolDISECode),
        ("Mainstream Grade", x => x.MainstreamGrade),
        ("Mainstream Section", x => x.MainstreamSection),
        ("Child SR given by the Institution", x => x.ChildSRGivenByInstitution),
        ("Mainstream Date", x => x.MainstreamDate),
        ("Current Grade After Mainstream", x => x.CurrentGradeAfterMainstream),
        ("Inactive Date", x => x.InactiveDate),
        ("Inactive Reasons", x => x.InactiveReasons),
        ("Inactive Remarks", x => x.InactiveRemarks),
        ("Brought Back Date", x => x.BroughtBackDate),
        ("Brought Back Reason", x => x.BroughtBackReason),
        ("Last Date of Inactive Follow Up", x => x.LastDateOfInactiveFollowUp),
        ("Response to Phone Call", x => x.ResponseToPhoneCall),
        ("Enrolled in School", x => x.EnrolledInSchool),
        ("School Name", x => x.SchoolName),
        ("Enrolment Grade (Inactive Follow Up)", x => x.EnrolmentGradeInactiveFollowUp),
        ("Current Address(State)", x => x.CurrentAddressState),
        ("Status after Mainstream", x => x.StatusAfterMainstream),
        ("Student Attendance Today: P/A", x => x.StudentAttendanceToday),
        ("Student Attendance % Last Month", x => x.StudentAttendancePercentLastMonth),
        ("Last Exam % (If Any)", x => x.LastExamPercent)
    ];

    private static bool HasBaseline(KadamProgrammeReportDTO item) =>
        HasAssessment(
            item.BaselineDate,
            item.BaselineTotal,
            item.BaselineTotalMarks,
            item.BaselinePercentage,
            item.BaselineMath,
            item.BaselineEnglish,
            item.BaselineEVS,
            item.BaselineHindi);

    private static bool HasEndline(KadamProgrammeReportDTO item) =>
        HasAssessment(
            item.EndlineDate,
            item.EndlineTotal,
            item.EndlineTotalMarks,
            item.EndlinePercentage,
            item.EndlineMath,
            item.EndlineEnglish,
            item.EndlineEVS,
            item.EndlineHindi);

    private static bool HasAssessment(params string?[] values) =>
        values.Any(value => !string.IsNullOrWhiteSpace(value));

    private static string? FormatSubjectValue(string? value, bool assessmentSubmitted)
    {
        if (!assessmentSubmitted)
            return string.Empty;

        if (string.IsNullOrWhiteSpace(value) ||
            (decimal.TryParse(value, out var marks) && marks == 0))
            return "0";

        return value;
    }

    private static string? FormatOngoingStep(string? ongoingStep, string? exitLevelStep)
    {
        if (string.IsNullOrWhiteSpace(ongoingStep) || string.IsNullOrWhiteSpace(exitLevelStep))
            return ongoingStep;

        if (int.TryParse(ongoingStep, out var ongoing) &&
            int.TryParse(exitLevelStep, out var exitLevel) &&
            ongoing == exitLevel)
            return "Completed";

        return ongoingStep;
    }
}
