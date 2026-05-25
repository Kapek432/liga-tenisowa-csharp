using System.Text;
using System.Text.Json;

// Demo REST API - najpierw należy uruchomić: dotnet run --project LigaTenisowa
// Domyślne dane z seed.json
const string baseUrl = "http://localhost:5118";
const string login = "admin";
const string apiToken = "liga-tenisowa-admin-token-demo";

var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
client.DefaultRequestHeaders.Add("X-Username", login);
client.DefaultRequestHeaders.Add("X-Api-Token", apiToken);

Console.WriteLine("=== Liga Tenisowa - demo REST API ===\n");

await RunStep("GET /api/gracze", () => client.GetAsync("/api/gracze"));
await RunStep("GET /api/sezony", () => client.GetAsync("/api/sezony"));
await RunStep("GET /api/ranking", () => client.GetAsync("/api/ranking"));
await RunStep("GET /api/ranking/alltime", () => client.GetAsync("/api/ranking/alltime"));
await RunStep("GET /api/mecze", () => client.GetAsync("/api/mecze"));
await RunStep("GET /api/mecze/1", () => client.GetAsync("/api/mecze/1"));

var postMecz = new
{
    gracz1Id = 5,
    gracz2Id = 6,
    sezonId = 1,
    nawierzchnia = 0,
    format = 0,
    dataMeczu = DateTime.Today.AddDays(14)
};
await RunStep("POST /api/mecze (zaplanowany Hurkacz vs inny gracz)", async () =>
{
    var json = JsonSerializer.Serialize(postMecz);
    return await client.PostAsync("/api/mecze",
        new StringContent(json, Encoding.UTF8, "application/json"));
});

Console.WriteLine("\n--- Test autoryzacji (zły token) ---");
var badClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
badClient.DefaultRequestHeaders.Add("X-Username", login);
badClient.DefaultRequestHeaders.Add("X-Api-Token", "zly-token");
await RunStep("GET /api/gracze (401)", () => badClient.GetAsync("/api/gracze"));

Console.WriteLine("\nDemo zakończone.");

static async Task RunStep(string opis, Func<Task<HttpResponseMessage>> request)
{
    Console.WriteLine($">> {opis}");
    try
    {
        var response = await request();
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
        var preview = body.Length > 400 ? body[..400] + "..." : body;
        if (!string.IsNullOrWhiteSpace(preview))
            Console.WriteLine($"Body: {preview}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Błąd: {ex.Message}");
        Console.WriteLine("Upewnij się, że aplikacja LigaTenisowa działa na http://localhost:5118");
    }
    Console.WriteLine();
}
