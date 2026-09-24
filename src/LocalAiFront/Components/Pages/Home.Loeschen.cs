using Chat.Core.Modelle;
using Microsoft.JSInterop;

namespace LocalAiFront.Components.Pages;

public partial class Home
{
    private async Task LoescheUnterhaltungAsync()
    {
        if (_sendet || _laedt || _unterhaltungId == Guid.Empty)
        {
            return;
        }
        _laedt = true;
        bool geloescht = false;
        try
        {
            if (!await BestaetigeLoeschenAsync())
            {
                return;
            }
            _fehler = null;
            _status = "Unterhaltung wird gelöscht …";
            await ChatService.LoescheUnterhaltungAsync(_unterhaltungId, _lebenszyklus.Token);
            geloescht = true;
            EntferneGeloeschteAnsicht();
            await ZeigeVerbleibendeUnterhaltungAsync();
            _status = "Unterhaltung gelöscht";
        }
        catch (OperationCanceledException) when (_lebenszyklus.IsCancellationRequested)
        {
            // Nach Verlassen der Seite ist keine weitere Anzeige nötig.
        }
        catch (Exception ausnahme)
        {
            _fehler = LoeschFehlermeldung(geloescht, ausnahme);
            _status = "Störung";
        }
        finally
        {
            _laedt = false;
        }
    }

    private ValueTask<bool> BestaetigeLoeschenAsync()
    {
        string titel = _unterhaltungen.FirstOrDefault(u => u.Id == _unterhaltungId)?.Titel ?? "Ausgewählte Unterhaltung";
        return JS.InvokeAsync<bool>("confirm", _lebenszyklus.Token,
            $"Möchten Sie die Unterhaltung mit dem Titel «{titel}» inkl. allen Nachrichten und Antworten endgültig löschen?");
    }

    private void EntferneGeloeschteAnsicht()
    {
        _unterhaltungen = _unterhaltungen.Where(u => u.Id != _unterhaltungId).ToArray();
        _unterhaltungId = Guid.Empty;
        _eintraege.Clear();
        _eingabe = string.Empty;
    }

    private async Task ZeigeVerbleibendeUnterhaltungAsync()
    {
        _unterhaltungen = await ChatService.ListeUnterhaltungenAsync(_lebenszyklus.Token);
        if (_unterhaltungen.FirstOrDefault() is { } verbleibend)
        {
            Unterhaltung unterhaltung = await ChatService.OeffneUnterhaltungAsync(
                verbleibend.Id,
                _lebenszyklus.Token);
            ZeigeUnterhaltung(unterhaltung);
        }
    }

    private static string LoeschFehlermeldung(bool geloescht, Exception ausnahme) => (geloescht, ausnahme) switch
    {
        (true, _) => "Die Unterhaltung wurde gelöscht, die verbleibende Liste konnte nicht geladen werden. Bitte erneut laden.",
        (false, InvalidOperationException) => "Die Unterhaltung konnte nicht gelöscht werden. Bitte eine aktive Antwort zuerst abbrechen oder warten und erneut versuchen.",
        _ => "Die Unterhaltung konnte nicht gelöscht werden. Bitte erneut versuchen."
    };
}
