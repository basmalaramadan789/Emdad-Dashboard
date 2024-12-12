using Emdad_Dashboard.Data;
using Emdad_Dashboard.Helper; 
using Emdad_Dashboard.Models;
using Emdad_Dashboard.VeiwModel.Attendance;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Emdad_Dashboard.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly FileHelper _fileHelper;
        public AttendanceController(ApplicationContext context, FileHelper fileHelper)
        {
            _context = context;
            _fileHelper = fileHelper;
        }

        [HttpPost]
        public async Task<IActionResult> UploadGeneralDailyReport(IFormFile UploadedFile, string ReportDate, string Shift)
        {
            if (UploadedFile == null || UploadedFile.Length == 0)
                return BadRequest(new { message = "Please select a valid file." });

            if (string.IsNullOrEmpty(ReportDate) || !DateTime.TryParse(ReportDate, out var parsedDate))
                return BadRequest(new { message = "Please select a valid date." });

            if (string.IsNullOrEmpty(Shift) || (Shift != "A" && Shift != "B"))
                return BadRequest(new { message = "Please select a valid shift." });

            try
            {
                // Generate the file name in the required format
                var fileName = $"shift{Shift}general_daily_{parsedDate:yyyy-MM-dd}.pdf";

                // Define the upload directory
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadedFile.CopyToAsync(stream);
                }

                return Ok(new { message = "File uploaded successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error uploading file: {ex.Message}" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var attendances = _context.attendances.OrderBy(x => x.DateOnly).ToList();
            ViewBag.dates = attendances.Select(x => x.DateOnly).Distinct().ToList();
            ViewBag.datesCount = attendances.GroupBy(x => x.DateOnly).Count();
            ViewBag.Attendances = attendances;
            ViewBag.EmployeeCount = attendances.GroupBy(x => x.Name).Count();
            ViewBag.PositionCount = attendances.GroupBy(x => x.Position).Count();

            ViewBag.TotalPresent = attendances.Where(x => x.Status == AttendanceStatus.P).Count();
            ViewBag.TotalAbsent = attendances.Where(x => x.Status == AttendanceStatus.U).Count();


            ViewBag.Backup = await _context.Backups.ToListAsync();

            return View(new AttendanceViewModel());
        }

        //[HttpGet("Search")]
        //[Route("Attendance/Search")]
        //public IActionResult Index(string employeeName)
        //{
        //    var attendances = _context.attendances.ToList();
        //    ViewBag.dates = attendances.Select(x => x.DateOnly).Distinct().ToList();
        //    ViewBag.Attendances = attendances;


        //    return View(new AttendanceViewModel());
        //}

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile UploadedFile)
        {
            List<List<object?>> excelData = new();
            List<AddAttendanceDTO> Attendances = new();
            List<AddBackupDTO> backups = new List<AddBackupDTO>();
            string formattedDate;
            var filePath = await _fileHelper.UploadFileAsync(UploadedFile);
            try
            {
                if (!ModelState.IsValid || UploadedFile == null || UploadedFile.Length == 0)
                {
                    ViewBag.Message = "No file selected.";
                    return View();
                }

                // Upload the file

                // Read Excel data
                excelData = _fileHelper.ReadExcelFileForAttendance(filePath, out formattedDate);

                ViewBag.ExcelData = excelData;
                ViewBag.Day = formattedDate;
                ViewBag.Message = "File uploaded and processed successfully.";

                if (formattedDate == null)
                {
                    ViewBag.Message = $"Error: there is no date";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                return View();
            }

            // Map Excel data to AddAttendanceDTO
            foreach (var row in excelData)
            {
                if (row[0] != null) // Ensure the row has data
                {
                    // Parse the shift value into the Shift enum
                    if (Enum.TryParse(row[3]?.ToString(), true, out Shift shiftEnum))
                    {
                        // Parse the status value into the AttendanceStatus enum
                        AttendanceStatus statusEnum = AttendanceStatus.Z; // Default value
                        if (Enum.TryParse(row[4]?.ToString(), true, out AttendanceStatus parsedStatus))
                        {
                            statusEnum = parsedStatus;
                        }

                        var dto = new AddAttendanceDTO
                        (
                             row[1]?.ToString(),
                             row[2]?.ToString(),
                             shiftEnum,
                            statusEnum,
                             DateOnly.ParseExact(formattedDate, "M/d", null)
                        );

                        Attendances.Add(dto);
                    }
                    else
                    {
                        // Handle invalid shift values (e.g., log or set default value)
                        ViewBag.Message = $"Invalid shift value in row: {row[3]}";
                    }
                }
            }

            Attendances = await _fileHelper.ExtractOverTime(Attendances, filePath);
            backups = await _fileHelper.ExtractBackup(filePath, DateOnly.ParseExact(formattedDate, "M/d", null));
            // Return the AddAttendance view with the Attendances list
            TempData["Attendances"] = JsonSerializer.Serialize(Attendances);
            TempData["Backup"] = JsonSerializer.Serialize(backups);
            return RedirectToAction(nameof(AddAttendance));

        }
        //[HttpPost("/Search")]
        //public async Task<IActionResult> Index([FromBody] SearchNameDTO request)
        //{
        //    IQueryable<Attendance> attendances = _context.attendances;

        //    // Apply filter if employeeName is provided
        //    if (!string.IsNullOrEmpty(request.employeeName))
        //    {
        //        attendances = attendances.Where(x => x.Name.Contains(request.employeeName));
        //    }

        //    // Populate ViewBag with filtered results
        //    ViewBag.dates = await attendances.Select(x => x.DateOnly).Distinct().ToListAsync();
        //    ViewBag.Attendances = await attendances.ToListAsync();

        //    return View(new AttendanceViewModel());
        //}




        [HttpGet("GetAttendanceStatistics")]
        public async Task<IActionResult> GetAttendanceStatistics()
        {
            var query = _context.attendances
                      .OrderByDescending(x => x.DateOnly)
                      .GroupBy(x => x.DateOnly)
                      .Take(5)
                      .Select(g => g.Key);

            Console.WriteLine(query.ToQueryString());
            // Fetch the top 5 most recent distinct dates
                var topDates = await _context.attendances
                .OrderByDescending(x => x.DateOnly)  // Sort by DateOnly in descending order
                .GroupBy(x => x.DateOnly)  // Group by DateOnly
                .Take(5)  // Take the top 5 most recent dates
                .Select(g => g.Key)  // Select the distinct DateOnly
                .ToListAsync();

            // Fetch the attendance records for those dates
            var attendancesForTopDates = await _context.attendances   // Filter records based on the top 5 dates
                .ToListAsync();

            // Check if no records found
            if (!attendancesForTopDates.Any())
                return NotFound("No attendance records found for the last 7 days.");

            // Calculate attendance and absence for each day
            var attendanceStatistics = topDates.Select(date => new
            {
                y = date.ToString("MM-dd"), // Format date as string for x-axis
                a = attendancesForTopDates.Count(a => a.DateOnly == date && (a.Status == AttendanceStatus.P || a.Status == AttendanceStatus.OV)), // Present
                b = attendancesForTopDates.Count(a => a.DateOnly == date && (a.Status == AttendanceStatus.U || a.Status == AttendanceStatus.E)),  // Absent
                c = attendancesForTopDates.Count(a => a.DateOnly == date && (a.Status == AttendanceStatus.O))  // Absent

            }).ToList();

            return Ok(attendanceStatistics);
        }

        [HttpPost("UploadPdf")]
        public async Task<IActionResult> UploadPdf(IFormFile file, string date)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file was uploaded.");

            // Ensure date is passed and properly formatted
            if (string.IsNullOrEmpty(date) || !DateTime.TryParse(date, out var parsedDate))
            {
                return BadRequest("Invalid or missing date.");
            }

            // Generate a new file name with the current date and time
            var timestamp = parsedDate.ToString("yyyy-MM-dd");
            var newFileName = $"attendance_daily_{timestamp}{Path.GetExtension(file.FileName)}";

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, newFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { FilePath = $"/uploads/{newFileName}" });
        }




        [HttpGet("GetPdf")]
        public IActionResult GetPdf(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name must be provided.");

            // Construct the file path
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            // Read the file and return as a FileResult
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }





        [HttpGet("AddAttendance")]
        public async Task<IActionResult> AddAttendance()
        {
            List<Attendance> attendances = new List<Attendance>();
            List<Backup> backups = new List<Backup>();
            var jsonString = TempData["Attendances"] as string;
            var backupsDTOJson = TempData["Backup"] as string;
            var attendancesDTO = JsonSerializer.Deserialize<List<AddAttendanceDTO>>(jsonString);
            var backupsDes = JsonSerializer.Deserialize<List<AddBackupDTO>>(backupsDTOJson);

            foreach (var attendance in attendancesDTO)
            {

                attendances.Add(new Attendance { Name = attendance.Name, Position = attendance.Position, Shift = attendance.Shift, Status = attendance.Status, DateOnly = attendance.Date });
            }

            foreach (var back in backupsDes)
            {
                backups.Add(new Backup { Name = back.Name, Position = back.Position, Shift = back.Shift, DateOnly = back.Date });
            }
            _context.attendances.AddRange(attendances);
            _context.Backups.AddRange(backups);
            _context.SaveChanges();

            return RedirectToAction("Index");

        }

        [HttpPost("UpdateDate")]
        public async Task<IActionResult> UpdateDate([FromBody] UpdateDate request)
        {
            try
            {
                var attendances = await _context.attendances
                    .Where(x => x.DateOnly == DateOnly.ParseExact(request.oldDate, "yyyy-MM-dd"))
                    .ToListAsync();

                foreach (var attendance in attendances)
                {
                    attendance.DateOnly = DateOnly.ParseExact(request.newDate, "yyyy-MM-dd");
                }

                _context.UpdateRange(attendances);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Date updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("GetAttendancesStatics")]
        public async Task<IActionResult> GetAttendancesStatics(GetAttendancsStaticsDTO request)
        {
            try
            {

                if (DateOnly.TryParse(request.date, out DateOnly realDate))
                {
                    // Fetch attendance records for the specified date
                    var attendances = await _context.attendances
                        .Where(x => x.DateOnly == realDate)
                        .ToListAsync();

                    // Total number of employees
                    int totalCount = attendances.Count;

                    // Count for each status type
                    int presentCount = attendances.Count(x => x.Status.ToString().ToUpper() == "P");
                    int offCount = attendances.Count(x => x.Status.ToString().ToUpper() == "O");
                    int absentCount = attendances.Count(x => x.Status.ToString().ToUpper() == "U");
                    int leaveCount = attendances.Count(x => x.Status.ToString().ToUpper() == "V");
                    int absentWithExcuseCount = attendances.Count(x => x.Status.ToString().ToUpper() == "E");
                    // Calculate percentages
                    double attendancePercentage = totalCount > 0
                        ? (presentCount / (double)totalCount) * 100
                        : 0;

                    double offCountPercentage = totalCount > 0
                        ? (offCount / (double)totalCount) * 100
                        : 0;

                    double absentPercentage = totalCount > 0
                        ? (absentCount / (double)totalCount) * 100
                        : 0;

                    double leavePercentage = totalCount > 0
                        ? (leaveCount / (double)totalCount) * 100
                        : 0;

                    double absentWithExcusePercentage = totalCount > 0
                        ? (absentWithExcuseCount / (double)totalCount) * 100
                        : 0;

                    // Prepare the response
                    var response = new
                    {
                        Date = realDate,
                        TotalEmployees = totalCount,
                        PresentEmployees = presentCount,
                        OffCount = offCount,
                        AbsentCount = absentCount,
                        AbsentWithExcuseCount = absentWithExcuseCount,
                        LeaveCount = leaveCount,
                        AbsentWithExcuse = absentWithExcuseCount,
                        AttendancePercentage = Math.Round(attendancePercentage, 2), // Rounded to 2 decimal places
                        OffPercentage = Math.Round(offCountPercentage, 2),
                        AbsentPercentage = Math.Round(absentPercentage, 2),
                        LeavePercentage = Math.Round(leavePercentage, 2),
                        AbsentWithExcusePercentage = Math.Round(absentWithExcusePercentage, 2)
                    };

                    return Json(response); // Return the data as JSON
                }


                return BadRequest("Invalid date format. Please use MM/dd/yyyy.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        public async Task<IActionResult> WeeklyReport()
        {
            var fromDate = _context.attendances
                           .Select(a => a.DateOnly)
                           .DefaultIfEmpty() // Ensures no exception if table is empty
                           .Max();

            var toDate = _context.attendances.Select(a => a.DateOnly).DefaultIfEmpty().Min();


            // Get data for the past 7 days
            var startDate = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-6)); // Last 7 days
            var endDate = DateOnly.FromDateTime(DateTime.Now.Date);

            // Fetch attendance data grouped by employee for the week
            var weeklyAttendance = _context.attendances
               .Where(a => a.DateOnly >= toDate && a.DateOnly <= fromDate)
               .GroupBy(a => a.Name)
               .Select(group => new WeeklyAttendanceReportViewModel
               {
                   EmployeeName = group.Key,
                   TotalDays = group.Count(),
                   PresentDays = group.Count(x => x.Status == AttendanceStatus.P),  // Avoid ToString
                   AbsentDays = group.Count(x => x.Status == AttendanceStatus.U)   // Avoid ToString
               }).ToList();

            return View(weeklyAttendance);
        }

        [HttpDelete("{date}/DeleteDay")]
        public async Task<IActionResult> DeleteDay(string date)
        {
            try
            {
                // Parse the date with the expected format
                DateTime parsedDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                // Format the parsed DateTime to the desired format
                string formattedDate = parsedDate.ToString("M/d/yyyy");


                // Find records matching the date
                var attendances = await _context.attendances
                    .Where(x => x.DateOnly == DateOnly.Parse(date))
                    .ToListAsync();

                if (attendances.Count == 0)
                {
                    return NotFound(new { message = "No records found for the specified date." });
                }

                // Remove the matching records
                _context.attendances.RemoveRange(attendances);
                await _context.SaveChangesAsync();

                // Return success response
                return Ok(new { message = "Records deleted successfully.", deletedCount = attendances.Count });
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "Invalid date format. Please use MM/dd/yyyy." });
            }
            catch (Exception ex)
            {
                // Log exception (optional)
                return StatusCode(500, new { message = "An error occurred while deleting records.", error = ex.Message });
            }
        }



    }


    public class AddBackupDTO
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string Shift { get; set; }


        public DateOnly Date { get; set; }

        public AddBackupDTO(string name, string position, string shift, DateOnly date)
        {
            Name = name;
            Position = position;
            Shift = shift;

            Date = date;
        }
    }
    public class AddAttendanceDTO
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public Shift Shift { get; set; }
        public bool IsBackup { get; set; }
        public AttendanceStatus Status { get; set; }
        public DateOnly Date { get; set; }

        // Constructor
        public AddAttendanceDTO(string name, string position, Shift shift, AttendanceStatus status, DateOnly date, bool isBackup = false)
        {
            Name = name;
            Position = position;
            Shift = shift;
            Status = status;
            Date = date;
            IsBackup = isBackup;
        }
    }

    public record UpdateDate(Guid id, string oldDate, string newDate);
    public record GetAttendancsStaticsDTO(string date);
    public record SearchNameDTO(string employeeName);
}
