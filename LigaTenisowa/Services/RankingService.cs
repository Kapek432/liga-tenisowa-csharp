using Microsoft.EntityFrameworkCore;

public class RankingService
{
    private readonly AppDbContext _db;

    public RankingService(AppDbContext db)
    {
        _db = db;
    }

    public List<RankingWiersz> ObliczRanking(int? sezonId)
    {
        var gracze = _db.Gracze.ToList();
        var query = _db.Mecze
            .Include(m => m.Sety)
            .Where(m => m.ZwyciezcaId != null);

        if (sezonId.HasValue)
            query = query.Where(m => m.SezonId == sezonId.Value);

        var mecze = query.OrderByDescending(m => m.DataMeczu).ToList();

        return gracze.Select(g => new RankingWiersz
        {
            GraczId = g.Id,
            Imie = g.Imie,
            Nazwisko = g.Nazwisko,
            Wygrane = mecze.Count(m => m.ZwyciezcaId == g.Id),
            Przegrane = mecze.Count(m => (m.Gracz1Id == g.Id || m.Gracz2Id == g.Id) && m.ZwyciezcaId != g.Id),
            Punkty = ObliczPunkty(g.Id, mecze),
            Forma = ObliczForme(g.Id, mecze)
        })
        .OrderByDescending(r => r.Punkty)
        .ThenByDescending(r => r.Wygrane)
        .ToList();
    }

    public int ObliczPunkty(int graczId, List<Mecz> mecze)
    {
        int punkty = 0;
        foreach (var m in mecze.Where(m => m.Gracz1Id == graczId || m.Gracz2Id == graczId))
        {
            bool wygral = m.ZwyciezcaId == graczId;
            int setyWygrane = m.Sety.Count(s =>
                (m.Gracz1Id == graczId && s.GemyGracz1 > s.GemyGracz2) ||
                (m.Gracz2Id == graczId && s.GemyGracz2 > s.GemyGracz1));

            if (wygral)
                punkty += setyWygrane == 2 ? 120 : 100;
            else
                punkty += setyWygrane == 1 ? 20 : 10;
        }
        return punkty;
    }

    public string ObliczForme(int graczId, List<Mecz> mecze)
    {
        var ostatnie = mecze
            .Where(m => m.Gracz1Id == graczId || m.Gracz2Id == graczId)
            .OrderByDescending(m => m.DataMeczu)
            .Take(5)
            .Select(m => m.ZwyciezcaId == graczId ? "W" : "L");

        return string.Join(" ", ostatnie);
    }
}

public class RankingWiersz
{
    public int GraczId { get; set; }
    public string Imie { get; set; }
    public string Nazwisko { get; set; }
    public int Wygrane { get; set; }
    public int Przegrane { get; set; }
    public int Punkty { get; set; }
    public string Forma { get; set; }
    public double WinRatio => (Wygrane + Przegrane) == 0 ? 0 :
        Math.Round((double)Wygrane / (Wygrane + Przegrane) * 100, 1);
}
