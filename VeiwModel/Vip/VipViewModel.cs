namespace Emdad_Dashboard.VeiwModel.Vip;

public class VipViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int? Visitors { get; set; }
    public DateTime? VisitDate { get; set; }
    public int VisitPerDay { get; set; }
    public string ArrivalTime { get; set; }
    public string ContactDetails { get; set; }
    public string ReservationDetails { get; set; }
    public string VisitType { get; set; }
    public string Gate { get; set; }
    public string VisitDetails { get; set; }
    public string Feedback { get; set; }
    public string Remarks { get; set; }
}
