public class GeneralDailyReportModel
{
    public DateTime SelectedDate { get; set; }
    public string SelectedShift { get; set; } // "A" or "B"
    public string PdfFilePath { get; set; }
}
