using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

static class ApiHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task<(bool Ok, HttpResponseMessage Response, string Body)> SendAsync(
        HttpClient client, HttpMethod method, string url, object body = null)
    {
        try
        {
            HttpResponseMessage response;
            if (body == null)
            {
                response = method.Method switch
                {
                    "GET" => await client.GetAsync(url),
                    "DELETE" => await client.DeleteAsync(url),
                    _ => await client.SendAsync(new HttpRequestMessage(method, url))
                };
            }
            else
            {
                var json = JsonSerializer.Serialize(body, JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                response = method.Method switch
                {
                    "POST" => await client.PostAsync(url, content),
                    "PUT" => await client.PutAsync(url, content),
                    _ => await client.SendAsync(new HttpRequestMessage(method, url) { Content = content })
                };
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, response, responseBody);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Błąd połączenia: {ex.Message}");
            return (false, null, "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
            return (false, null, "");
        }
    }

    public static void PrintHttpError(HttpResponseMessage response, string body)
    {
        if (response == null) return;
        Console.WriteLine($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
        if (!string.IsNullOrWhiteSpace(body))
            Console.WriteLine(body);
    }

    public static string FormatDate(JsonElement el)
    {
        if (el.ValueKind == JsonValueKind.String &&
            DateTime.TryParse(el.GetString(), out var dt))
            return dt.ToString("dd.MM.yyyy");
        return el.ToString();
    }

    public static string GetString(JsonElement el, string prop, string fallback = "-")
    {
        if (!el.TryGetProperty(prop, out var p)) return fallback;
        return p.ValueKind == JsonValueKind.Null ? fallback : p.ToString();
    }

    public static void PrintSeparator(int length = 100) =>
        Console.WriteLine(new string('-', length));

    public static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Length <= maxLength ? value : value[..(maxLength - 1)] + "...";
    }
}
