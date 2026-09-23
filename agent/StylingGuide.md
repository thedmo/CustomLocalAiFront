# StylingGuide.md: Codekonventionen für C#, Blazor, CSS, Tests und Git

Gilt für KI-Code und Handcode gleich (Kursunterlagen S. 18). Wo dieses Dokument schweigt, gelten die C# Coding Conventions von Microsoft und `dotnet format` mit der `.editorconfig` im Repository. Ändern darf diese Datei nur die Gruppe, mit Eintrag im Reviewprotokoll.

## 1. Sprache und Namen

- Fachbegriffe deutsch, exakt wie im Glossar des Konzepts: Unterhaltung, Nachricht, Antwort, AntwortZustand, Störfall, Protokolleintrag. Technische Begriffe englisch: Service, Client, Store, Repository, Options.
- Keine Umlaute und kein ss in Bezeichnern: `Loeschen`, `Stoerfall`, `Laeuft`, `Zeitueberschreitung`. In Kommentaren, Meldungen und Texten für Benutzer normale Schreibweise mit Umlauten und ss.
- Typen, Methoden, Eigenschaften: PascalCase. Lokale Variablen, Parameter: camelCase. Private Felder: `_camelCase`. Konstanten: PascalCase. Schnittstellen: `I` vorne (`IChatService`).
- Asynchrone Methoden enden auf `Async` (`SendeNachrichtAsync`). Im Konzept stehen die Namen ohne Suffix; das ist dieselbe Methode.
- Eine Klasse je Datei, Dateiname gleich Klassenname. Namensraum gleich Ordner (`Chat.Core.Modelle`).
- Namen sagen, was die Sache ist. Keine Abkürzungen ausser `Id`, `Db`, `Http`, `Ct` für den CancellationToken-Parameter.

## 2. Struktur und Schichten (Konzept 7.1)

| Projekt | Inhalt | Darf kennen |
|---|---|---|
| `Chat.Web` | Blazor-Seite und Komponenten, CSS, `Program.cs` mit der Verdrahtung | `Chat.Core` |
| `Chat.Core` | `IChatService`, `ChatService`, Modelle, `IModelServerClient`, `IStore`, `Konfiguration`, `Stoerfall` | nichts aus dem Repository |
| `Chat.Adapter.ModelRunner` | `ModelRunnerClient`, einziger Ort mit Kenntnis der API des Docker Model Runner | `Chat.Core` |
| `Chat.Store.Sqlite` | `SqliteStore`, `ChatDbContext`, Migrationen, Protokollschreiber | `Chat.Core` |
| `Chat.Tests` | xUnit-Tests, Fake-Adapter, Speicher im Arbeitsspeicher | alle |

Regel: Abhängigkeiten nur nach unten. `Chat.Web` referenziert nie Adapter oder Store; die Verdrahtung passiert in `Program.cs` per Dependency Injection (`AddScoped<IChatService, ChatService>()`, `AddSingleton<IModelServerClient, ModelRunnerClient>()`).

## 3. Formatierung

- `dotnet format` vor jedem Commit. `.editorconfig` ist massgeblich: vier Leerzeichen, UTF-8, LF, file-scoped namespaces, `var` nur, wenn der Typ rechts sichtbar ist.
- Methoden unter 40 Zeilen, Dateien unter 300 Zeilen; sonst teilen.
- Kein auskommentierter Code. `// TODO` nur mit Kartennummer: `// TODO AP09: Kaskade beim Loeschen`.
- Ein Ausdruck je Zeile; keine verschachtelten ternären Ausdrücke.

## 4. Kommentare und Header

- Ein Kommentar erklärt das Warum. Das Was sagt der Name; wenn nicht, ist der Name falsch.
- XML-Dokumentation (`///`) nur für öffentliche Schnittstellen in `Chat.Core`, ein Satz je Methode.
- Kein Dateikopf mit Autor, Datum, Copyright. Das liefert Git.
- Deutsch, ganze Sätze, keine Emojis.

## 5. Fehlerbehandlung und Störfälle (Konzept 7.3, M09)

- Genau vier Störfälle als Enum `Stoerfall`: `ModellserverNichtErreichbar`, `ModellUnbekannt`, `KonfigurationUngueltig`, `Zeitueberschreitung`. Eine Ausnahme `StoerfallException(Stoerfall fall, string grund)`.
- Behandelt wird an Grenzen: HTTP zum Model Runner im Adapter, Datei und Datenbank im Store, Konfiguration beim Start. Innerhalb einer Schicht werden Ausnahmen nicht abgefangen, nur weitergereicht.
- Kein leeres `catch`, kein `catch (Exception)` ohne Weitergabe oder Protokoll. Ein Fehler ist laut: Protokolleintrag mit Grund, Meldung an die Seite, Zustand `Gestoert`.
- Abbruch ist kein Fehler: `OperationCanceledException` führt zu Zustand `Abgebrochen`, der bereits empfangene Teil bleibt.
- Zeitlimit und Abbruch immer über `CancellationToken`, der bis zum `HttpClient` weitergereicht wird (Konzept 7.2). Kein `Thread.Sleep`, kein `.Result`, kein `.Wait()`.

