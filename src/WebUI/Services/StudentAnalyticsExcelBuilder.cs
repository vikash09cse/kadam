using ClosedXML.Excel;
using Core.DTOs.App;

namespace WebUI.Services;

public static class StudentAnalyticsExcelBuilder
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static string CreateBreakdownFileName(DateTime? timestamp = null) =>
        $"Student_Analytics_Breakdown_{(timestamp ?? DateTime.Now):yyyyMMdd_HHmmss}.xlsx";

    public static string CreateExceptionsFileName(DateTime? timestamp = null) =>
        $"Student_Exceptions_{(timestamp ?? DateTime.Now):yyyyMMdd_HHmmss}.xlsx";

    public static byte[] BuildBreakdown(IEnumerable<StudentAnalyticsBreakdownDTO> data, string groupBy)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Breakdown");
        var dimensionHeader = string.Equals(groupBy, "INSTITUTION", StringComparison.OrdinalIgnoreCase)
            ? "Institution"
            : "Division";

        var columns = GetBreakdownColumns(dimensionHeader);
        for (var col = 1; col <= columns.Count; col++)
            worksheet.Cell(1, col).Value = columns[col - 1].Header;

        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        foreach (var item in data)
        {
            for (var col = 1; col <= columns.Count; col++)
                worksheet.Cell(row, col).Value = columns[col - 1].Getter(item) ?? string.Empty;
            row++;
        }

        worksheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream, false);
        return stream.ToArray();
    }

    public static byte[] BuildExceptions(IEnumerable<StudentExceptionListItemDTO> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Exceptions");
        var columns = GetExceptionColumns();

        for (var col = 1; col <= columns.Count; col++)
            worksheet.Cell(1, col).Value = columns[col - 1].Header;

        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        foreach (var item in data)
        {
            for (var col = 1; col <= columns.Count; col++)
                worksheet.Cell(row, col).Value = columns[col - 1].Getter(item) ?? string.Empty;
            row++;
        }

        worksheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream, false);
        return stream.ToArray();
    }

    private static List<(string Header, Func<StudentAnalyticsBreakdownDTO, string?> Getter)> GetBreakdownColumns(string dimensionHeader) =>
    [
        (dimensionHeader, x => x.DimensionName),
        ("State", x => x.StateName),
        ("Total Students", x => x.TotalStudents.ToString()),
        ("Active", x => x.ActiveCount.ToString()),
        ("Inactive", x => x.InactiveCount.ToString()),
        ("Completed", x => x.CompletedCount.ToString()),
        ("Baseline Completed", x => x.BaselineCompletedCount.ToString()),
        ("Baseline %", x => x.BaselineCompletedPercent.ToString("0.##")),
        ("Progress Started", x => x.ProgressStartedCount.ToString()),
        ("Steps Completed", x => x.StepsCompletedCount.ToString()),
        ("Endline Completed", x => x.EndlineCompletedCount.ToString()),
        ("Mainstreamed", x => x.MainstreamedCount.ToString()),
        ("Mainstream Eligible", x => x.MainstreamEligibleCount.ToString())
    ];

    private static List<(string Header, Func<StudentExceptionListItemDTO, string?> Getter)> GetExceptionColumns() =>
    [
        ("Exception", x => x.ExceptionLabel),
        ("Student Id", x => x.StudentCode),
        ("Student Name", x => x.StudentName),
        ("Institution", x => x.InstitutionName),
        ("Division", x => x.DivisionName),
        ("State", x => x.StateName),
        ("Grade", x => x.GradeName),
        ("Section", x => x.Section),
        ("Enrollment Date", x => x.EnrollmentDate?.ToString("dd-MMM-yyyy")),
        ("Status", x => x.CurrentStatusText),
        ("Kadam Plus", x => x.IsKadamPlusStudent ? "Yes" : "No"),
        ("Last Step", x => x.LastProgressStepId?.ToString()),
        ("Exit Step", x => x.ExitStepId?.ToString()),
        ("Baseline Date", x => x.BaselineCompletedDate?.ToString("dd-MMM-yyyy")),
        ("Endline Date", x => x.EndlineCompletedDate?.ToString("dd-MMM-yyyy")),
        ("Inactive Date", x => x.InActiveDate?.ToString("dd-MMM-yyyy")),
        ("Days Overdue", x => x.DaysOverdue?.ToString())
    ];
}
