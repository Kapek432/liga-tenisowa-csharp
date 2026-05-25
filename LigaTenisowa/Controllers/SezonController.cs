using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class SezonController : BaseController
{
    private readonly AppDbContext _db;

    public SezonController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        var sezony = _db.Sezony.ToList();
        return View(sezony);
    }

    public IActionResult Szczegoly(int id)
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        var sezon = _db.Sezony.FirstOrDefault(s => s.Id == id);
        if (sezon == null) return NotFound();

        ViewBag.Mecze = _db.Mecze
            .Include(m => m.Gracz1)
            .Include(m => m.Gracz2)
            .Include(m => m.Zwyciezca)
            .Where(m => m.SezonId == id)
            .OrderBy(m => m.DataMeczu)
            .ToList();

        return View(sezon);
    }

    public IActionResult Dodaj()
    {
        var redirect = SprawdzAdmina("Sezon");
        if (redirect != null) return redirect;

        return View();
    }

    [HttpPost]
    public IActionResult Dodaj(Sezon sezon)
    {
        var redirect = SprawdzAdmina("Sezon");
        if (redirect != null) return redirect;

        if (!ModelState.IsValid) return View(sezon);

        // tylko jeden sezon może być aktywny
        if (sezon.CzyAktywny)
        {
            var aktywne = _db.Sezony.Where(s => s.CzyAktywny).ToList();
            aktywne.ForEach(s => s.CzyAktywny = false);
        }

        _db.Sezony.Add(sezon);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Edytuj(int id)
    {
        var redirect = SprawdzAdmina("Sezon");
        if (redirect != null) return redirect;

        var sezon = _db.Sezony.FirstOrDefault(s => s.Id == id);
        if (sezon == null) return NotFound();
        return View(sezon);
    }

    [HttpPost]
    public IActionResult Edytuj(Sezon sezon)
    {
        var redirect = SprawdzAdmina("Sezon");
        if (redirect != null) return redirect;

        if (!ModelState.IsValid) return View(sezon);

        if (sezon.CzyAktywny)
        {
            var aktywne = _db.Sezony.Where(s => s.CzyAktywny && s.Id != sezon.Id).ToList();
            aktywne.ForEach(s => s.CzyAktywny = false);
        }

        _db.Sezony.Update(sezon);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Usun(int id)
    {
        var redirect = SprawdzAdmina("Sezon");
        if (redirect != null) return redirect;

        var sezon = _db.Sezony.FirstOrDefault(s => s.Id == id);
        if (sezon == null) return NotFound();
        return View(sezon);
    }

    [HttpPost, ActionName("Usun")]
    public IActionResult UsunPotwierdzony(int id)
    {
        var redirect = SprawdzAdmina("Sezon");
        if (redirect != null) return redirect;

        var sezon = _db.Sezony.FirstOrDefault(s => s.Id == id);
        if (sezon == null) return NotFound();

        _db.Sezony.Remove(sezon);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }
}