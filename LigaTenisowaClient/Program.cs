using System.Text.Json;

const string baseUrl = "http://localhost:5118";
var client = new HttpClient { BaseAddress = new Uri(baseUrl) };

Console.Write("Login: ");
var login = Console.ReadLine()?.Trim() ?? "";
Console.Write("API Token: ");
var token = Console.ReadLine()?.Trim() ?? "";

client.DefaultRequestHeaders.Add("X-Username", login);
client.DefaultRequestHeaders.Add("X-Api-Token", token);

while (true)
{
    Console.WriteLine("\n=== Liga Tenisowa API Client ===");
    Console.WriteLine("1. Lista graczy");
    Console.WriteLine("2. Szczegóły gracza");
    Console.WriteLine("3. Dodaj gracza");
    Console.WriteLine("4. Edytuj gracza");
    Console.WriteLine("5. Usuń gracza");
    Console.WriteLine("6. Lista meczów");
    Console.WriteLine("7. Szczegóły meczu");
    Console.WriteLine("8. Dodaj mecz");
    Console.WriteLine("9. Wpisz wynik meczu");
    Console.WriteLine("10. Usuń mecz");
    Console.WriteLine("11. Lista sezonów");
    Console.WriteLine("12. Ranking sezonu");
    Console.WriteLine("13. Ranking All-Time");
    Console.WriteLine("0. Wyjście");
    Console.Write("\nWybierz opcję: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1": await ListaGraczy(); break;
        case "2":
            var idG = ConsoleInput.ReadInt("ID gracza: ", 1);
            await SzczegolyGracza(idG); break;
        case "3": await DodajGracza(); break;
        case "4":
            var idE = ConsoleInput.ReadInt("ID gracza: ", 1);
            await EdytujGracza(idE); break;
        case "5":
            var idU = ConsoleInput.ReadInt("ID gracza: ", 1);
            await UsunGracza(idU); break;
        case "6": await ListaMeczow(); break;
        case "7":
            var idM = ConsoleInput.ReadInt("ID meczu: ", 1);
            await SzczegolyMeczu(idM); break;
        case "8": await DodajMecz(); break;
        case "9":
            var idW = ConsoleInput.ReadInt("ID meczu: ", 1);
            await WpiszWynik(idW); break;
        case "10":
            var idDel = ConsoleInput.ReadInt("ID meczu: ", 1);
            await UsunMecz(idDel); break;
        case "11": await ListaSezonow(); break;
        case "12": await RankingSezon(); break;
        case "13": await RankingAllTime(); break;
        case "0": return;
        default: Console.WriteLine("Nieznana opcja."); break;
    }
}

async Task ListaGraczy()
{
    var (ok, resp, body) = await ApiHelper.SendAsync(client, HttpMethod.Get, "/api/gracze");
    if (!ok) { ApiHelper.PrintHttpError(resp, body); return; }

    using var doc = JsonDocument.Parse(body);
    Console.WriteLine("\n--- Gracze ---");
    Console.WriteLine($"{"ID",-4} | {"Imię i nazwisko",-26} | {"Kraj",-14} | {"Punkty",8} | {"Forma (5)",-12}");
    ApiHelper.PrintSeparator();
    foreach (var g in doc.RootElement.EnumerateArray())
    {
        var name = $"{ApiHelper.GetString(g, "imie")} {ApiHelper.GetString(g, "nazwisko")}";
        Console.WriteLine(
            $"{ApiHelper.GetString(g, "id"),-4} | {ApiHelper.Truncate(name, 26),-26} | {ApiHelper.Truncate(ApiHelper.GetString(g, "kraj"), 14),-14} | {ApiHelper.GetString(g, "punkty"),8} | {ApiHelper.GetString(g, "forma", "-"),-12}");
    }
}

