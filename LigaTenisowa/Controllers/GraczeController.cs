using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

public class GraczeController : BaseController
{
    private readonly AppDbContext _db;

    public GraczeController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        var gracze = _db.Gracze.ToList();
        return View(gracze);
    }

    public IActionResult Szczegoly(int id)
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        if (!_db.Gracze.Any(g => g.Id == id))
            return NotFound();

        return RedirectToAction("Profil", "Statystyki", new { id });
    }

    public IActionResult Dodaj()
    {
        var redirect = SprawdzAdmina("Gracze");
        if (redirect != null) return redirect;

        return View();
    }

    [HttpPost]
    public IActionResult Dodaj(Gracz gracz)
    {
        var redirect = SprawdzAdmina("Gracze");
        if (redirect != null) return redirect;

        if (!ModelState.IsValid) return View(gracz);

        _db.Gracze.Add(gracz);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Edytuj(int id)
    {
        var redirect = SprawdzAdmina("Gracze");
        if (redirect != null) return redirect;

        var gracz = _db.Gracze.FirstOrDefault(g => g.Id == id);
        if (gracz == null) return NotFound();
        return View(gracz);
    }

    [HttpPost]
    public IActionResult Edytuj(Gracz gracz)
    {
        var redirect = SprawdzAdmina("Gracze");
        if (redirect != null) return redirect;

        if (!ModelState.IsValid) return View(gracz);

        _db.Gracze.Update(gracz);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Usun(int id)
    {
        var redirect = SprawdzAdmina("Gracze");
        if (redirect != null) return redirect;

        var gracz = _db.Gracze.FirstOrDefault(g => g.Id == id);
        if (gracz == null) return NotFound();

        if (!CzyMoznaUsunacGracza(id, out var powod))
            ViewBag.Blad = powod;

        return View(gracz);
    }

    [HttpPost, ActionName("Usun")]
    public IActionResult UsunPotwierdzony(int id)
    {
        var redirect = SprawdzAdmina("Gracze");
        if (redirect != null) return redirect;

        var gracz = _db.Gracze.FirstOrDefault(g => g.Id == id);
        if (gracz == null) return NotFound();

        if (!CzyMoznaUsunacGracza(id, out var powod))
        {
            TempData["Blad"] = powod;
            return RedirectToAction("Index");
        }

        try
        {
            _db.Gracze.Remove(gracz);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            TempData["Blad"] = "Nie można usunąć gracza - istnieją powiązane dane w bazie.";
            return RedirectToAction("Index");
        }
    }

    private bool CzyMoznaUsunacGracza(int id, out string powod)
    {
        if (_db.Mecze.Any(m => m.Gracz1Id == id || m.Gracz2Id == id || m.ZwyciezcaId == id))
        {
            powod = "Nie można usunąć gracza powiązanego z meczami. Najpierw usuń powiązane mecze.";
            return false;
        }

        if (_db.Uzytkownicy.Any(u => u.GraczId == id))
        {
            powod = "Nie można usunąć gracza z powiązanym kontem użytkownika. Usuń lub zmień użytkownika w panelu admina.";
            return false;
        }

        powod = "";
        return true;
    }
}