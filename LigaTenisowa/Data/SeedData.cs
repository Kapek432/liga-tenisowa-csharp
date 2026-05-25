public class SeedFile
{
    public SeedAdmin Admin { get; set; }
    public List<SeedUzytkownik> Uzytkownicy { get; set; } = new();
    public List<SeedGracz> Gracze { get; set; } = new();
    public List<SeedSezon> Sezony { get; set; } = new();
    public List<SeedMecz> Mecze { get; set; } = new();
}

public class SeedAdmin
{
    public string Login { get; set; }
    public string Haslo { get; set; }
    public string ApiToken { get; set; }
}

public class SeedUzytkownik
{
    public string Login { get; set; }
    public string Haslo { get; set; }
    public string ApiToken { get; set; }
    public int GraczIndex { get; set; }
}

public class SeedGracz
{
    public string Imie { get; set; }
    public string Nazwisko { get; set; }
    public string Kraj { get; set; }
    public string Reka { get; set; }
    public string StylGry { get; set; }
    public string DataUrodzenia { get; set; }
}

public class SeedSezon
{
    public string Nazwa { get; set; }
    public string DataRozpoczecia { get; set; }
    public string DataZakonczenia { get; set; }
    public bool CzyAktywny { get; set; }
}

public class SeedMecz
{
    public int SezonIndex { get; set; }
    public int Gracz1Index { get; set; }
    public int Gracz2Index { get; set; }
    public int? ZwyciezcaIndex { get; set; }
    public string DataMeczu { get; set; }
    public string Nawierzchnia { get; set; }
    public string Format { get; set; }
    public List<SeedSet> Sety { get; set; } = new();
    public SeedStatystyki Statystyki { get; set; }
}

public class SeedSet
{
    public int NumerSeta { get; set; }
    public int GemyGracz1 { get; set; }
    public int GemyGracz2 { get; set; }
    public int? TiebreakGracz1 { get; set; }
    public int? TiebreakGracz2 { get; set; }
}

public class SeedStatystyki
{
    public int AsyGracz1 { get; set; }
    public int AsyGracz2 { get; set; }
    public int DoubleFaultsGracz1 { get; set; }
    public int DoubleFaultsGracz2 { get; set; }
    public double PierwszySerwisProcentGracz1 { get; set; }
    public double PierwszySerwisProcentGracz2 { get; set; }
    public int PktNa1SerGracz1 { get; set; }
    public int PktNa1SerGracz2 { get; set; }
    public int PktNa2SerGracz1 { get; set; }
    public int PktNa2SerGracz2 { get; set; }
    public int WinnersGracz1 { get; set; }
    public int WinnersGracz2 { get; set; }
    public int UnforcedErrorsGracz1 { get; set; }
    public int UnforcedErrorsGracz2 { get; set; }
    public int BreakPktWykorzystaneGracz1 { get; set; }
    public int BreakPktWykorzystaneGracz2 { get; set; }
    public int BreakPktOkazjeGracz1 { get; set; }
    public int BreakPktOkazjeGracz2 { get; set; }
    public int CzasMeczuMin { get; set; }
    public int? Publicznosc { get; set; }
}
