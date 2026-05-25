public class RekordyViewModel
{
    public Gracz NajwiecejAsow { get; set; }
    public int MaxAsy { get; set; }

    public Gracz NajwiecejWinners { get; set; }
    public int MaxWinners { get; set; }

    public Gracz NajmniejBledow { get; set; }
    public int MinBledow { get; set; }

    public Gracz NajlepszySerwis { get; set; }
    public double MaxSerwisProc { get; set; }

    public Mecz NajdluzszyMecz { get; set; }
    public int MaxCzasMeczu { get; set; }

    public Gracz LiderAsowKariery { get; set; }
    public int SumAsowKariery { get; set; }

    public Gracz LiderWinnersKariery { get; set; }
    public int SumWinnersKariery { get; set; }

    public Gracz NajmniejBledowKariery { get; set; }
    public int SumBledowKariery { get; set; }

    public Gracz NajlepszySerwisKariery { get; set; }
    public double SredniSerwisKariery { get; set; }

    public Gracz NajwiecejCzasuKariery { get; set; }
    public int SumCzasuKariery { get; set; }
}
