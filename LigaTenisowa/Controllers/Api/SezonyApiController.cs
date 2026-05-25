using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/sezony")]
public class SezonyApiController : BaseApiController
{
    public SezonyApiController(AppDbContext db) : base(db) { }

    [HttpGet]
    public IActionResult GetAll()
    {
        if (!SprawdzToken()) return Unauthorized("Brak autoryzacji.");

        var sezony = _db.Sezony.Select(s => new
        {
            s.Id,
            s.Nazwa,
            s.DataRozpoczecia,
            s.DataZakonczenia,
            s.CzyAktywny,
            LiczbaMeczow = _db.Mecze.Count(m => m.SezonId == s.Id)
        }).ToList();

        return Ok(sezony);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        if (!SprawdzToken()) return Unauthorized("Brak autoryzacji.");

        var sezon = _db.Sezony.FirstOrDefault(s => s.Id == id);
        if (sezon == null) return NotFound("Sezon nie istnieje.");

        return Ok(new
        {
            sezon.Id,
            sezon.Nazwa,
            sezon.DataRozpoczecia,
            sezon.DataZakonczenia,
            sezon.CzyAktywny
        });
    }
}
