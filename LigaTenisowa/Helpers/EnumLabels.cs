public static class EnumLabels
{
    public static string GetReka(Reka reka) => reka switch
    {
        Reka.Prawa => "Prawa",
        Reka.Lewa => "Lewa",
        _ => reka.ToString()
    };

    public static string GetStylGry(StylGry styl) => styl switch
    {
        StylGry.Allcourt => "Allcourt",
        StylGry.Baseliner => "Baseliner",
        StylGry.ServeAndVolley => "Serve and Volley",
        _ => styl.ToString()
    };

    public static string GetNawierzchnia(Nawierzchnia n) => n switch
    {
        Nawierzchnia.Hard => "Hard",
        Nawierzchnia.Clay => "Clay",
        Nawierzchnia.Grass => "Grass",
        _ => n.ToString()
    };

    public static string GetFormat(Format f) => f switch
    {
        Format.Bo3 => "Best of 3",
        Format.Bo5 => "Best of 5",
        _ => f.ToString()
    };
}
