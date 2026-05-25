using System.ComponentModel.DataAnnotations;

public class Gracz
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Imie { get; set; }

    [Required]
    [MaxLength(50)]
    public string Nazwisko { get; set; }

    public DateTime DataUrodzenia { get; set; }

    [MaxLength(50)]
    public string Kraj { get; set; }

    public Reka Reka { get; set; }          
    public StylGry StylGry { get; set; } 

    public ICollection<Mecz> MeczeJakoGracz1 { get; set; } = new List<Mecz>();
    public ICollection<Mecz> MeczeJakoGracz2 { get; set; } = new List<Mecz>();
    public ICollection<Mecz> WygraneMecze { get; set; } = new List<Mecz>();
    public Uzytkownik? Uzytkownik { get; set; }
}

public enum Reka { Prawa, Lewa }
public enum StylGry { Allcourt, Baseliner, ServeAndVolley }