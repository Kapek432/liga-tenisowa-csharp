using System.ComponentModel.DataAnnotations;

public class Uzytkownik
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Login { get; set; }

    [Required]
    public string HasloHash { get; set; }

    [Required]
    public string ApiToken { get; set; }

    public bool CzyAdmin { get; set; }

    public int? GraczId { get; set; } // null jeśli admin
    public Gracz? Gracz { get; set; }
}