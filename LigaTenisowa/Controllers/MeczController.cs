using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class MeczController : BaseController
{
    private readonly AppDbContext _db;

    public MeczController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Kalendarz()
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        var mecze = _db.Mecze
            .Include(m => m.Gracz1)
            .Include(m => m.Gracz2)
            .Include(m => m.Sezon)
            .Where(m => m.ZwyciezcaId == null)
            .OrderBy(m => m.DataMeczu)
            .ToList();

        return View(mecze);
    }

    public IActionResult Index()
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        var mecze = _db.Mecze
            .Include(m => m.Gracz1)
            .Include(m => m.Gracz2)
            .Include(m => m.Zwyciezca)
            .Include(m => m.Sezon)
            .ToList();
        return View(mecze);
    }

    public IActionResult Szczegoly(int id)
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        var mecz = _db.Mecze
            .Include(m => m.Gracz1)
            .Include(m => m.Gracz2)
            .Include(m => m.Zwyciezca)
            .Include(m => m.Sezon)
            .Include(m => m.Sety)
            .Include(m => m.Statystyki)
            .FirstOrDefault(m => m.Id == id);

        if (mecz == null) return NotFound();
        return View(mecz);
    }

    public IActionResult Dodaj()
    {
        var redirect = SprawdzAdmina("Mecz");
        if (redirect != null) return redirect;

        ViewBag.Gracze = _db.Gracze.ToList();
        ViewBag.Sezony = _db.Sezony.ToList();
        return View();
    }

    [HttpPost]
    public IActionResult Dodaj(Mecz mecz)
    {
        var redirect = SprawdzAdmina("Mecz");
        if (redirect != null) return redirect;

        if (mecz.Gracz1Id == mecz.Gracz2Id)
        {
            ModelState.AddModelError("", "Gracz nie może grać sam ze sobą.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Gracze = _db.Gracze.ToList();
            ViewBag.Sezony = _db.Sezony.ToList();
            return View(mecz);
        }

        _db.Mecze.Add(mecz);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Edytuj(int id)
    {
        var redirect = SprawdzAdmina("Mecz");
        if (redirect != null) return redirect;

        var mecz = _db.Mecze.FirstOrDefault(m => m.Id == id);
        if (mecz == null) return NotFound();

        ViewBag.Gracze = _db.Gracze.ToList();
        ViewBag.Sezony = _db.Sezony.ToList();
        return View(mecz);
    }

    [HttpPost]
    public IActionResult Edytuj(Mecz mecz)
    {
        var redirect = SprawdzAdmina("Mecz");
        if (redirect != null) return redirect;

        if (mecz.Gracz1Id == mecz.Gracz2Id)
        {
            ModelState.AddModelError("", "Gracz nie może grać sam ze sobą.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Gracze = _db.Gracze.ToList();
            ViewBag.Sezony = _db.Sezony.ToList();
            return View(mecz);
        }

        _db.Mecze.Update(mecz);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Usun(int id)
    {
        var redirect = SprawdzAdmina("Mecz");
        if (redirect != null) return redirect;

        var mecz = _db.Mecze
            .Include(m => m.Gracz1)
            .Include(m => m.Gracz2)
            .Include(m => m.Sezon)
            .FirstOrDefault(m => m.Id == id);

        if (mecz == null) return NotFound();
        return View(mecz);
    }

    [HttpPost, ActionName("Usun")]
    public IActionResult UsunPotwierdzony(int id)
    {
        var redirect = SprawdzAdmina("Mecz");
        if (redirect != null) return redirect;

        var mecz = _db.Mecze
            .Include(m => m.Sety)
            .Include(m => m.Statystyki)
            .FirstOrDefault(m => m.Id == id);

        if (mecz == null) return NotFound();

        _db.Mecze.Remove(mecz);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult WpiszWynik(int id)
    {
        var mecz = _db.Mecze
            .Include(m => m.Gracz1)
            .Include(m => m.Gracz2)
            .Include(m => m.Sety)
            .Include(m => m.Statystyki)
            .FirstOrDefault(m => m.Id == id);

        if (mecz == null) return NotFound();

        var brakUprawnien = SprawdzWpisWynik(mecz);
        if (brakUprawnien != null) return brakUprawnien;

        return View(mecz);
    }

    [HttpPost]
    public IActionResult WpiszWynik(int id, List<Set> sety, StatystykiMeczu statystyki, int zwyciezcaId)
    {
        var mecz = _db.Mecze
            .Include(m => m.Sety)
            .Include(m => m.Statystyki)
            .FirstOrDefault(m => m.Id == id);

        if (mecz == null) return NotFound();

        var brakUprawnien = SprawdzWpisWynik(mecz);
        if (brakUprawnien != null) return brakUprawnien;

        // Usuń stare sety
        _db.Sety.RemoveRange(mecz.Sety);

        // Dodaj nowe sety
        foreach (var set in sety)
        {
            set.MeczId = id;
            _db.Sety.Add(set);
        }

        // Statystyki
        if (mecz.Statystyki != null)
            _db.StatystykiMeczow.Remove(mecz.Statystyki);

        statystyki.MeczId = id;
        _db.StatystykiMeczow.Add(statystyki);

        // Zwycięzca
            mecz.ZwyciezcaId = zwyciezcaId;

        _db.SaveChanges();
        return RedirectToAction("Szczegoly", new { id });
    }
}