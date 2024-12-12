using System.ComponentModel.DataAnnotations;

namespace Emdad_Dashboard.Models
{
    public class KPI
    {
        public Guid Id { get; set; }
        public string EmployeeName { get; set; }
        public int WorkingDay { get; set; }
        public string Title { get; set; }

        public decimal Service {  get; set; }
        public decimal ServicePoint { get; set; }
        public decimal Appearance { get; set; }
        public decimal AppearancePoint { get; set; }
        public decimal General { get; set; }
        public decimal GeneralPoint  { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPoint { get; set; }
        public decimal Attendance { get; set; }
        public decimal AttendancePoint { get; set; }
        public string Shift { get; set; }

        public Rate Rate { get; set; }
    }

    public enum Rate
    {
        Excellent,
        VeryGood,
        Good,
        [Display(Name = "Need Improvement")]
        NeedImprovement
    }

}