async Task SzczegolyGracza(int id)
{
    var (ok, resp, body) = await ApiHelper.SendAsync(client, HttpMethod.Get, $"/api/gracze/{id}");
    if (!ok) { ApiHelper.PrintHttpError(resp, body); return; }

    var g = JsonDocument.Parse(body).RootElement;
    Console.WriteLine($"\n--- Gracz #{id} ---");
    Console.WriteLine($"Imię: {ApiHelper.GetString(g, "imie")}");
    Console.WriteLine($"Nazwisko: {ApiHelper.GetString(g, "nazwisko")}");
    Console.WriteLine($"Kraj: {ApiHelper.GetString(g, "kraj")}");
    Console.WriteLine($"Data urodzenia: {ApiHelper.FormatDate(g.GetProperty("dataUrodzenia"))}");
    Console.WriteLine($"Ręka: {ApiHelper.GetString(g, "reka")}");
    Console.WriteLine($"Styl gry: {ApiHelper.GetString(g, "stylGry")}");
    Console.WriteLine($"W/L: {ApiHelper.GetString(g, "wygrane")}/{ApiHelper.GetString(g, "przegrane")} | Punkty: {ApiHelper.GetString(g, "punkty")}");
    Console.WriteLine($"Forma: {ApiHelper.GetString(g, "forma", "-")}");
}

async Task DodajGracza()
{
    var imie = ConsoleInput.ReadRequired("Imię: ");
    var nazwisko = ConsoleInput.ReadRequired("Nazwisko: ");
    var kraj = ConsoleInput.ReadRequired("Kraj: ");
    var data = ConsoleInput.ReadDate("Data urodzenia");
    var reka = ConsoleInput.ReadEnumChoice("Ręka", new[] { "Prawa", "Lewa" });
    var styl = ConsoleInput.ReadEnumChoice("Styl gry", new[] { "Allcourt", "Baseliner", "ServeAndVolley" });

    var body = new
    {
        imie,
        nazwisko,
        kraj,
        reka,
        stylGry = styl,
        dataUrodzenia = data.ToString("yyyy-MM-dd")
    };

    var (ok, resp, responseBody) = await ApiHelper.SendAsync(client, HttpMethod.Post, "/api/gracze", body);
    Console.WriteLine(ok ? "Gracz dodany pomyślnie." : "Błąd dodawania gracza.");
    if (!ok) ApiHelper.PrintHttpError(resp, responseBody);
}

async Task EdytujGracza(int id)
{
    var (getOk, getResp, getBody) = await ApiHelper.SendAsync(client, HttpMethod.Get, $"/api/gracze/{id}");
    if (!getOk) { ApiHelper.PrintHttpError(getResp, getBody); return; }

    var current = JsonDocument.Parse(getBody).RootElement;
    var imie = ConsoleInput.ReadOptional("Nowe imię", ApiHelper.GetString(current, "imie"));
    var nazwisko = ConsoleInput.ReadOptional("Nowe nazwisko", ApiHelper.GetString(current, "nazwisko"));
    var kraj = ConsoleInput.ReadOptional("Nowy kraj", ApiHelper.GetString(current, "kraj"));

    Console.WriteLine("Ręka (0=Prawa, 1=Lewa | Enter = bez zmiany):");
    Console.WriteLine($"  Obecna: {ApiHelper.GetString(current, "reka")}");
    var rekaStr = Console.ReadLine()?.Trim();
    int reka = rekaStr == "" ? ParseReka(ApiHelper.GetString(current, "reka")) :
        ConsoleInput.ReadEnumChoice("Wybierz rękę", new[] { "Prawa", "Lewa" });

    Console.WriteLine("Styl gry  (0=Allcourt, 1=Baseliner, 2=ServeAndVolley | Enter = bez zmiany):");
    Console.WriteLine($"  Obecny: {ApiHelper.GetString(current, "stylGry")}");
    var stylStr = Console.ReadLine()?.Trim();
    int styl = stylStr == "" ? ParseStyl(ApiHelper.GetString(current, "stylGry")) :
        ConsoleInput.ReadEnumChoice("Wybierz styl", new[] { "Allcourt", "Baseliner", "ServeAndVolley" });

    var dataRaw = ApiHelper.GetString(current, "dataUrodzenia");
    DateTime.TryParse(dataRaw, out var curDate);
    var data = ConsoleInput.ReadDate("Data urodzenia", curDate);

    var body = new { imie, nazwisko, kraj, reka, stylGry = styl, dataUrodzenia = data.ToString("yyyy-MM-dd") };
    var (ok, resp, responseBody) = await ApiHelper.SendAsync(client, HttpMethod.Put, $"/api/gracze/{id}", body);
    Console.WriteLine(ok ? "Gracz zaktualizowany." : "Błąd edycji gracza.");
    if (!ok) ApiHelper.PrintHttpError(resp, responseBody);
}

