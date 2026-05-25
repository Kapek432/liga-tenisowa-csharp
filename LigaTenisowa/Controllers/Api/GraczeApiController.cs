using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/gracze")]
public class GraczeApiController : BaseApiController
{
    private readonly RankingService _ranking;

    public GraczeApiController(AppDbContext db, RankingService ranking) : base(db)
    {
        _ranking = ranking;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        if (!SprawdzToken()) return Unauthorized("Brak autoryzacji.");

        var aktywnySezon = _db.Sezony.FirstOrDefault(s => s.CzyAktywny);
        var ranking = _ranking.ObliczRanking(aktywnySezon?.Id);

        var gracze = _db.Gracze.Select(g => new
        {
            g.Id,
            g.Imie,
            g.Nazwisko,
            g.Kraj,
            Reka = EnumLabels.GetReka(g.Reka),
            StylGry = EnumLabels.GetStylGry(g.StylGry),
            g.DataUrodzenia
        }).ToList();

        var wynik = gracze.Select(g =>
        {
            var r = ranking.FirstOrDefault(x => x.GraczId == g.Id);
            return new
            {
                g.Id,
                g.Imie,
                g.Nazwisko,
                g.Kraj,
                g.Reka,
                g.StylGry,
                g.DataUrodzenia,
                Wygrane = r?.Wygrane ?? 0,
                Przegrane = r?.Przegrane ?? 0,
                Punkty = r?.Punkty ?? 0,
                Forma = r?.Forma ?? ""
            };
        })
        .OrderByDescending(g => g.Punkty)
        .ToList();

        return Ok(wynik);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        if (!SprawdzToken()) return Unauthorized("Brak autoryzacji.");

        var gracz = _db.Gracze.FirstOrDefault(g => g.Id == id);
        if (gracz == null) return NotFound("Gracz nie istnieje.");

        var ranking = _ranking.ObliczRanking(null);
        var r = ranking.FirstOrDefault(x => x.GraczId == id);

        return Ok(new
        {
            gracz.Id,
            gracz.Imie,
            gracz.Nazwisko,
            gracz.Kraj,
            Reka = EnumLabels.GetReka(gracz.Reka),
            StylGry = EnumLabels.GetStylGry(gracz.StylGry),
            gracz.DataUrodzenia,
            Wygrane = r?.Wygrane ?? 0,
            Przegrane = r?.Przegrane ?? 0,
            Punkty = r?.Punkty ?? 0,
            Forma = r?.Forma ?? ""
        });
    }

    [HttpPost]
    public IActionResult Post([FromBody] Gracz gracz)
    {
        if (!SprawdzAdmina()) return Unauthorized("Tylko administrator może dodawać graczy.");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            _db.Gracze.Add(gracz);
            _db.SaveChanges();
            return CreatedAtAction(nameof(Get), new { id = gracz.Id }, new
            {
                gracz.Id,
                gracz.Imie,
                gracz.Nazwisko,
                Reka = EnumLabels.GetReka(gracz.Reka),
                StylGry = EnumLabels.GetStylGry(gracz.StylGry)
            });
        }
        catch (DbUpdateException ex)
        {
            return BadRequest($"Nie udało się dodać gracza: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] Gracz gracz)
    {
        if (!SprawdzAdmina()) return Unauthorized("Tylko administrator może edytować graczy.");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var istniejacy = _db.Gracze.FirstOrDefault(g => g.Id == id);
        if (istniejacy == null) return NotFound("Gracz nie istnieje.");

        istniejacy.Imie = gracz.Imie;
        istniejacy.Nazwisko = gracz.Nazwisko;
        istniejacy.Kraj = gracz.Kraj;
        istniejacy.Reka = gracz.Reka;
        istniejacy.StylGry = gracz.StylGry;
        istniejacy.DataUrodzenia = gracz.DataUrodzenia;

        try
        {
            _db.SaveChanges();
            return Ok(new
            {
                istniejacy.Id,
                istniejacy.Imie,
                istniejacy.Nazwisko,
                Reka = EnumLabels.GetReka(istniejacy.Reka),
                StylGry = EnumLabels.GetStylGry(istniejacy.StylGry)
            });
        }
        catch (DbUpdateException ex)
        {
            return BadRequest($"Nie udało się zaktualizować gracza: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (!SprawdzAdmina()) return Unauthorized("Tylko administrator może usuwać graczy.");

        var gracz = _db.Gracze.FirstOrDefault(g => g.Id == id);
        if (gracz == null) return NotFound("Gracz nie istnieje.");

        if (_db.Mecze.Any(m => m.Gracz1Id == id || m.Gracz2Id == id || m.ZwyciezcaId == id))
            return BadRequest("Nie można usunąć gracza powiązanego z meczami. Najpierw usuń powiązane mecze.");

        if (_db.Uzytkownicy.Any(u => u.GraczId == id))
            return BadRequest("Nie można usunąć gracza z powiązanym kontem użytkownika.");

        try
        {
            _db.Gracze.Remove(gracz);
            _db.SaveChanges();
            return Ok("Gracz usunięty.");
        }
        catch (DbUpdateException ex)
        {
            return BadRequest($"Nie udało się usunąć gracza: {ex.InnerException?.Message ?? ex.Message}");
        }
    }
}
