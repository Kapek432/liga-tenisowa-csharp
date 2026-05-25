using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class StatystykiController : BaseController
{
    private readonly AppDbContext _db;

    public StatystykiController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Profil(int id)
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        var gracz = _db.Gracze.FirstOrDefault(g => g.Id == id);
        if (gracz == null) return NotFound();

        var mecze = _db.Mecze
            .Include(m => m.Gracz1)
            .Include(m => m.Gracz2)
            .Include(m => m.Zwyciezca)
            .Include(m => m.Sety)
            .Include(m => m.Statystyki)
            .Where(m => m.Gracz1Id == id || m.Gracz2Id == id)
            .OrderByDescending(m => m.DataMeczu)
            .ToList();

        var rozegrane = mecze.Where(m => m.ZwyciezcaId != null).ToList();
        var statystyki = mecze
            .Where(m => m.Statystyki != null)
            .Select(m => new {
                m.Statystyki,
                JakoGracz1 = m.Gracz1Id == id
            }).ToList();

        var forma = rozegrane.Take(5)
            .Select(m => m.ZwyciezcaId == id ? "W" : "L");

        var model = new ProfilViewModel
        {
            Gracz = gracz,
            Wygrane = rozegrane.Count(m => m.ZwyciezcaId == id),
            Przegrane = rozegrane.Count(m => m.ZwyciezcaId != id),
            TotalAsy = statystyki.Sum(s => s.JakoGracz1 ?
                s.Statystyki.AsyGracz1 : s.Statystyki.AsyGracz2),
            TotalDoubleFaults = statystyki.Sum(s => s.JakoGracz1 ?
                s.Statystyki.DoubleFaultsGracz1 : s.Statystyki.DoubleFaultsGracz2),
            TotalWinners = statystyki.Sum(s => s.JakoGracz1 ?
                s.Statystyki.WinnersGracz1 : s.Statystyki.WinnersGracz2),
            TotalUnforcedErrors = statystyki.Sum(s => s.JakoGracz1 ?
                s.Statystyki.UnforcedErrorsGracz1 : s.Statystyki.UnforcedErrorsGracz2),
            SredniSerwisProcent = statystyki.Count > 0 ?
                Math.Round(statystyki.Average(s => s.JakoGracz1 ?
                    s.Statystyki.PierwszySerwisProcentGracz1 :
                    s.Statystyki.PierwszySerwisProcentGracz2), 1) : 0,
            OstatnieMecze = rozegrane.Take(5).ToList(),
            Forma = string.Join(" ", forma)
        };

        return View(model);
    }

    public IActionResult HeadToHead()
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        ViewBag.Gracze = _db.Gracze.ToList();
        return View();
    }

    [HttpPost]
    public IActionResult HeadToHead(int gracz1Id, int gracz2Id)
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        if (gracz1Id == gracz2Id)
        {
            ViewBag.Blad = "Wybierz dwóch różnych graczy.";
            ViewBag.Gracze = _db.Gracze.ToList();
            return View();
        }

        var mecze = _db.Mecze
            .Include(m => m.Gracz1)
            .Include(m => m.Gracz2)
            .Include(m => m.Zwyciezca)
            .Include(m => m.Sety)
            .Where(m =>
                (m.Gracz1Id == gracz1Id && m.Gracz2Id == gracz2Id) ||
                (m.Gracz1Id == gracz2Id && m.Gracz2Id == gracz1Id))
            .OrderByDescending(m => m.DataMeczu)
            .ToList();

        var model = new HeadToHeadViewModel
        {
            Gracz1 = _db.Gracze.Find(gracz1Id),
            Gracz2 = _db.Gracze.Find(gracz2Id),
            WygraneGracz1 = mecze.Count(m => m.ZwyciezcaId == gracz1Id),
            WygraneGracz2 = mecze.Count(m => m.ZwyciezcaId == gracz2Id),
            Mecze = mecze
        };

        ViewBag.Gracze = _db.Gracze.ToList();
        return View(model);
    }

    public IActionResult Rekordy()
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        var statystyki = _db.StatystykiMeczow
            .Include(s => s.Mecz).ThenInclude(m => m.Gracz1)
            .Include(s => s.Mecz).ThenInclude(m => m.Gracz2)
            .ToList();

        if (!statystyki.Any())
        {
            ViewBag.Blad = "Brak danych statystycznych.";
            return View(new RekordyViewModel());
        }

        var najdluzszy = _db.Mecze
            .Include(m => m.Gracz1)
            .Include(m => m.Gracz2)
            .Include(m => m.Statystyki)
            .Where(m => m.Statystyki != null)
            .OrderByDescending(m => m.Statystyki.CzasMeczuMin)
            .FirstOrDefault();

        var gracze = _db.Gracze.ToList();
        var kariery = ObliczRekordyKariery(statystyki, gracze);

        var model = new RekordyViewModel
        {
            MaxAsy = statystyki.Max(s => Math.Max(s.AsyGracz1, s.AsyGracz2)),
            NajwiecejAsow = statystyki
                .OrderByDescending(s => Math.Max(s.AsyGracz1, s.AsyGracz2))
                .Select(s => s.AsyGracz1 >= s.AsyGracz2 ? s.Mecz.Gracz1 : s.Mecz.Gracz2)
                .First(),

            MaxWinners = statystyki.Max(s => Math.Max(s.WinnersGracz1, s.WinnersGracz2)),
            NajwiecejWinners = statystyki
                .OrderByDescending(s => Math.Max(s.WinnersGracz1, s.WinnersGracz2))
                .Select(s => s.WinnersGracz1 >= s.WinnersGracz2 ? s.Mecz.Gracz1 : s.Mecz.Gracz2)
                .First(),

            MinBledow = statystyki.Min(s => Math.Min(s.UnforcedErrorsGracz1, s.UnforcedErrorsGracz2)),
            NajmniejBledow = statystyki
                .OrderBy(s => Math.Min(s.UnforcedErrorsGracz1, s.UnforcedErrorsGracz2))
                .Select(s => s.UnforcedErrorsGracz1 <= s.UnforcedErrorsGracz2 ? s.Mecz.Gracz1 : s.Mecz.Gracz2)
                .First(),

            MaxSerwisProc = statystyki.Max(s => Math.Max(s.PierwszySerwisProcentGracz1, s.PierwszySerwisProcentGracz2)),
            NajlepszySerwis = statystyki
                .OrderByDescending(s => Math.Max(s.PierwszySerwisProcentGracz1, s.PierwszySerwisProcentGracz2))
                .Select(s => s.PierwszySerwisProcentGracz1 >= s.PierwszySerwisProcentGracz2 ? s.Mecz.Gracz1 : s.Mecz.Gracz2)
                .First(),

            NajdluzszyMecz = najdluzszy,
            MaxCzasMeczu = najdluzszy?.Statystyki?.CzasMeczuMin ?? 0,

            LiderAsowKariery = kariery.LiderAsow,
            SumAsowKariery = kariery.SumAsow,
            LiderWinnersKariery = kariery.LiderWinners,
            SumWinnersKariery = kariery.SumWinners,
            NajmniejBledowKariery = kariery.NajmniejBledow,
            SumBledowKariery = kariery.SumBledow,
            NajlepszySerwisKariery = kariery.NajlepszySerwis,
            SredniSerwisKariery = kariery.SredniSerwis,
            NajwiecejCzasuKariery = kariery.NajwiecejCzasu,
            SumCzasuKariery = kariery.SumCzasu
        };

        return View(model);
    }

    private static (Gracz LiderAsow, int SumAsow, Gracz LiderWinners, int SumWinners,
        Gracz NajmniejBledow, int SumBledow, Gracz NajlepszySerwis, double SredniSerwis,
        Gracz NajwiecejCzasu, int SumCzasu) ObliczRekordyKariery(
        List<StatystykiMeczu> statystyki, List<Gracz> gracze)
    {
        var asy = new Dictionary<int, int>();
        var winners = new Dictionary<int, int>();
        var bledy = new Dictionary<int, int>();
        var czas = new Dictionary<int, int>();
        var serwis = new Dictionary<int, List<double>>();

        foreach (var s in statystyki)
        {
            void Dodaj(int graczId, int a, int w, int b, double serwProc)
            {
                asy[graczId] = asy.GetValueOrDefault(graczId) + a;
                winners[graczId] = winners.GetValueOrDefault(graczId) + w;
                bledy[graczId] = bledy.GetValueOrDefault(graczId) + b;
                czas[graczId] = czas.GetValueOrDefault(graczId) + s.CzasMeczuMin;
                if (!serwis.ContainsKey(graczId))
                    serwis[graczId] = new List<double>();
                serwis[graczId].Add(serwProc);
            }

            Dodaj(s.Mecz.Gracz1Id, s.AsyGracz1, s.WinnersGracz1, s.UnforcedErrorsGracz1, s.PierwszySerwisProcentGracz1);
            Dodaj(s.Mecz.Gracz2Id, s.AsyGracz2, s.WinnersGracz2, s.UnforcedErrorsGracz2, s.PierwszySerwisProcentGracz2);
        }

        Gracz GraczPoId(int id) => gracze.First(g => g.Id == id);

        var liderAsowId = asy.OrderByDescending(kv => kv.Value).First().Key;
        var liderWinnersId = winners.OrderByDescending(kv => kv.Value).First().Key;
        var najmniejBledowId = bledy.OrderBy(kv => kv.Value).First().Key;
        var najlepszySerwis = serwis
            .Select(kv => (Id: kv.Key, Srednia: Math.Round(kv.Value.Average(), 1)))
            .OrderByDescending(x => x.Srednia)
            .First();
        var najwiecejCzasuId = czas.OrderByDescending(kv => kv.Value).First().Key;

        return (
            GraczPoId(liderAsowId), asy[liderAsowId],
            GraczPoId(liderWinnersId), winners[liderWinnersId],
            GraczPoId(najmniejBledowId), bledy[najmniejBledowId],
            GraczPoId(najlepszySerwis.Id), najlepszySerwis.Srednia,
            GraczPoId(najwiecejCzasuId), czas[najwiecejCzasuId]);
    }

}