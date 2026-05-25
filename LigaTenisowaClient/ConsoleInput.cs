static class ConsoleInput
{
    public static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(value))
                return value;
            Console.WriteLine("Pole nie może być puste.");
        }
    }

    public static string ReadOptional(string prompt, string current = null)
    {
        var hint = current != null ? $" [{current}]" : " (Enter = bez zmiany)";
        Console.Write($"{prompt}{hint}: ");
        var value = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(value) ? current : value;
    }

    public static int ReadInt(string prompt, int? min = null, int? max = null)
    {
        while (true)
        {
            Console.Write(prompt);
            var raw = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(raw))
            {
                Console.WriteLine("Podaj liczbę całkowitą.");
                continue;
            }
            if (!int.TryParse(raw, out var n))
            {
                Console.WriteLine("Nieprawidłowy format liczby.");
                continue;
            }
            if (min.HasValue && n < min.Value)
            {
                Console.WriteLine($"Wartość musi być >= {min.Value}.");
                continue;
            }
            if (max.HasValue && n > max.Value)
            {
                Console.WriteLine($"Wartość musi być <= {max.Value}.");
                continue;
            }
            return n;
        }
    }

    public static int? ReadOptionalInt(string prompt)
    {
        Console.Write($"{prompt} (Enter = pomiń): ");
        var raw = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(raw)) return null;
        return int.TryParse(raw, out var n) ? n : ReadInt(prompt);
    }

    public static DateTime ReadDate(string prompt, DateTime? defaultValue = null)
    {
        while (true)
        {
            var def = defaultValue ?? DateTime.Today;
            Console.Write($"{prompt} (RRRR-MM-DD, Enter = {def:yyyy-MM-dd}): ");
            var raw = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(raw))
                return def;
            if (DateTime.TryParse(raw, out var dt))
                return dt;
            Console.WriteLine("Nieprawidłowy format daty.");
        }
    }

    public static int ReadEnumChoice(string prompt, string[] labels)
    {
        for (int i = 0; i < labels.Length; i++)
            Console.WriteLine($"{i} = {labels[i]}");
        return ReadInt($"{prompt} (0-{labels.Length - 1}): ", 0, labels.Length - 1);
    }

    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} (t/n): ");
            var raw = Console.ReadLine()?.Trim().ToLower();
            if (raw == "t" || raw == "tak") return true;
            if (raw == "n" || raw == "nie") return false;
            Console.WriteLine("Odpowiedz t lub n.");
        }
    }
}
