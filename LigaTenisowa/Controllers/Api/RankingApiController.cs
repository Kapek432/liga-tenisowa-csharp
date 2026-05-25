using Microsoft.AspNetCore.Mvc;

[Route("api/ranking")]
public class RankingApiController : BaseApiController
{
    private readonly RankingService _ranking;

    public RankingApiController(AppDbContext db, RankingService ranking) : base(db)
    {
        _ranking = ranking;
    }

    [HttpGet]
    public IActionResult GetSezon()
    {
        if (!SprawdzToken()) return Unauthorized("Brak autoryzacji.");

        var aktywnySezon = _db.Sezony.FirstOrDefault(s => s.CzyAktywny);
        if (aktywnySezon == null) return NotFound("Brak aktywnego sezonu.");

        var ranking = _ranking.ObliczRanking(aktywnySezon.Id);
        return Ok(new { Sezon = aktywnySezon.Nazwa, Ranking = ranking });
    }

    [HttpGet("alltime")]
    public IActionResult GetAllTime()
    {
        if (!SprawdzToken()) return Unauthorized("Brak autoryzacji.");

        var ranking = _ranking.ObliczRanking(null);
        return Ok(ranking);
    }
}
