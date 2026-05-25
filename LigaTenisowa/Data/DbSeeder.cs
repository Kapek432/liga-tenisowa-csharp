using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Uzytkownicy.Any())
            return;

        var seedPath = Path.Combine(AppContext.BaseDirectory, "Data", "seed.json");
        if (!File.Exists(seedPath))
            seedPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "seed.json");

        if (!File.Exists(seedPath))
            throw new FileNotFoundException($"Nie znaleziono pliku seed: {seedPath}");

        var json = File.ReadAllText(seedPath);
        var seed = JsonSerializer.Deserialize<SeedFile>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (seed == null)
            throw new InvalidOperationException("Nie udało się wczytać danych seed.");

        db.Uzytkownicy.Add(new Uzytkownik
        {
            Login = seed.Admin.Login,
            HasloHash = BCrypt.Net.BCrypt.HashPassword(seed.Admin.Haslo),
            ApiToken = seed.Admin.ApiToken,
            CzyAdmin = true,
            GraczId = null
        });

        var gracze = seed.Gracze.Select(g => new Gracz
        {
            Imie = g.Imie,
            Nazwisko = g.Nazwisko,
            Kraj = g.Kraj,
            Reka = ParseEnum<Reka>(g.Reka),
            StylGry = ParseEnum<StylGry>(g.StylGry),
            DataUrodzenia = DateTime.Parse(g.DataUrodzenia)
        }).ToList();

        db.Gracze.AddRange(gracze);

        var sezony = seed.Sezony.Select(s => new Sezon
        {
            Nazwa = s.Nazwa,
            DataRozpoczecia = DateTime.Parse(s.DataRozpoczecia),
            DataZakonczenia = DateTime.Parse(s.DataZakonczenia),
            CzyAktywny = s.CzyAktywny
        }).ToList();

        db.Sezony.AddRange(sezony);
        db.SaveChanges();

        foreach (var su in seed.Uzytkownicy)
        {
            db.Uzytkownicy.Add(new Uzytkownik
            {
                Login = su.Login,
                HasloHash = BCrypt.Net.BCrypt.HashPassword(su.Haslo),
                ApiToken = su.ApiToken,
                CzyAdmin = false,
                GraczId = gracze[su.GraczIndex].Id
            });
        }

        db.SaveChanges();

        foreach (var sm in seed.Mecze)
        {
            var mecz = new Mecz
            {
                SezonId = sezony[sm.SezonIndex].Id,
                Gracz1Id = gracze[sm.Gracz1Index].Id,
                Gracz2Id = gracze[sm.Gracz2Index].Id,
                ZwyciezcaId = sm.ZwyciezcaIndex.HasValue ? gracze[sm.ZwyciezcaIndex.Value].Id : null,
                DataMeczu = DateTime.Parse(sm.DataMeczu),
                Nawierzchnia = ParseEnum<Nawierzchnia>(sm.Nawierzchnia),
                Format = ParseEnum<Format>(sm.Format)
            };

            db.Mecze.Add(mecz);
            db.SaveChanges();

            foreach (var ss in sm.Sety)
            {
                db.Sety.Add(new Set
                {
                    MeczId = mecz.Id,
                    NumerSeta = ss.NumerSeta,
                    GemyGracz1 = ss.GemyGracz1,
                    GemyGracz2 = ss.GemyGracz2,
                    TiebreakGracz1 = ss.TiebreakGracz1,
                    TiebreakGracz2 = ss.TiebreakGracz2
                });
            }

            if (sm.Statystyki != null)
            {
                var st = sm.Statystyki;
                db.StatystykiMeczow.Add(new StatystykiMeczu
                {
                    MeczId = mecz.Id,
                    AsyGracz1 = st.AsyGracz1,
                    AsyGracz2 = st.AsyGracz2,
                    DoubleFaultsGracz1 = st.DoubleFaultsGracz1,
                    DoubleFaultsGracz2 = st.DoubleFaultsGracz2,
                    PierwszySerwisProcentGracz1 = st.PierwszySerwisProcentGracz1,
                    PierwszySerwisProcentGracz2 = st.PierwszySerwisProcentGracz2,
                    PktNa1SerGracz1 = st.PktNa1SerGracz1,
                    PktNa1SerGracz2 = st.PktNa1SerGracz2,
                    PktNa2SerGracz1 = st.PktNa2SerGracz1,
                    PktNa2SerGracz2 = st.PktNa2SerGracz2,
                    WinnersGracz1 = st.WinnersGracz1,
                    WinnersGracz2 = st.WinnersGracz2,
                    UnforcedErrorsGracz1 = st.UnforcedErrorsGracz1,
                    UnforcedErrorsGracz2 = st.UnforcedErrorsGracz2,
                    BreakPktWykorzystaneGracz1 = st.BreakPktWykorzystaneGracz1,
                    BreakPktWykorzystaneGracz2 = st.BreakPktWykorzystaneGracz2,
                    BreakPktOkazjeGracz1 = st.BreakPktOkazjeGracz1,
                    BreakPktOkazjeGracz2 = st.BreakPktOkazjeGracz2,
                    CzasMeczuMin = st.CzasMeczuMin,
                    Publicznosc = st.Publicznosc
                });
            }
        }

        db.SaveChanges();
    }

    private static T ParseEnum<T>(string value) where T : struct, Enum
    {
        return Enum.Parse<T>(value, ignoreCase: true);
    }
}
