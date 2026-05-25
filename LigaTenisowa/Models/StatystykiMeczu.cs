using System.ComponentModel.DataAnnotations;

public class StatystykiMeczu
{
    public int Id { get; set; }

    [Required]
    public int MeczId { get; set; }
    public Mecz Mecz { get; set; }

    // Serwis
    public int AsyGracz1 { get; set; }
    public int AsyGracz2 { get; set; }
    public int DoubleFaultsGracz1 { get; set; }
    public int DoubleFaultsGracz2 { get; set; }
    public double PierwszySerwisProcentGracz1 { get; set; }
    public double PierwszySerwisProcentGracz2 { get; set; }

    // Punkty na serwisie
    public int PktNa1SerGracz1 { get; set; }
    public int PktNa1SerGracz2 { get; set; }
    public int PktNa2SerGracz1 { get; set; }
    public int PktNa2SerGracz2 { get; set; }

    // Uderzenia
    public int WinnersGracz1 { get; set; }
    public int WinnersGracz2 { get; set; }
    public int UnforcedErrorsGracz1 { get; set; }
    public int UnforcedErrorsGracz2 { get; set; }

    // Break pointy
    public int BreakPktWykorzystaneGracz1 { get; set; }
    public int BreakPktWykorzystaneGracz2 { get; set; }
    public int BreakPktOkazjeGracz1 { get; set; }
    public int BreakPktOkazjeGracz2 { get; set; }

    // Ogólne
    public int CzasMeczuMin { get; set; }
    public int? Publicznosc { get; set; }
}