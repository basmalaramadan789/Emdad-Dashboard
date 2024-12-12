using Emdad_Dashboard.Data;
using Emdad_Dashboard.Helper;
using Emdad_Dashboard.Models;
using Emdad_Dashboard.VeiwModel.KPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace Emdad_Dashboard.Controllers
{
    public class KPIController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly FileHelper _fileHelper;

        public KPIController(ApplicationContext context, FileHelper fileHelper)
        {
            _context = context;
            _fileHelper = fileHelper;
        }

        [HttpGet("GetKPISummary")]
        public IActionResult GetKPISummary()
        {
            // Sample data, replace this with your database query
            var kpis = _context.kPIs.Select(k => new KPIViewModel
            {
                EmployeeName = k.EmployeeName,
                Title = k.Title,
                Shift = k.Shift,
                TotalPoint = k.TotalPoint,
                Total = k.Total,
                Service = k.Service,
                ServicePoint = k.ServicePoint,
                Appearance = k.Appearance,
                AppearancePoint = k.AppearancePoint,
                General = k.General,
                GeneralPoint = k.GeneralPoint,
                Attendance = k.Attendance,
                AttendancePoint = k.AttendancePoint
            }).ToList();

            // Grouping data by EmployeeName or another category if required
            var summary = kpis.GroupBy(k => k.EmployeeName)
                .Select(g => new
                {
                    EmployeeName = g.Key,
                    TotalPoints = g.Sum(k => k.TotalPoint),
                    TotalScores = g.Sum(k => k.Total),
                    TotalServicePromesies = g.Sum(k => k.Service),
                    TotalAppearence = g.Sum(k => k.Appearance),
                    TotalGeneralAndBehavior = g.Sum(k => k.General),
                    TotalAttendane = g.Sum(k => k.Attendance),


                }).ToList();

            return Ok(summary);
        }



        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var KPIs = _context.kPIs.ToList();
            List<KPIViewModel> kpisView = new();

            foreach (var kpi in KPIs)
            {

                var viewModel = new KPIViewModel()
                {
                    Id = kpi.Id,
                    EmployeeName = kpi.EmployeeName,
                    WorkingDay = kpi.WorkingDay,
                    Title = kpi.Title,
                    ServicePoint = kpi.ServicePoint,
                    Service = kpi.Service,
                    AppearancePoint = kpi.AppearancePoint,
                    Appearance = kpi.Appearance,
                    Attendance = kpi.Attendance,
                    AttendancePoint = kpi.AttendancePoint,
                    Shift = kpi.Shift,
                    GeneralPoint = kpi.GeneralPoint,
                    General = kpi.General,
                    TotalPoint = kpi.TotalPoint,
                    Total = kpi.Total,
                    Rate = kpi.Rate.ToString()
                };
                kpisView.Add(viewModel);
            }

            ViewBag.KPIs = KPIs;

            return View(kpisView);
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile UploadedFile)
        {
            List<List<object?>> excelData = new();
            List<AddKPIDTO> kpis = new();
            string formattedDate;
            try
            {
                if (!ModelState.IsValid || UploadedFile == null || UploadedFile.Length == 0)
                {
                    ViewBag.Message = "No file selected.";
                    return View();
                }

                // Upload the file
                var filePath = await _fileHelper.UploadFileAsync(UploadedFile);

                // Read Excel data
                excelData = _fileHelper.ReadExcelFileForKPI(filePath);

                ViewBag.ExcelData = excelData;
                ViewBag.Message = "File uploaded and processed successfully.";

            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                return View();
            }

            // Map Excel data to AddAttendanceDTO
            foreach (var row in excelData)
            {
                if (row[0].ToString() != "" && !string.IsNullOrEmpty(row[4].ToString()) && row[4].ToString() != "") // Ensure the row has data
                {
                    var val = row[4].ToString();
                    // Parse the status value into the AttendanceStatus enum
                    // Default value

                    kpis.Add(
                        new AddKPIDTO(row[1].ToString(), row[2].ToString(), Convert.ToDecimal(row[3]),
                        Convert.ToDecimal(row[5]), Convert.ToDecimal(row[7]), Convert.ToDecimal(row[9]), row[14].ToString())
                        );

                    
                }
                else
                {

                    // Handle invalid shift values (e.g., log or set default value)
                    ViewBag.Message = $"Invalid shift value in row: {row[3]}";


                }

            }

            // Return the AddAttendance view with the Attendances list
            TempData["kpis"] = JsonSerializer.Serialize(kpis);
            return RedirectToAction(nameof(AddKPI));
        }
        public IActionResult KpiDashboard()
        {
            var kpis = _context.kPIs.Select(kpi => new KPIViewModel
            {
                Id = kpi.Id,
                EmployeeName = kpi.EmployeeName,
                WorkingDay = kpi.WorkingDay,
                Title = kpi.Title,
                ServicePoint = kpi.ServicePoint,
                Service = kpi.Service,
                AppearancePoint = kpi.AppearancePoint,
                Appearance = kpi.Appearance,
                AttendancePoint = kpi.AttendancePoint,
                Attendance = kpi.Attendance,
                GeneralPoint = kpi.GeneralPoint,
                General = kpi.General,
                TotalPoint = kpi.TotalPoint,
                Total = kpi.Total,
                Shift = kpi.Shift,
                Rate = kpi.Rate.ToString()
            }).ToList();

            return View(kpis);
        }

        [HttpGet("AddKPI")]
        public async Task<IActionResult> AddKPI()
        {
            List<KPI> kpis = new List<KPI>();
            var kpiDTO = JsonSerializer.Deserialize<List<AddKPIDTO>>(TempData["kpis"].ToString());
            foreach (var kpi in kpiDTO)
            {
                // Increment the WorkingDay by 1 before adding to the list
                if (_context.kPIs.Any(x => x.EmployeeName == kpi.employeeName))
                {
                    var existKpi = _context.kPIs.FirstOrDefault(x => x.EmployeeName == kpi.employeeName);
                    // scores
                    var serviceScore = Convert.ToDecimal(20);
                    var appearanceScore = Convert.ToDecimal(20);
                    var generalScore = Convert.ToDecimal(50);
                    var attendanceScore = Convert.ToDecimal(10);
                    // update working day
                    var workingDay = existKpi.WorkingDay + 1;
                    // Calc the last points
                    //var servicePointsLast = (existKpi.Service / serviceScore) * 5;
                    //var appearancePointsLast = (existKpi.Appearance / appearanceScore) * 5;
                    //var generalPointsLast = (existKpi.General / generalScore) * 5;

                    // calc the recent point
                    var servicePoint = ((existKpi.ServicePoint + kpi.service) / workingDay);
                    var appearancePoint = ((existKpi.AppearancePoint + kpi.appearance) / workingDay);
                    var generalPoint = ((existKpi.GeneralPoint + kpi.generalPerformance) / workingDay);
                    var attendancePoint = ((existKpi.AttendancePoint + kpi.attendance) / workingDay);

                    // calc the recent percentage
                    var servicePercentage = (servicePoint / 5) * serviceScore;
                    var appearancePercentage = (appearancePoint / 10) * appearanceScore;
                    var generalPercentage = (generalPoint / 19) * generalScore;
                    var attendancePercentage = (generalPoint / 1) * attendanceScore;


                    existKpi.Service = servicePercentage;
                    existKpi.Appearance = appearancePercentage;
                    existKpi.General = generalPercentage;

                    existKpi.ServicePoint = servicePoint;
                    existKpi.AppearancePoint = appearancePoint;
                    existKpi.GeneralPoint = generalPoint;

                    var totalPoint = ((existKpi.ServicePoint + existKpi.AppearancePoint + existKpi.GeneralPoint) + (kpi.service + kpi.appearance + kpi.generalPerformance)) / workingDay;
                    var total = (totalPoint / 35) * 100;
                    existKpi.Total = total;

                    existKpi.WorkingDay = workingDay;

                    existKpi.Rate = total > 95 ? Rate.Excellent :
                    total >= 85 ? Rate.VeryGood :
                    total >= 75 ? Rate.Good :
                    Rate.NeedImprovement;


                    _context.kPIs.Update(existKpi);
                    _context.SaveChanges();
                }
                else
                {
                    var servicePercentage = (kpi.service / 5) * Convert.ToDecimal(20);
                    var appearancePercentage = (kpi.appearance / 10) * Convert.ToDecimal(20);
                    var generalPercentage = (kpi.generalPerformance / 19) * Convert.ToDecimal(50);
                    var attendancePercentage = (kpi.attendance / 1) * Convert.ToDecimal(10);

                    var total = servicePercentage + appearancePercentage + generalPercentage + attendancePercentage;
                    var totalPoints = kpi.service + kpi.appearance + kpi.generalPerformance + kpi.attendance;


                    Rate rate;

                    if (total >= 90)
                    {
                        rate = Rate.Excellent; // Excellent for points 90 and above
                    }
                    else if (total >= 75)
                    {
                        rate = Rate.VeryGood; // VeryGood for points 75 to 89
                    }
                    else if (total >= 60)
                    {
                        rate = Rate.Good; // Good for points 60 to 74
                    }
                    else
                    {
                        rate = Rate.NeedImprovement; // Need Improvement for points below 60
                    }

                    _context.kPIs.Add(new KPI
                    {
                        EmployeeName = kpi.employeeName,
                        WorkingDay = 1,
                        Title = kpi.title,
                        Service = servicePercentage,
                        ServicePoint = kpi.service,
                        AppearancePoint = kpi.appearance,
                        Appearance = appearancePercentage,
                        General = generalPercentage,
                        GeneralPoint = kpi.generalPerformance,
                        Attendance = attendancePercentage,
                        AttendancePoint = kpi.attendance,
                        Total = total,
                        TotalPoint = totalPoints,
                        Shift = kpi.shift,
                        Rate = rate
                    });
                }
            }

            _context.SaveChanges();

            return RedirectToAction("Index");

        }
    }
    



    public record AddKPIDTO(string employeeName, string title, decimal service, decimal appearance, decimal generalPerformance, decimal attendance, string shift);
}
