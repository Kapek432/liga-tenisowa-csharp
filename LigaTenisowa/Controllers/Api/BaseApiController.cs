using Microsoft.AspNetCore.Mvc;

[ApiController]
public class BaseApiController : ControllerBase
{
    protected readonly AppDbContext _db;

    public BaseApiController(AppDbContext db)
    {
        _db = db;
    }

    protected bool SprawdzToken()
    {
        var login = Request.Headers["X-Username"].ToString();
        var token = Request.Headers["X-Api-Token"].ToString();

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(token))
            return false;

        return _db.Uzytkownicy.Any(u => u.Login == login && u.ApiToken == token);
    }

    protected bool SprawdzAdmina()
    {
        var login = Request.Headers["X-Username"].ToString();
        var token = Request.Headers["X-Api-Token"].ToString();

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(token))
            return false;

        return _db.Uzytkownicy.Any(u => u.Login == login && u.ApiToken == token && u.CzyAdmin);
    }

    protected Uzytkownik? PobierzUzytkownikaZTokena()
    {
        var login = Request.Headers["X-Username"].ToString();
        var token = Request.Headers["X-Api-Token"].ToString();
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(token))
            return null;
        return _db.Uzytkownicy.FirstOrDefault(u => u.Login == login && u.ApiToken == token);
    }

    protected bool MozeWpisacWynik(Mecz mecz, Uzytkownik uzytkownik)
    {
        if (uzytkownik.CzyAdmin)
            return true;
        if (!uzytkownik.GraczId.HasValue)
            return false;
        return mecz.Gracz1Id == uzytkownik.GraczId || mecz.Gracz2Id == uzytkownik.GraczId;
    }
}