using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Chat.Core;

internal sealed class UnterhaltungsZugriff
{
    // Alle Circuits am selben Singleton-Store teilen Start-/Löschsperre und aktive Anfragen.
    private static readonly ConditionalWeakTable<IStore, UnterhaltungsZugriff> _zugriffe = new();
    private readonly SemaphoreSlim _sperre = new(1, 1);
    public ConcurrentDictionary<Guid, AktiveAnfrage> Anfragen { get; } = new();
    public ConcurrentDictionary<Guid, byte> Entwuerfe { get; } = new();

    public static UnterhaltungsZugriff FuerStore(IStore store) =>
        _zugriffe.GetValue(store, _ => new UnterhaltungsZugriff());

    public async Task<AntwortLauf> StartenAsync(Func<Task<AntwortLauf>> starten, CancellationToken ct)
    {
        // Laden, erstes Speichern und Registrierung müssen vor einem Löschen abgeschlossen sein.
        await _sperre.WaitAsync(ct);
        try
        {
            return await starten();
        }
        finally
        {
            _sperre.Release();
        }
    }

    public async Task LoeschenAsync(IStore store, Guid id, CancellationToken ct)
    {
        await _sperre.WaitAsync(ct);
        try
        {
            // Ein Eintrag bleibt bis zum gespeicherten Abschluss bestehen, auch beim Abbruch.
            if (Anfragen.Values.Any(a => a.Unterhaltung.Id == id))
            {
                throw new InvalidOperationException("Die Unterhaltung hat noch eine aktive Antwort. Bitte zuerst abbrechen oder warten.");
            }
            if (Entwuerfe.TryRemove(id, out _))
            {
                return;
            }
            await store.LoeschenAsync(id, ct);
        }
        finally
        {
            _sperre.Release();
        }
    }
}
