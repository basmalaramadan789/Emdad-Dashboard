namespace Emdad_Dashboard.Models
{
    public class Attendance
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public Shift Shift { get; set; }
        public AttendanceStatus Status { get; set; }
        public DateOnly DateOnly { get; set; } // 1/8   2/8
    }

    public enum Shift {
        AM,
        PM
    }

    public enum AttendanceStatus { 
        P,U,O,V,E,Z,OV
    }
}
