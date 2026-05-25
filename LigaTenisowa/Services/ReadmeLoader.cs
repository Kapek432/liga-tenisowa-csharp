using Markdig;

public static class ReadmeLoader
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public static string Load()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "README.md"),
            Path.Combine(Directory.GetCurrentDirectory(), "README.md"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "README.md")
        };

        foreach (var path in candidates)
        {
            var full = Path.GetFullPath(path);
            if (File.Exists(full))
                return File.ReadAllText(full);
        }

        return "Nie znaleziono pliku README.md.";
    }

    public static string LoadAsHtml() => Markdown.ToHtml(Load(), Pipeline);
}
