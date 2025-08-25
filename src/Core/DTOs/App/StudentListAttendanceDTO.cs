public partial class StudentListAttendanceDTO 
{
    public int SrNo { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string EnrollmentDate { get; set; } = string.Empty;
    public int Age { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public int Id { get; set; }
    public DateTime? AttendanceDate { get; set; }
    public int AttendanceStatus { get; set; }  
    
}