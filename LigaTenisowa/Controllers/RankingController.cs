using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class RankingController : BaseController
{
    private readonly AppDbContext _db;
    private readonly RankingService _ranking;

    public RankingController(AppDbContext db, RankingService ranking)
    {
        _db = db;
        _ranking = ranking;
    }

    public IActionResult Index()
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        var aktywnySezon = _db.Sezony.FirstOrDefault(s => s.CzyAktywny);
        if (aktywnySezon == null)
        {
            ViewBag.Blad = "Brak aktywnego sezonu.";
            return View(new List<RankingViewModel>());
        }

        var ranking = MapujDoViewModel(_ranking.ObliczRanking(aktywnySezon.Id));
        ViewBag.Sezon = aktywnySezon.Nazwa;
        return View(ranking);
    }

    public IActionResult AllTime()
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        var ranking = MapujDoViewModel(_ranking.ObliczRanking(null));
        return View(ranking);
    }

    private List<RankingViewModel> MapujDoViewModel(List<RankingWiersz> wiersze)
    {
        var gracze = _db.Gracze.ToDictionary(g => g.Id);
        return wiersze
            .Where(w => gracze.ContainsKey(w.GraczId))
            .Select(w => new RankingViewModel
            {
                Gracz = gracze[w.GraczId],
                Wygrane = w.Wygrane,
                Przegrane = w.Przegrane,
                Punkty = w.Punkty,
                Forma = w.Forma
            })
            .ToList();
    }
}
