public class RankingViewModel
{
    public Gracz Gracz { get; set; }
    public int Wygrane { get; set; }
    public int Przegrane { get; set; }
    public int Punkty { get; set; }
    public double WinRatio => (Wygrane + Przegrane) == 0 ? 0 :
        Math.Round((double)Wygrane / (Wygrane + Przegrane) * 100, 1);
    public string Forma { get; set; }
}