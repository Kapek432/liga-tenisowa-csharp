using Microsoft.AspNetCore.Mvc;

public class AuthController : Controller
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("Login") != null)
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public IActionResult Login(string login, string haslo)
    {
        var user = _db.Uzytkownicy.FirstOrDefault(u => u.Login == login);

        if (user == null || !BCrypt.Net.BCrypt.Verify(haslo, user.HasloHash))
        {
            ViewBag.Error = "Nieprawidłowy login lub hasło.";
            return View();
        }

        HttpContext.Session.SetString("Login", user.Login);
        HttpContext.Session.SetString("CzyAdmin", user.CzyAdmin.ToString());
        HttpContext.Session.SetInt32("UzytkownikId", user.Id);
        if (user.GraczId.HasValue)
            HttpContext.Session.SetInt32("GraczId", user.GraczId.Value);
        else
            HttpContext.Session.Remove("GraczId");

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}