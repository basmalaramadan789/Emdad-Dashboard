using Emdad_Dashboard.Controllers;
using Emdad_Dashboard.Models;
using Emdad_Dashboard.VeiwModel.SiteIssues;
using ExcelDataReader;

namespace Emdad_Dashboard.Helper
{
    public class FileHelper
    {
        private readonly string _uploadDirectory;

        public FileHelper()
        {
            _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("Invalid file");

            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }

            var sanitizedFileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(_uploadDirectory, sanitizedFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }

        public List<List<object>> ReadExcelFileForAttendance(string filePath, out string formattedDate)
        {
            var excelData = new List<List<object>>();
            formattedDate = null;

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var sheets = result.Tables;

                    if (sheets.Count > 0)
                    {
                        var firstSheet = sheets[0];
                        for (int row = 0; row < firstSheet.Rows.Count; row++)
                        {
                            var rowData = new List<object>();
                            for (int col = 0; col < firstSheet.Columns.Count; col++)
                            {
                                rowData.Add(firstSheet.Rows[row][col]);
                            }
                            excelData.Add(rowData);
                        }

                        if (excelData.Count > 1 && DateTime.TryParse((excelData[1])[4]?.ToString(), out DateTime date))
                        {
                            formattedDate = date.ToString("M/d");
                        }
                    }
                }
            }

            return excelData;
        }

        public async Task<List<AddAttendanceDTO>> ExtractOverTime(List<AddAttendanceDTO> attendances, string filePath)
        {

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var sheets = result.Tables;

                    if (sheets.Count > 1) // Ensure the second sheet exists
                    {
                        var secondSheet = sheets[1];

                        // Loop through the rows of the second sheet
                        for (int i = 1; i < secondSheet.Rows.Count; i++) // Start at 1 to skip the header row
                        {
                            var row = secondSheet.Rows[i];
                            if (row[0] != DBNull.Value) // Check for non-empty values
                            {
                                var name = row[0].ToString(); // Get the name from the first column
                                var attendance = attendances.FirstOrDefault(x => x.Name == name);

                                if (attendance != null) // Check if an attendance object exists for the name
                                {
                                    attendance.Status = AttendanceStatus.OV; // Assign the status
                                }
                            }
                        }
                    }
                }
            }

            return attendances;
        }

        public async Task<List<AddBackupDTO>> ExtractBackup(string filePath, DateOnly date)
        {
            List<AddBackupDTO> backups = new List<AddBackupDTO>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var sheets = result.Tables;

                    if (sheets.Count > 2) // Ensure the third sheet exists
                    {
                        var secondSheet = sheets[2]; // Access the third sheet (0-based index)

                        // Loop through the rows of the second sheet
                        for (int i = 1; i < secondSheet.Rows.Count; i++) // Start at 1 to skip the header row
                        {
                            var row = secondSheet.Rows[i];

                            if (row[0] != null)
                            {
                                string name = row[0]?.ToString()?.Trim(); // Safe access and trimming
                                string postition = row[1]?.ToString()?.Trim();
                                string shift = row[2]?.ToString()?.Trim();

                                if (!string.IsNullOrEmpty(name) &&
                                    !string.IsNullOrEmpty(postition) &&
                                  !string.IsNullOrEmpty(shift))
                                {
                                    backups.Add(new AddBackupDTO
                                    (
                                         name,
                                        postition,
                                        shift,
                                        date
                                    ));
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("The expected sheet does not exist in the Excel file.");
                    }
                }
            }

            return backups;
        }






        public List<List<object>> ReadExcelFileForKPI(string filePath)
        {
            var excelData = new List<List<object>>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var sheets = result.Tables;

                    if (sheets.Count > 0)
                    {
                        var firstSheet = sheets[0];
                        for (int row = 5; row < firstSheet.Rows.Count; row++)
                        {
                            if (firstSheet.Rows[row][5] != null)
                            {
                                var rowData = new List<object>();
                                for (int col = 0; col < firstSheet.Columns.Count; col++)
                                {
                                    rowData.Add(firstSheet.Rows[row][col]);
                                }
                                excelData.Add(rowData);

                            }
                        }

                    }
                }
            }

            return excelData;
        }

        public List<List<SiteIssue>> ReadExcelFileForSiteIssue(string filePath)
        {
            var excelData = new List<List<SiteIssue>>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var sheets = result.Tables;

                    if (sheets.Count > 0)
                    {
                        var firstSheet = sheets[0];
                        for (int row = 8; row < firstSheet.Rows.Count; row++)
                        {
                            if (firstSheet.Rows[row][8] != null)
                            {
                                var siteIssue = new SiteIssue
                                {
                                    Id = Guid.NewGuid(), // You might want to adjust this based on your data structure or requirements
                                    Type = firstSheet.Rows[row][1]?.ToString(),
                                    Time = TimeSpan.TryParse(firstSheet.Rows[row][2]?.ToString(), out TimeSpan time) ? time : TimeSpan.Zero,
                                    //Date = DateTime.TryParse(firstSheet.Rows[row][2]?.ToString(), out DateTime date) ? date : DateTime.MinValue,
                                    Date = DateOnly.TryParse(firstSheet.Rows[row][3]?.ToString(), out DateOnly date) ? date : default(DateOnly),
                                    Owner = firstSheet.Rows[row][4]?.ToString(),
                                    Description = firstSheet.Rows[row][5]?.ToString(),
                                    Resolution = firstSheet.Rows[row][6]?.ToString(),
                                    Status = firstSheet.Rows[row][7]?.ToString(),
                                    EmployeesName = firstSheet.Rows[row][8]?.ToString()
                                };

                                var rowData = new List<SiteIssue> { siteIssue };
                                excelData.Add(rowData);
                            }
                        }
                    }
                }
            }

            return excelData;
        }


        public List<List<Vip>> ReadExcelFileForVip(string filePath)
        {

            var excelData = new List<List<Vip>>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var sheets = result.Tables;

                    if (sheets.Count > 0)
                    {
                        var firstSheet = sheets[3];
                        for (int row = 2; row < firstSheet.Rows.Count; row++) // Assuming headers are in the first row
                        {
                            if (firstSheet.Rows[row][11] != null)
                            {
                                var vip = new Vip
                                {
                                    Id = Guid.NewGuid(), // You might want to adjust this based on your data structure or requirements
                                    Name = firstSheet.Rows[row][1]?.ToString(),
                                    Visitors = int.TryParse(firstSheet.Rows[row][2]?.ToString(), out int visitors) ? visitors : (int?)null,
                                    VisitDate = DateTime.TryParse(firstSheet.Rows[row][3]?.ToString(), out DateTime visitDate) ? visitDate : (DateTime?)null,
                                    VisitPerDay = int.TryParse(firstSheet.Rows[row][4]?.ToString(), out int visitPerDay) ? visitPerDay : 0,
                                    ArrivalTime = firstSheet.Rows[row][5]?.ToString(),
                                    ContactDetails = firstSheet.Rows[row][6]?.ToString(),
                                    ReservationDetails = firstSheet.Rows[row][7]?.ToString(),
                                    VisitType = firstSheet.Rows[row][8]?.ToString(),
                                    Gate = firstSheet.Rows[row][9]?.ToString(),
                                    VisitDetails = firstSheet.Rows[row][10]?.ToString(),
                                    Feedback = firstSheet.Rows[row][11]?.ToString(),
                                    Remarks = firstSheet.Rows[row][12]?.ToString()
                                };

                                var rowData = new List<Vip> { vip };
                                excelData.Add(rowData);
                            }
                        }
                    }
                }
            }

            return excelData;
        }
    }
}