## 6. Daten und Transaktionen (Konzept 9)

- Zugriff nur über `IStore`. Eine fachliche Operation ist eine Transaktion: Nachricht speichern und Antwort mit Zustand `Angefordert` anlegen zusammen (7.2 Schritt 3); Antwort abschliessen mit Zustand, Text und Dauer zusammen (Schritt 9).
- Kennungen sind `Guid`, erzeugt beim Anlegen. Löschen kaskadiert auf Nachrichten und Antworten; der Protokolleintrag bleibt (9.2).
- Kein Chattext und keine Systemanweisung im Protokoll (9.3). Pfade für Datenbank und Protokoll nur aus der Konfiguration.
- Migrationen liegen in `Chat.Store.Sqlite/Migrationen` und werden beim Start angewendet.

## 7. Konfiguration (Konzept 7.4)

- Alle Werte in `appsettings.json` unter dem Abschnitt `Chat`, überschreibbar per Umgebungsvariable (`Chat__Modellname`). Die Klasse `Konfiguration` wird beim Start validiert; ungültig heisst Störfall `KonfigurationUngueltig`, Meldung mit dem fehlenden Wert, Start bricht ab.
- `appsettings.example.json` ist die Vorlage mit Platzhaltern. Persönliche Pfade, Geheimnisse, Hostnamen anderer Geräte: nie im Repository.
- Werte werden gelesen, nie im Code verdrahtet. Eine Änderung wirkt nach Neustart des Containers ohne Neubau (Z02).

## 8. Razor, HTML und CSS (Konzept 8, Skript SE I Kapitel 7 Accessibility)

Razor-Komponenten:

- Komponenten enthalten Anzeige und Ereignisse, keine Fachlogik. Logik liegt im `ChatService`. Eine Komponente je Bereich: `Unterhaltungsliste`, `Verlauf`, `Eingabe`, `Statuszeile`; Dateiname gleich Komponentenname, `@code`-Block unter 60 Zeilen, sonst Logik in eine Klasse.
- Streaming: `await foreach` über `IAsyncEnumerable<string>`, nach jedem Teil `StateHasChanged()`. Abbruch über eine `CancellationTokenSource` der Seite; der Knopf heisst `Senden` oder `Abbrechen` je nach Zustand.
- Ereignisbehandler heissen nach der Absicht (`SendenAsync`, `AbbrechenAsync`), nicht nach dem Ereignis (`OnClick`).
- Kein eigenes JavaScript ausser dokumentiertem Sonderfall (zum Beispiel Scrollen zum Ende des Verlaufs); jeder Sonderfall steht in der Karte und in einer Datei `wwwroot/js/sonderfaelle.js`.

HTML:

- Semantische Elemente: `header` für Logo und Titel, `aside` für die Unterhaltungsliste, `main` für den Verlauf, `form` für die Eingabe, `button` für Knöpfe (nie `div` mit Klick). Überschriften in Reihenfolge `h1`, `h2`.
- Jede Eingabe hat ein `label`; das Eingabefeld ist ein `textarea` mit `maxlength` gleich der Eingabegrenze aus der Konfiguration.
- Tastatur: Enter sendet, Shift+Enter macht eine neue Zeile, Escape bricht ab; Fokusreihenfolge folgt der Leserichtung; sichtbarer Fokusrahmen.
- Der Streaming-Bereich und die Statuszeile tragen `aria-live="polite"`, damit Meldungen vorgelesen werden. Bilder und das Logo haben `alt`.
- Keine Inline-Styles, keine `style`-Attribute, keine Tabellen für Layout.

CSS:

- Eine Datei `wwwroot/css/hfu.css` mit den CSS-Variablen des Corporate Design (Farben, Schrift, Abstände, Radien) und dem Layout; Komponenten-Styles in `Komponente.razor.css` nur für das, was die Komponente allein betrifft.
- Keine Farb- oder Schriftwerte ausserhalb der Variablen; kein `!important`; Klassen nach dem Muster `bereich-element--zustand` in Kleinbuchstaben, Fachbegriffe deutsch: `verlauf-nachricht--antwort`.
- Kontrast mindestens 4,5 zu 1 für Text; Bedeutung nie nur durch Farbe (Zustand Gestört hat Text und Symbol).
- Zwei Fensterbreiten: unter 800 Pixel klappt die Unterhaltungsliste ein, der Verlauf füllt die Breite; Schriftgrössen in `rem`, keine festen Pixelhöhen für Text.
- Corporate Design ist ein Testfall in Konzept 11.3 (M07): Logo im Kopf, Farben und Schrift aus `_Logos`.

