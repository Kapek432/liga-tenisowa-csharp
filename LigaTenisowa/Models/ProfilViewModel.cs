public class ProfilViewModel
{
    public Gracz Gracz { get; set; }
    public int Wygrane { get; set; }
    public int Przegrane { get; set; }
    public double WinRatio => (Wygrane + Przegrane) == 0 ? 0 :
        Math.Round((double)Wygrane / (Wygrane + Przegrane) * 100, 1);
    public int TotalAsy { get; set; }
    public int TotalDoubleFaults { get; set; }
    public int TotalWinners { get; set; }
    public int TotalUnforcedErrors { get; set; }
    public double SredniSerwisProcent { get; set; }
    public List<Mecz> OstatnieMecze { get; set; }
    public string Forma { get; set; } 
}