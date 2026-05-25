using System.ComponentModel.DataAnnotations;

public class Sezon
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nazwa { get; set; }

    public DateTime DataRozpoczecia { get; set; }
    public DateTime DataZakonczenia { get; set; }
    public bool CzyAktywny { get; set; }

    public ICollection<Mecz> Mecze { get; set; } = new List<Mecz>();
}