int ParseReka(string label) => label.Contains("Lew", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
int ParseStyl(string label)
{
    if (label.Contains("Basel", StringComparison.OrdinalIgnoreCase)) return 1;
    if (label.Contains("Serve", StringComparison.OrdinalIgnoreCase)) return 2;
    return 0;
}

async Task UsunGracza(int id)
{
    if (!ConsoleInput.ReadYesNo($"Czy na pewno usunąć gracza #{id}?")) return;
    var (ok, resp, body) = await ApiHelper.SendAsync(client, HttpMethod.Delete, $"/api/gracze/{id}");
    Console.WriteLine(ok ? "Gracz usunięty." : "Błąd usuwania gracza.");
    if (!ok) ApiHelper.PrintHttpError(resp, body);
}

async Task ListaMeczow()
{
    var (ok, resp, body) = await ApiHelper.SendAsync(client, HttpMethod.Get, "/api/mecze");
    if (!ok) { ApiHelper.PrintHttpError(resp, body); return; }

    Console.WriteLine("\n--- Mecze ---");
    Console.WriteLine($"{"ID",-4} | {"Data",-10} | {"Gracz 1",-18} | {"Gracz 2",-18} | {"Nawierzchnia",-12} | {"Zwycięzca",-18}");
    ApiHelper.PrintSeparator();
    foreach (var m in JsonDocument.Parse(body).RootElement.EnumerateArray())
    {
        var zw = m.TryGetProperty("zwyciezca", out var z) && z.ValueKind != JsonValueKind.Null
            ? z.GetString() : "nierozegrany";
        Console.WriteLine(
            $"{ApiHelper.GetString(m, "id"),-4} | {ApiHelper.FormatDate(m.GetProperty("dataMeczu")),-10} | " +
            $"{ApiHelper.Truncate(ApiHelper.GetString(m, "gracz1"), 18),-18} | {ApiHelper.Truncate(ApiHelper.GetString(m, "gracz2"), 18),-18} | " +
            $"{ApiHelper.GetString(m, "nawierzchnia"),-12} | {ApiHelper.Truncate(zw ?? "nierozegrany", 18),-18}");
    }
}

async Task SzczegolyMeczu(int id)
{
    var (ok, resp, body) = await ApiHelper.SendAsync(client, HttpMethod.Get, $"/api/mecze/{id}");
    if (!ok) { ApiHelper.PrintHttpError(resp, body); return; }

    var m = JsonDocument.Parse(body).RootElement;
    Console.WriteLine($"\n--- Mecz #{id} ---");
    Console.WriteLine($"{ApiHelper.GetString(m, "gracz1")} vs {ApiHelper.GetString(m, "gracz2")}");
    Console.WriteLine($"Data: {ApiHelper.FormatDate(m.GetProperty("dataMeczu"))}");
    Console.WriteLine($"Nawierzchnia: {ApiHelper.GetString(m, "nawierzchnia")} | Format: {ApiHelper.GetString(m, "format")}");
    Console.WriteLine($"Sezon: {ApiHelper.GetString(m, "sezon")}");
    Console.WriteLine($"Zwycięzca: {ApiHelper.GetString(m, "zwyciezca", "- nierozegrany")}");

    if (m.TryGetProperty("sety", out var sety) && sety.ValueKind == JsonValueKind.Array)
    {
        Console.WriteLine("Sety:");
        foreach (var s in sety.EnumerateArray())
        {
            var tb1 = s.TryGetProperty("tiebreakGracz1", out var t1) && t1.ValueKind != JsonValueKind.Null ? $" ({t1})" : "";
            var tb2 = s.TryGetProperty("tiebreakGracz2", out var t2) && t2.ValueKind != JsonValueKind.Null ? $" ({t2})" : "";
            Console.WriteLine($"  Set {ApiHelper.GetString(s, "numerSeta")}: " +
                $"{ApiHelper.GetString(s, "gemyGracz1")}{tb1}:{ApiHelper.GetString(s, "gemyGracz2")}{tb2}");
        }
    }

    if (m.TryGetProperty("statystyki", out var st) && st.ValueKind == JsonValueKind.Object)
    {
        Console.WriteLine("Statystyki: asy " + ApiHelper.GetString(st, "asyGracz1") + "/" + ApiHelper.GetString(st, "asyGracz2") +
            ", czas " + ApiHelper.GetString(st, "czasMeczuMin") + " min");
    }
}

async Task DodajMecz()
{
    await ListaSezonow();
    var sezonId = ConsoleInput.ReadInt("ID sezonu: ", 1);
    Console.WriteLine("Podaj ID graczy (lista powyżej w graczach):");
    await ListaGraczy();
    var g1 = ConsoleInput.ReadInt("ID gracza 1: ", 1);
    var g2 = ConsoleInput.ReadInt("ID gracza 2: ", 1);
    var data = ConsoleInput.ReadDate("Data meczu");
    var naw = ConsoleInput.ReadEnumChoice("Nawierzchnia", new[] { "Hard", "Clay", "Grass" });
    var format = ConsoleInput.ReadEnumChoice("Format", new[] { "Best of 3", "Best of 5" });

    var body = new
    {
        gracz1Id = g1,
        gracz2Id = g2,
        sezonId,
        nawierzchnia = naw,
        format,
        dataMeczu = data
    };

    var (ok, resp, responseBody) = await ApiHelper.SendAsync(client, HttpMethod.Post, "/api/mecze", body);
    Console.WriteLine(ok ? "Mecz dodany pomyślnie." : "Błąd dodawania meczu.");
    if (!ok) ApiHelper.PrintHttpError(resp, responseBody);
    else if (ConsoleInput.ReadYesNo("Czy od razu wpisać wynik?"))
    {
        if (responseBody.Contains("\"id\""))
        {
            var id = JsonDocument.Parse(responseBody).RootElement.GetProperty("id").GetInt32();
            await WpiszWynik(id);
        }
    }
}

async Task WpiszWynik(int id)
{
    var (getOk, getResp, getBody) = await ApiHelper.SendAsync(client, HttpMethod.Get, $"/api/mecze/{id}");
    if (!getOk) { ApiHelper.PrintHttpError(getResp, getBody); return; }

    Console.WriteLine(getBody);
    Console.WriteLine("Podaj ID zwycięzcy (musi być graczem 1 lub 2 tego meczu):");
    var zwyciezcaId = ConsoleInput.ReadInt("ID zwycięzcy: ", 1);

    var sety = new List<object>();
    var liczbaSetow = ConsoleInput.ReadInt("Ile setów wpisać (1-5): ", 1, 5);
    for (int i = 0; i < liczbaSetow; i++)
    {
        Console.WriteLine($"Set {i + 1}:");
        var g1 = ConsoleInput.ReadInt("  Gemy gracza 1: ", 0, 7);
        var g2 = ConsoleInput.ReadInt("  Gemy gracza 2: ", 0, 7);
        var tb1 = ConsoleInput.ReadOptionalInt("  Tiebreak gracza 1");
        var tb2 = ConsoleInput.ReadOptionalInt("  Tiebreak gracza 2");
        sety.Add(new
        {
            numerSeta = i + 1,
            gemyGracz1 = g1,
            gemyGracz2 = g2,
            tiebreakGracz1 = tb1,
            tiebreakGracz2 = tb2
        });
    }

    var statystyki = new
    {
        asyGracz1 = ConsoleInput.ReadInt("Asy gracza 1: ", 0),
        asyGracz2 = ConsoleInput.ReadInt("Asy gracza 2: ", 0),
        doubleFaultsGracz1 = ConsoleInput.ReadInt("Double faults G1: ", 0),
        doubleFaultsGracz2 = ConsoleInput.ReadInt("Double faults G2: ", 0),
        pierwszySerwisProcentGracz1 = (double)ConsoleInput.ReadInt("1. serwis % G1: ", 0, 100),
        pierwszySerwisProcentGracz2 = (double)ConsoleInput.ReadInt("1. serwis % G2: ", 0, 100),
        winnersGracz1 = ConsoleInput.ReadInt("Winners G1: ", 0),
        winnersGracz2 = ConsoleInput.ReadInt("Winners G2: ", 0),
        unforcedErrorsGracz1 = ConsoleInput.ReadInt("Unforced Errors G1: ", 0),
        unforcedErrorsGracz2 = ConsoleInput.ReadInt("Unforced Errors  G2: ", 0),
        breakPktWykorzystaneGracz1 = ConsoleInput.ReadInt("Break Pointy - wykorzystane G1: ", 0),
        breakPktWykorzystaneGracz2 = ConsoleInput.ReadInt("Break Pointy - wykorzystane G2: ", 0),
        breakPktOkazjeGracz1 = ConsoleInput.ReadInt("Break Pointy - okazje G1: ", 0),
        breakPktOkazjeGracz2 = ConsoleInput.ReadInt("Break Pointy - okazje G2: ", 0),
        czasMeczuMin = ConsoleInput.ReadInt("Czas meczu (min): ", 1),
        publicznosc = ConsoleInput.ReadOptionalInt("Publiczność")
    };

    var body = new { zwyciezcaId, sety, statystyki };
    var (ok, resp, responseBody) = await ApiHelper.SendAsync(client, HttpMethod.Put, $"/api/mecze/{id}/wynik", body);
    Console.WriteLine(ok ? "Wynik zapisany." : "Błąd zapisu wyniku.");
    if (!ok) ApiHelper.PrintHttpError(resp, responseBody);
}

async Task UsunMecz(int id)
{
    if (!ConsoleInput.ReadYesNo($"Czy na pewno usunąć mecz #{id}?")) return;
    var (ok, resp, body) = await ApiHelper.SendAsync(client, HttpMethod.Delete, $"/api/mecze/{id}");
    Console.WriteLine(ok ? "Mecz usunięty." : "Błąd usuwania meczu.");
    if (!ok) ApiHelper.PrintHttpError(resp, body);
}

async Task ListaSezonow()
{
    var (ok, resp, body) = await ApiHelper.SendAsync(client, HttpMethod.Get, "/api/sezony");
    if (!ok) { ApiHelper.PrintHttpError(resp, body); return; }

    Console.WriteLine("\n--- Sezony ---");
    Console.WriteLine($"{"ID",-4} | {"Nazwa",-28} | {"Mecze",8} | {"Status",-10}");
    ApiHelper.PrintSeparator();
    foreach (var s in JsonDocument.Parse(body).RootElement.EnumerateArray())
    {
        var aktywny = s.TryGetProperty("czyAktywny", out var a) && a.GetBoolean() ? "aktywny" : "-";
        Console.WriteLine(
            $"{ApiHelper.GetString(s, "id"),-4} | {ApiHelper.Truncate(ApiHelper.GetString(s, "nazwa"), 28),-28} | {ApiHelper.GetString(s, "liczbaMeczow"),8} | {aktywny,-10}");
    }
}

async Task RankingSezon()
{
    var (ok, resp, body) = await ApiHelper.SendAsync(client, HttpMethod.Get, "/api/ranking");
    if (!ok) { ApiHelper.PrintHttpError(resp, body); return; }

    var doc = JsonDocument.Parse(body).RootElement;
    Console.WriteLine($"\n--- Ranking sezonu: {ApiHelper.GetString(doc, "sezon")} ---");
    Console.WriteLine($"{"#",-4} | {"Gracz",-26} | {"Punkty",8} | {"W",4} | {"L",4} | {"Forma (5)",-12}");
    ApiHelper.PrintSeparator();
    int i = 1;
    foreach (var r in doc.GetProperty("ranking").EnumerateArray())
    {
        var name = $"{ApiHelper.GetString(r, "imie")} {ApiHelper.GetString(r, "nazwisko")}";
        Console.WriteLine(
            $"{i++,-4} | {ApiHelper.Truncate(name, 26),-26} | {ApiHelper.GetString(r, "punkty"),8} | {ApiHelper.GetString(r, "wygrane"),4} | {ApiHelper.GetString(r, "przegrane"),4} | {ApiHelper.GetString(r, "forma", "-"),-12}");
    }
}

async Task RankingAllTime()
{
    var (ok, resp, body) = await ApiHelper.SendAsync(client, HttpMethod.Get, "/api/ranking/alltime");
    if (!ok) { ApiHelper.PrintHttpError(resp, body); return; }

    Console.WriteLine("\n--- Ranking All-Time ---");
    Console.WriteLine($"{"#",-4} | {"Gracz",-26} | {"Punkty",8} | {"W",4} | {"L",4}");
    ApiHelper.PrintSeparator();
    int i = 1;
    foreach (var r in JsonDocument.Parse(body).RootElement.EnumerateArray())
    {
        var name = $"{ApiHelper.GetString(r, "imie")} {ApiHelper.GetString(r, "nazwisko")}";
        Console.WriteLine(
            $"{i++,-4} | {ApiHelper.Truncate(name, 26),-26} | {ApiHelper.GetString(r, "punkty"),8} | {ApiHelper.GetString(r, "wygrane"),4} | {ApiHelper.GetString(r, "przegrane"),4}");
    }
}
