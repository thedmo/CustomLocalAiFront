namespace Chat.Core.Modelle;

public sealed class Antwort
{
    private static readonly HashSet<(AntwortZustand Von, AntwortZustand Nach)> _erlaubteUebergaenge =
    [
        (AntwortZustand.Angefordert, AntwortZustand.Laeuft),
        (AntwortZustand.Angefordert, AntwortZustand.Abgebrochen),
        (AntwortZustand.Angefordert, AntwortZustand.Gestoert),
        (AntwortZustand.Laeuft, AntwortZustand.Fertig),
        (AntwortZustand.Laeuft, AntwortZustand.Abgebrochen),
        (AntwortZustand.Laeuft, AntwortZustand.Gestoert)
    ];

    public Antwort(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }

    public string Text { get; private set; } = string.Empty;

    public AntwortZustand Zustand { get; private set; } = AntwortZustand.Angefordert;

    public TimeSpan? Dauer { get; private set; }

    public Stoerfall? Fall { get; private set; }

    public string? Grund { get; private set; }

    /// <summary>Rekonstruiert einen gespeicherten Zustand ohne neue Zustandsübergänge.</summary>
    public static Antwort Wiederherstellen(
        Guid id, string text, AntwortZustand zustand,
        TimeSpan? dauer, Stoerfall? fall, string? grund)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (!Enum.IsDefined(zustand))
        {
            throw new ArgumentOutOfRangeException(nameof(zustand));
        }

        return new Antwort(id)
        {
            Text = text,
            Zustand = zustand,
            Dauer = dauer,
            Fall = fall,
            Grund = grund
        };
    }

    public void FuegeTeilHinzu(string teil)
    {
        ArgumentException.ThrowIfNullOrEmpty(teil);

        if (Zustand != AntwortZustand.Laeuft)
        {
            throw new InvalidOperationException("Antwortteile dürfen nur im Zustand Läuft ergänzt werden.");
        }

        Text += teil;
    }

    public void WechsleZu(
        AntwortZustand neuerZustand,
        TimeSpan? dauer = null,
        Stoerfall? fall = null,
        string? grund = null)
    {
        if (!_erlaubteUebergaenge.Contains((Zustand, neuerZustand)))
        {
            throw new InvalidOperationException(
                $"Zustandsübergang von {Zustand} nach {neuerZustand} ist nicht erlaubt.");
        }

        Zustand = neuerZustand;
        Dauer = dauer;
        Fall = fall;
        Grund = grund;
    }
}
