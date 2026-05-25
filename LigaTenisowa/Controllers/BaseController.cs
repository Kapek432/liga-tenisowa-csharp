using Microsoft.AspNetCore.Mvc;

public class BaseController : Controller
{
    protected bool CzyZalogowany => 
        HttpContext.Session.GetString("Login") != null;

    protected bool CzyAdmin => 
        HttpContext.Session.GetString("CzyAdmin") == "True";

    protected int? SesjaGraczId => HttpContext.Session.GetInt32("GraczId");

    protected bool MozeWpisacWynik(Mecz mecz)
    {
        if (CzyAdmin)
            return true;
        if (!SesjaGraczId.HasValue)
            return false;
        return mecz.Gracz1Id == SesjaGraczId || mecz.Gracz2Id == SesjaGraczId;
    }

    protected IActionResult SprawdzSesje()
    {
        if (!CzyZalogowany)
            return RedirectToAction("Login", "Auth");
        return null;
    }

    protected IActionResult SprawdzAdmina(string redirectController = "Home", string redirectAction = "Index")
    {
        var redirect = SprawdzSesje();
        if (redirect != null)
            return redirect;

        if (!CzyAdmin)
        {
            TempData["Blad"] = "Ta operacja wymaga uprawnień administratora.";
            return RedirectToAction(redirectAction, redirectController);
        }

        return null;
    }

    protected IActionResult SprawdzWpisWynik(Mecz mecz, string redirectController = "Mecz", string redirectAction = "Index")
    {
        var redirect = SprawdzSesje();
        if (redirect != null)
            return redirect;

        if (!MozeWpisacWynik(mecz))
        {
            TempData["Blad"] = "Możesz wpisywać wyniki tylko do meczów, w których bierzesz udział.";
            return RedirectToAction(redirectAction, redirectController);
        }

        return null;
    }
}