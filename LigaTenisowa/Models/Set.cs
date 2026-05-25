using System.ComponentModel.DataAnnotations;

public class Set
{
    public int Id { get; set; }

    [Required]
    public int MeczId { get; set; }
    public Mecz Mecz { get; set; }

    public int NumerSeta { get; set; }
    public int GemyGracz1 { get; set; }
    public int GemyGracz2 { get; set; }
    public int? TiebreakGracz1 { get; set; } // null = nie było tiebreaka
    public int? TiebreakGracz2 { get; set; }
}