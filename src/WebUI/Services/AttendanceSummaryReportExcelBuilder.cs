using ClosedXML.Excel;
using Core.DTOs;

namespace WebUI.Services;

public static class AttendanceSummaryReportExcelBuilder
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static string CreateFileName(DateTime? timestamp = null) =>
        $"Student_Attendance_Summary_{(timestamp ?? DateTime.Now):yyyyMMdd_HHmmss}.xlsx";

    public static byte[] Build(IEnumerable<StudentAttendanceSummaryReportDTO> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance Summary");
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

    private static List<(string Header, Func<StudentAttendanceSummaryReportDTO, string?> Getter)> GetExcelColumns() =>
    [
        ("Student Id", x => x.StudentId),
        ("Student Name", x => x.StudentName),
        ("Institution", x => x.InstitutionName),
        ("Grade", x => x.GradeName),
        ("Section", x => x.Section),
        ("Present", x => x.PresentCount.ToString()),
        ("Absent", x => x.AbsentCount.ToString()),
        ("Holiday", x => x.HolidayCount.ToString()),
        ("Working Days", x => x.WorkingDays.ToString()),
        ("Attendance %", x => x.AttendancePercent.ToString("0.##"))
    ];
}
