namespace Emdad_Dashboard.Models
{
    public class Backup
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Shift { get; set; }
        public AttendanceStatus Status { get; set; }
        public DateOnly DateOnly { get; set; } // 1/8   2/8

    }
}