## 9. Tests (Konzept 11)

- xUnit in `Chat.Tests`. Name `Methode_Situation_Erwartung`: `SendeNachricht_LeererText_WirftStoerfall`.
- `ChatService` wird mit Fake-Adapter und Speicher im Arbeitsspeicher getestet. Der echte Model Runner nur in Integrationstests mit Attribut `[Trait("Art", "Integration")]`, die nicht im Standardlauf laufen.
- Jeder Störfall und jeder Pfeil des Zustandsdiagramms (Konzept 4.7.6) hat einen Test. Das erwartete Ergebnis steht vor dem Lauf fest.
- Kein Test hängt von Reihenfolge, Uhrzeit oder Netz ab.

## 10. Git

- Eine Karte, ein Branch (`ap07-backend-senden`), Zusammenführung nach Review; oder kleine Commits direkt auf `main`. Die Gruppe entscheidet in AP06 und trägt es hier ein.
- Commit-Nachricht: Kartennummer plus Aussage im Präsens: `AP08: Abbrechen-Knopf loest CancellationToken aus`. Bei KI-Beteiligung zweite Zeile `KI: Werkzeug, geprueft von XX`.
- Nur eigene Pfade stagen (`git add src/Chat.Core`), nie `git add -A`. Nie committen: `bin/`, `obj/`, `data/`, `*.db`, `appsettings.json` mit echten Werten (siehe `.gitignore`).

## 11. Muster-Code

Störfall und Ausnahme (`Chat.Core/Stoerfall.cs`):

```csharp
namespace Chat.Core;

public enum Stoerfall { ModellserverNichtErreichbar, ModellUnbekannt, KonfigurationUngueltig, Zeitueberschreitung }

public sealed class StoerfallException(Stoerfall fall, string grund) : Exception(grund)
{
    public Stoerfall Fall { get; } = fall;
}
```

Schnittstelle zur Seite (`Chat.Core/IChatService.cs`, Konzept 7.3):

```csharp
namespace Chat.Core;

public interface IChatService
{
    Task<Guid> NeueUnterhaltungAsync(CancellationToken ct);
    IAsyncEnumerable<string> SendeNachrichtAsync(Guid unterhaltungId, string text, CancellationToken ct);
    Task<IReadOnlyList<Unterhaltung>> ListeUnterhaltungenAsync(CancellationToken ct);
    Task<Unterhaltung> OeffneUnterhaltungAsync(Guid id, CancellationToken ct);
    Task LoescheUnterhaltungAsync(Guid id, CancellationToken ct);
    Task<Systemstatus> StatusAsync(CancellationToken ct);
}
```

Adapter, Streaming mit Weitergabe des Tokens (`Chat.Adapter.ModelRunner/ModelRunnerClient.cs`, Auszug):

```csharp
public async IAsyncEnumerable<string> StreamAntwortAsync(
    IReadOnlyList<Nachricht> verlauf, string systemanweisung, [EnumeratorCancellation] CancellationToken ct)
{
    using var anfrage = BaueAnfrage(verlauf, systemanweisung);   // Modellname und Adresse aus der Konfiguration
    using var antwort = await _http.SendAsync(anfrage, HttpCompletionOption.ResponseHeadersRead, ct);
    if (!antwort.IsSuccessStatusCode)
        throw new StoerfallException(FallAus(antwort.StatusCode), $"Model Runner antwortet {(int)antwort.StatusCode}");

    await foreach (var teil in LiesTeileAsync(antwort, ct))   // Server-Sent Events, ein Teil je Ereignis
        yield return teil;
}
```

Komponente, Streaming anzeigen (`Chat.Web/Components/Eingabe.razor`, Auszug):

```csharp
private async Task SendenAsync()
{
    _cts = new CancellationTokenSource(_konfiguration.Zeitlimit);
    try
    {
        await foreach (var teil in ChatService.SendeNachrichtAsync(UnterhaltungId, _eingabe, _cts.Token))
        {
            _teile.Add(teil);
            StateHasChanged();
        }
    }
    catch (OperationCanceledException) { _status = "Abgebrochen, Teilantwort bleibt."; }
    catch (StoerfallException e) { _status = Meldung(e.Fall, e.Message); }
}
```

## 12. Review-Checkliste

Steht in `agent/Review_Checkliste.md` und gilt für die Selbstprüfung des Werkzeugs und das Review durch Menschen. Sie prüft diesen Guide in Kurzform: Namen (Abschnitt 1), Schichtgrenze (2), Störfälle (5), Protokoll ohne Chattext (6), Konfiguration (7), Oberfläche (8), Tests je Störfall und Zustandspfeil (9).
