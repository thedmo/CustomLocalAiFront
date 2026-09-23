namespace Chat.Core;

public sealed class Konfiguration
{
    public string AdresseModellserver { get; init; } = string.Empty;

    public string Modellname { get; init; } = string.Empty;

    public string Systemanweisung { get; init; } = string.Empty;

    public double Temperatur { get; init; } = 0.2;

    public int MaximaleAntwortlaenge { get; init; } = 1024;

    public int Eingabegrenze { get; init; } = 4000;

    public int ZeitlimitSekunden { get; init; } = 60;

    public string SpeicherortDb { get; init; } = string.Empty;

    public string Protokolldatei { get; init; } = string.Empty;

    public void Pruefen()
    {
        PruefeText(AdresseModellserver, nameof(AdresseModellserver));

        if (!Uri.TryCreate(AdresseModellserver, UriKind.Absolute, out _))
        {
            WirfUngueltig(nameof(AdresseModellserver));
        }

        PruefeText(Modellname, nameof(Modellname));

        if (Temperatur is < 0 or > 2)
        {
            WirfUngueltig(nameof(Temperatur));
        }

        if (MaximaleAntwortlaenge <= 0)
        {
            WirfUngueltig(nameof(MaximaleAntwortlaenge));
        }

        if (Eingabegrenze <= 0)
        {
            WirfUngueltig(nameof(Eingabegrenze));
        }

        if (ZeitlimitSekunden <= 0)
        {
            WirfUngueltig(nameof(ZeitlimitSekunden));
        }
    }

    private static void PruefeText(string wert, string name)
    {
        if (string.IsNullOrWhiteSpace(wert))
        {
            WirfUngueltig(name);
        }
    }

    private static void WirfUngueltig(string name)
    {
        throw new StoerfallException(
            Stoerfall.KonfigurationUngueltig,
            $"Konfigurationswert {name} ist ungültig.");
    }
}
