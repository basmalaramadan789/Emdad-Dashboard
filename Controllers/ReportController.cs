using Emdad_Dashboard.VeiwModel.Attendance;
using Microsoft.AspNetCore.Mvc;

namespace Emdad_Dashboard.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report/ReportPage
        public IActionResult DailyReport()
        {
            var model = new DailyReportModel
            {
                SelectedDate = new DateTime(2024, 12, 05),  // Set default date to 2024-12-02
                ReportType = "daily", // Default report type
                PdfFilePath = $"/Uploads/attendance_daily_{new DateTime(2024, 12, 05):yyyy-MM-dd}.pdf" // Default PDF
            };

            return View(model);
        }

        // POST: Report/ReportPage
        [HttpPost]
        public IActionResult DailyReport(DailyReportModel model)
        {
            // Based on the selected report type, construct the appropriate PDF file path
            switch (model.ReportType)
            {
                case "weekly":
                    model.PdfFilePath = $"/Uploads/week_{model.Year}_{model.Month:00}_w{model.Week}.pdf";
                    break;
                case "monthly":
                    model.PdfFilePath = $"/Uploads/monthly_{model.Year}_{model.Month:00}.pdf";
                    break;
                case "daily":
                default:
                    model.PdfFilePath = $"/Uploads/attendance_daily_{model.SelectedDate:yyyy-MM-dd}.pdf";
                    break;
            }

            return View(model);
        }

        public IActionResult GeneralDailyReport()
        {
            var model = new GeneralDailyReportModel
            {
                SelectedDate = new DateTime(2024, 12, 05), // Default to today's date
                SelectedShift = "B", // Default shift
                PdfFilePath = $"/Uploads/shiftBgeneral_daily_{DateTime.Now:yyyy-MM-dd}.pdf" // Default file path
            };

            return View(model);
        }

        // POST: Report/GeneralDailyReport
        [HttpPost]
        public IActionResult GeneralDailyReport(GeneralDailyReportModel model)
        {
            if (string.IsNullOrEmpty(model.SelectedShift) || (model.SelectedShift != "A" && model.SelectedShift != "B"))
            {
                model.SelectedShift = "A"; // Default to Shift A if not provided or invalid
            }

            // Construct the PDF file path based on the selected shift and date
            model.PdfFilePath = $"/Uploads/shift{model.SelectedShift}general_daily_{model.SelectedDate:yyyy-MM-dd}.pdf";

            return View(model);
        }

    }
}
