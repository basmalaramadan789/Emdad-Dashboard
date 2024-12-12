using Emdad_Dashboard.Models;

namespace Emdad_Dashboard.VeiwModel.KPI
{
    public class KPIViewModel
    {
        public Guid Id { get; set; }
        public string EmployeeName { get; set; }
        public int WorkingDay { get; set; }
        public string Title { get; set; }

        public decimal ServicePoint { get; set; }
        public decimal Service { get; set; }
        public decimal AppearancePoint { get; set; }
        public decimal Appearance { get; set; }
        public decimal AttendancePoint { get; set; }
        public decimal Attendance { get; set; }
        public decimal GeneralPoint { get; set; }
        public decimal General { get; set; }
        public decimal TotalPoint { get; set; }
        public decimal Total { get; set; }
        public string Shift { get; set; }


        public string Rate { get; set; }
    }


}
