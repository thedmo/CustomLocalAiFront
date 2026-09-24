using Chat.Core.Modelle;

namespace LocalAiFront.Components.Pages;

public partial class Home
{
    private async Task LadeUnterhaltungAsync(Guid id)
    {
        if (_sendet || _laedt)
        {
            return;
        }

        _laedt = true;
        _fehler = null;
        _status = "Unterhaltung wird geladen …";
        try
        {
            Unterhaltung unterhaltung = await ChatService.OeffneUnterhaltungAsync(id, _lebenszyklus.Token);
            _unterhaltungen = await ChatService.ListeUnterhaltungenAsync(_lebenszyklus.Token);
            ZeigeUnterhaltung(unterhaltung);
            SchliesseSeitenleiste();
            _status = "Bereit";
        }
        catch (OperationCanceledException) when (_lebenszyklus.IsCancellationRequested)
        {
            // Die verlassene Seite benötigt kein Ladeergebnis mehr.
        }
        catch (KeyNotFoundException)
        {
            _fehler = "Die Unterhaltung ist nicht mehr vorhanden.";
            _status = "Störung";
        }
        catch (Exception)
        {
            _fehler = "Die Unterhaltung konnte nicht geladen werden. Bitte erneut versuchen.";
            _status = "Störung";
        }
        finally
        {
            _laedt = false;
        }
    }

    private void ZeigeUnterhaltung(Unterhaltung unterhaltung)
    {
        _eintraege.Clear();
        foreach (Nachricht nachricht in unterhaltung.Nachrichten)
        {
            _eintraege.Add(new ChatEintrag(true, nachricht.Text));
            _eintraege.Add(new ChatEintrag(false, nachricht.Antwort.Text)
            {
                Zustandsmeldung = nachricht.Antwort.Zustand switch
                {
                    AntwortZustand.Abgebrochen => "Antwort abgebrochen",
                    AntwortZustand.Gestoert => $"Antwort gestört: {nachricht.Antwort.Grund}",
                    AntwortZustand.Fertig => "Antwort fertig",
                    AntwortZustand.Laeuft => "Antwort läuft",
                    _ => "Antwort angefordert"
                }
            });
        }
        _unterhaltungId = unterhaltung.Id;
        _eingabe = string.Empty;
        _letzteNachrichtenAnzahl = -1;
    }

    private async Task AktualisiereListeAsync()
    {
        try
        {
            _unterhaltungen = await ChatService.ListeUnterhaltungenAsync(_lebenszyklus.Token);
        }
        catch (OperationCanceledException) when (_lebenszyklus.IsCancellationRequested)
        {
            // Die verlassene Seite benötigt keine aktualisierte Liste mehr.
        }
        catch (Exception)
        {
            _fehler = "Die Unterhaltungsliste konnte nicht aktualisiert werden.";
        }
    }
}
