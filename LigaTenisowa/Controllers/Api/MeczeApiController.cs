using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/mecze")]
public class MeczeApiController : BaseApiController
{
    public MeczeApiController(AppDbContext db) : base(db) { }

    [HttpGet]
    public IActionResult GetAll()
    {
        if (!SprawdzToken()) return Unauthorized("Brak autoryzacji.");

        var mecze = _db.Mecze
            .Include(m => m.Gracz1)
            .Include(m => m.Gracz2)
            .Include(m => m.Zwyciezca)
            .Include(m => m.Sezon)
            .Select(m => new
            {
                m.Id,
                m.DataMeczu,
                Nawierzchnia = EnumLabels.GetNawierzchnia(m.Nawierzchnia),
                Format = EnumLabels.GetFormat(m.Format),
                Sezon = m.Sezon.Nazwa,
                Gracz1 = $"{m.Gracz1.Imie} {m.Gracz1.Nazwisko}",
                Gracz2 = $"{m.Gracz2.Imie} {m.Gracz2.Nazwisko}",
                Zwyciezca = m.Zwyciezca != null ? $"{m.Zwyciezca.Imie} {m.Zwyciezca.Nazwisko}" : null
            }).ToList();

        return Ok(mecze);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        if (!SprawdzToken()) return Unauthorized("Brak autoryzacji.");

        var mecz = _db.Mecze
            .Include(m => m.Gracz1)
            .Include(m => m.Gracz2)
            .Include(m => m.Zwyciezca)
            .Include(m => m.Sezon)
            .Include(m => m.Sety)
            .Include(m => m.Statystyki)
            .FirstOrDefault(m => m.Id == id);

        if (mecz == null) return NotFound("Mecz nie istnieje.");

        return Ok(new
        {
            mecz.Id,
            mecz.DataMeczu,
            Nawierzchnia = EnumLabels.GetNawierzchnia(mecz.Nawierzchnia),
            Format = EnumLabels.GetFormat(mecz.Format),
            Sezon = mecz.Sezon.Nazwa,
            Gracz1 = $"{mecz.Gracz1.Imie} {mecz.Gracz1.Nazwisko}",
            Gracz2 = $"{mecz.Gracz2.Imie} {mecz.Gracz2.Nazwisko}",
            Zwyciezca = mecz.Zwyciezca != null ? $"{mecz.Zwyciezca.Imie} {mecz.Zwyciezca.Nazwisko}" : null,
            Sety = mecz.Sety.OrderBy(s => s.NumerSeta).Select(s => new
            {
                s.NumerSeta,
                s.GemyGracz1,
                s.GemyGracz2,
                s.TiebreakGracz1,
                s.TiebreakGracz2
            }),
            Statystyki = MapStatystyki(mecz.Statystyki)
        });
    }

    private static object MapStatystyki(StatystykiMeczu st)
    {
        if (st == null) return null;

        return new
        {
            st.AsyGracz1,
            st.AsyGracz2,
            st.DoubleFaultsGracz1,
            st.DoubleFaultsGracz2,
            st.PierwszySerwisProcentGracz1,
            st.PierwszySerwisProcentGracz2,
            st.PktNa1SerGracz1,
            st.PktNa1SerGracz2,
            st.PktNa2SerGracz1,
            st.PktNa2SerGracz2,
            st.WinnersGracz1,
            st.WinnersGracz2,
            st.UnforcedErrorsGracz1,
            st.UnforcedErrorsGracz2,
            st.BreakPktWykorzystaneGracz1,
            st.BreakPktWykorzystaneGracz2,
            st.BreakPktOkazjeGracz1,
            st.BreakPktOkazjeGracz2,
            st.CzasMeczuMin,
            st.Publicznosc
        };
    }

    [HttpPost]
    public IActionResult Post([FromBody] Mecz mecz)
    {
        if (!SprawdzToken()) return Unauthorized("Brak autoryzacji.");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (mecz.Gracz1Id == mecz.Gracz2Id)
            return BadRequest("Gracz nie może grać sam ze sobą.");

        if (!_db.Gracze.Any(g => g.Id == mecz.Gracz1Id) || !_db.Gracze.Any(g => g.Id == mecz.Gracz2Id))
            return BadRequest("Wybrany gracz nie istnieje.");

        if (!_db.Sezony.Any(s => s.Id == mecz.SezonId))
            return BadRequest("Wybrany sezon nie istnieje.");

        try
        {
            _db.Mecze.Add(mecz);
            _db.SaveChanges();
            return CreatedAtAction(nameof(Get), new { id = mecz.Id }, new { mecz.Id, mecz.DataMeczu });
        }
        catch (DbUpdateException ex)
        {
            return BadRequest($"Nie udało się dodać meczu: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    [HttpPut("{id}/wynik")]
    public IActionResult PutWynik(int id, [FromBody] WynikRequest request)
    {
        if (!SprawdzToken()) return Unauthorized("Brak autoryzacji.");
        if (request == null) return BadRequest("Brak danych wyniku.");

        var uzytkownik = PobierzUzytkownikaZTokena();
        if (uzytkownik == null) return Unauthorized("Brak autoryzacji.");

        var mecz = _db.Mecze
            .Include(m => m.Sety)
            .Include(m => m.Statystyki)
            .FirstOrDefault(m => m.Id == id);

        if (mecz == null) return NotFound("Mecz nie istnieje.");

        if (!MozeWpisacWynik(mecz, uzytkownik))
            return Unauthorized("Możesz wpisywać wyniki tylko do meczów, w których bierzesz udział.");

        if (request.ZwyciezcaId != mecz.Gracz1Id && request.ZwyciezcaId != mecz.Gracz2Id)
            return BadRequest("Zwycięzca musi być jednym z graczy meczu.");

        try
        {
            _db.Sety.RemoveRange(mecz.Sety);
            if (mecz.Statystyki != null)
                _db.StatystykiMeczow.Remove(mecz.Statystyki);

            if (request.Sety != null)
            {
                foreach (var set in request.Sety)
                {
                    set.MeczId = id;
                    _db.Sety.Add(set);
                }
            }

            if (request.Statystyki != null)
            {
                request.Statystyki.MeczId = id;
                _db.StatystykiMeczow.Add(request.Statystyki);
            }

            mecz.ZwyciezcaId = request.ZwyciezcaId;
            _db.SaveChanges();
            return Ok("Wynik zapisany.");
        }
        catch (DbUpdateException ex)
        {
            return BadRequest($"Nie udało się zapisać wyniku: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (!SprawdzAdmina()) return Unauthorized("Tylko administrator może usuwać mecze.");

        var mecz = _db.Mecze
            .Include(m => m.Sety)
            .Include(m => m.Statystyki)
            .FirstOrDefault(m => m.Id == id);

        if (mecz == null) return NotFound("Mecz nie istnieje.");

        try
        {
            _db.Mecze.Remove(mecz);
            _db.SaveChanges();
            return Ok("Mecz usunięty.");
        }
        catch (DbUpdateException ex)
        {
            return BadRequest($"Nie udało się usunąć meczu: {ex.InnerException?.Message ?? ex.Message}");
        }
    }
}

public class WynikRequest
{
    public int ZwyciezcaId { get; set; }
    public List<Set> Sety { get; set; }
    public StatystykiMeczu Statystyki { get; set; }
}
