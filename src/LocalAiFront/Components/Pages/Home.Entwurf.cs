namespace LocalAiFront.Components.Pages;

public partial class Home
{
    private async Task NeueUnterhaltungAsync()
    {
        if (_sendet || _laedt)
        {
            return;
        }

        _laedt = true;
        _fehler = null;
        try
        {
            _unterhaltungId = Guid.Empty;
            _eintraege.Clear();
            _eingabe = string.Empty;
            _unterhaltungen = await ChatService.ListeUnterhaltungenAsync(_lebenszyklus.Token);
            _status = "Bereit";
        }
        catch (OperationCanceledException) when (_lebenszyklus.IsCancellationRequested)
        {
            // Die verlassene Seite benötigt keine aktualisierte Liste mehr.
        }
        catch (Exception)
        {
            _fehler = "Die Unterhaltungsliste konnte nicht geladen werden.";
            _status = "Störung";
        }
        finally
        {
            _laedt = false;
        }
    }

    private async Task VerwirfEntwurfAsync(Guid id)
    {
        try
        {
            await ChatService.LoescheUnterhaltungAsync(id, CancellationToken.None);
            _unterhaltungId = Guid.Empty;
        }
        catch (Exception)
        {
            _fehler = "Der Entwurf konnte nach dem fehlgeschlagenen Senden nicht verworfen werden.";
            _status = "Störung";
        }
    }
}
