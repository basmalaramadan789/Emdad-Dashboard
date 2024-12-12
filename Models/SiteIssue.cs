namespace Emdad_Dashboard.Models;


    public class SiteIssue
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public TimeSpan Time { get; set; }
        public DateOnly Date { get; set; }
        public string Owner { get; set; }
        public string Description { get; set; }
        public string Resolution { get; set; }
        public string Status { get; set; }
        public string EmployeesName { get; set; }
    }

