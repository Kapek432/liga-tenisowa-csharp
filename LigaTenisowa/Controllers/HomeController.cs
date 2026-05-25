using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LigaTenisowa.Models;

namespace LigaTenisowa.Controllers;

public class HomeController : BaseController
{
    public IActionResult Index()
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        return View();
    }

    public IActionResult Privacy()
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;
        return RedirectToAction("Dokumentacja");
    }

    public IActionResult Dokumentacja()
    {
        var redirect = SprawdzSesje();
        if (redirect != null) return redirect;

        ViewBag.ReadmeHtml = ReadmeLoader.LoadAsHtml();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}