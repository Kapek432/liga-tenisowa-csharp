using System.ComponentModel.DataAnnotations;

public class Mecz
{
    public int Id { get; set; }

    [Required]
    public int SezonId { get; set; }
    public Sezon Sezon { get; set; }

    [Required]
    public int Gracz1Id { get; set; }
    public Gracz Gracz1 { get; set; }

    [Required]
    public int Gracz2Id { get; set; }
    public Gracz Gracz2 { get; set; }

    public int? ZwyciezcaId { get; set; }  // null = nierozegrany
    public Gracz? Zwyciezca { get; set; }

    public DateTime DataMeczu { get; set; }
    public Nawierzchnia Nawierzchnia { get; set; }  
    public Format Format { get; set; } 

    public ICollection<Set> Sety { get; set; } = new List<Set>();
    public StatystykiMeczu? Statystyki { get; set; }
}

public enum Nawierzchnia { Hard, Clay, Grass }
public enum Format { Bo3, Bo5 }