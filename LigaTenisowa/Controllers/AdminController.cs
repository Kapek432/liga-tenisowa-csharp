using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AdminController : BaseController
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    private IActionResult SprawdzAdmina()
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;
        if (!CzyAdmin) return Forbid();
        return null;
    }

    public IActionResult Index()
    {
        var redirect = SprawdzAdmina();
        if (redirect != null) return redirect;

        var uzytkownicy = _db.Uzytkownicy
            .Include(u => u.Gracz)
            .ToList();
        return View(uzytkownicy);
    }

    public IActionResult Dodaj()
    {
        var redirect = SprawdzAdmina();
        if (redirect != null) return redirect;

        ViewBag.Gracze = _db.Gracze.ToList();
        return View();
    }

    [HttpPost]
    public IActionResult Dodaj(string login, string haslo, bool czyAdmin, int? graczId)
    {
        var redirect = SprawdzAdmina();
        if (redirect != null) return redirect;

        if (_db.Uzytkownicy.Any(u => u.Login == login))
        {
            ModelState.AddModelError("", "Użytkownik o takim loginie już istnieje.");
            ViewBag.Gracze = _db.Gracze.ToList();
            return View();
        }

        var uzytkownik = new Uzytkownik
        {
            Login = login,
            HasloHash = BCrypt.Net.BCrypt.HashPassword(haslo),
            ApiToken = Guid.NewGuid().ToString(),
            CzyAdmin = czyAdmin,
            GraczId = graczId == 0 ? null : graczId
        };

        _db.Uzytkownicy.Add(uzytkownik);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Usun(int id)
    {
        var redirect = SprawdzAdmina();
        if (redirect != null) return redirect;

        var uzytkownik = _db.Uzytkownicy
            .Include(u => u.Gracz)
            .FirstOrDefault(u => u.Id == id);

        if (uzytkownik == null) return NotFound();
        return View(uzytkownik);
    }

    [HttpPost, ActionName("Usun")]
    public IActionResult UsunPotwierdzony(int id)
    {
        var redirect = SprawdzAdmina();
        if (redirect != null) return redirect;

        var uzytkownik = _db.Uzytkownicy.FirstOrDefault(u => u.Id == id);
        if (uzytkownik == null) return NotFound();

        // Nie można usunąć własnego konta
        if (uzytkownik.Login == HttpContext.Session.GetString("Login"))
        {
            TempData["Blad"] = "Nie możesz usunąć własnego konta.";
            return RedirectToAction("Index");
        }

        _db.Uzytkownicy.Remove(uzytkownik);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult ResetujToken(int id)
    {
        var redirect = SprawdzAdmina();
        if (redirect != null) return redirect;

        var uzytkownik = _db.Uzytkownicy.FirstOrDefault(u => u.Id == id);
        if (uzytkownik == null) return NotFound();

        uzytkownik.ApiToken = Guid.NewGuid().ToString();
        _db.SaveChanges();

        TempData["Info"] = $"Token użytkownika {uzytkownik.Login} został zresetowany.";
        return RedirectToAction("Index");
    }
}