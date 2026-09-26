using ClosedXML.Excel;
using Core.DTOs.App;

namespace WebUI.Services;

public static class ThemeActivityReportExcelBuilder
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static string CreateFileName(DateTime? timestamp = null) =>
        $"Theme_Activity_Report_{(timestamp ?? DateTime.Now):yyyyMMdd_HHmmss}.xlsx";

    public static byte[] Build(IEnumerable<ThemeActivityReportDTO> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Theme Activity");
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

    private static string FormatActivityDate(DateTime? value) =>
        value.HasValue ? value.Value.ToString("dd-MMM-yyyy") : string.Empty;

    private static List<(string Header, Func<ThemeActivityReportDTO, string?> Getter)> GetExcelColumns() =>
    [
        ("Activity Date", x => FormatActivityDate(x.ThemeActivityDate)),
        ("State", x => x.StateName),
        ("Division", x => x.DivisionName),
        ("Institution", x => x.InstitutionName),
        ("Theme", x => x.ThemeName),
        ("Grades / Sections", x => x.GradeSectionsText),
        ("Eligible students", x => x.TotalStudents.ToString()),
        ("Students attended", x => x.StudentAttended.ToString()),
        ("Children's Day", x => x.DidChildrenDayHappen ? "Yes" : "No"),
        ("Parents attended", x => x.TotalParentsAttended?.ToString()),
        ("Total participants", x => x.TotalParticipants.ToString()),
        ("Source", x => x.EntryPointText),
        ("Created By", x => x.CreatedByName)
    ];
}
