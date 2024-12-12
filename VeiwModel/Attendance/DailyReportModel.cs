public class DailyReportModel
{
    public DateTime SelectedDate { get; set; }
    public string ReportType { get; set; } // "daily", "weekly", or "monthly"
    public int Year { get; set; }
    public int Month { get; set; }
    public int Week { get; set; }
    public string PdfFilePath { get; set; }
}
