using ClosedXML.Excel;
using Core.DTOs.App;

namespace WebUI.Services;

public static class FollowupReportExcelBuilder
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static string CreateFileName(DateTime? timestamp = null) =>
        $"Student_Followup_Report_{(timestamp ?? DateTime.Now):yyyyMMdd_HHmmss}.xlsx";

    public static byte[] Build(IEnumerable<StudentFollowupListDTO> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Student Follow-up");
        var columns = GetExcelColumns();

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

    private static string FormatPercent(float? value) =>
        value.HasValue ? value.Value.ToString("0.##") : string.Empty;

    private static List<(string Header, Func<StudentFollowupListDTO, string?> Getter)> GetExcelColumns() =>
    [
        ("Visit Date", x => x.FollowupDate.ToString("dd-MMM-yyyy")),
        ("Institution", x => x.InstitutionName),
        ("Grade", x => x.GradeName),
        ("Section", x => x.Section),
        ("Incharge", x => x.InchargeName),
        ("Contact", x => x.InchargeContactNumber),
        ("Sit together", x => x.IsChildSitTogether),
        ("Last month present", x => x.LastMonthAttendanceCount?.ToString()),
        ("Last month working days", x => x.LastMonthWorkingDayCount?.ToString()),
        ("Last month %", x => FormatPercent(x.LastMonthAttendancePercentage)),
        ("Male", x => x.MaleStudentCount?.ToString()),
        ("Female", x => x.FemaleStudentCount?.ToString()),
        ("Present today", x => x.TodayStudentPresentCount?.ToString()),
        ("Total students", x => x.TotalStudentCount?.ToString()),
        ("Today %", x => FormatPercent(x.TotalStudentPercentage)),
        ("Created By", x => x.CreatedByName)
    ];
}
