using System.Diagnostics;
using Chat.Core.Modelle;

namespace Chat.Core;

internal sealed class AktiveAnfrage : IDisposable
{
    private readonly object _sperre = new();
    private readonly CancellationTokenSource _beobachtungBeenden = new();
    private bool _abgeschlossen;

    public AktiveAnfrage(
        Unterhaltung unterhaltung,
        Antwort antwort,
        int zeitlimitSekunden,
        CancellationToken lebenszyklus)
    {
        Unterhaltung = unterhaltung;
        Antwort = antwort;
        Lebenszyklus = lebenszyklus;
        BenutzerAbbruch = new CancellationTokenSource();
        Zeitlimit = new CancellationTokenSource(TimeSpan.FromSeconds(zeitlimitSekunden));
        Verknuepft = CancellationTokenSource.CreateLinkedTokenSource(
            lebenszyklus,
            BenutzerAbbruch.Token,
            Zeitlimit.Token);
        Beobachtung = CancellationTokenSource.CreateLinkedTokenSource(
            lebenszyklus,
            _beobachtungBeenden.Token);
        Laufzeit = Stopwatch.StartNew();
    }

    public Unterhaltung Unterhaltung { get; }

    public Antwort Antwort { get; }

    // Mutation und Speicherung teilen dieselbe Sperre, damit kein alter Teil den Abschluss überschreibt.
    public SemaphoreSlim Speichersperre { get; } = new(1, 1);

    public CancellationTokenSource BenutzerAbbruch { get; }

    public CancellationToken Lebenszyklus { get; }

    public CancellationTokenSource Zeitlimit { get; }

    public CancellationTokenSource Verknuepft { get; }

    public CancellationTokenSource Beobachtung { get; }

    public Stopwatch Laufzeit { get; }

    public CancellationToken Token => Verknuepft.Token;

    public CancellationToken BeobachtungToken => Beobachtung.Token;

    public bool IstAbgeschlossen
    {
        get
        {
            lock (_sperre)
            {
                return _abgeschlossen;
            }
        }
    }

    public bool VersucheTeilHinzuzufuegen(string teil)
    {
        lock (_sperre)
        {
            if (_abgeschlossen)
            {
                return false;
            }

            if (Antwort.Zustand == AntwortZustand.Angefordert)
            {
                Antwort.WechsleZu(AntwortZustand.Laeuft);
            }

            Antwort.FuegeTeilHinzu(teil);
            return true;
        }
    }

    public bool VersucheAbzuschliessen(
        AntwortZustand zustand,
        Stoerfall? fall,
        string? grund)
    {
        lock (_sperre)
        {
            if (_abgeschlossen)
            {
                return false;
            }

            Antwort.WechsleZu(zustand, Laufzeit.Elapsed, fall, grund);
            _abgeschlossen = true;
            return true;
        }
    }

    public void Dispose()
    {
        Laufzeit.Stop();
        _beobachtungBeenden.Cancel();
        Beobachtung.Dispose();
        Verknuepft.Dispose();
        Zeitlimit.Dispose();
        BenutzerAbbruch.Dispose();
        _beobachtungBeenden.Dispose();
    }
}
