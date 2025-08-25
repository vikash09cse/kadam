using System;

namespace Core.Entities
{
    public class StudentAttendance 
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int AttendanceStatus { get; set; }
        public string? AttendanceNote { get; set; }
        public int CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public int ModifyBy { get; set; }
        public DateTime ModifyDate { get; set; }
       
    }
}
