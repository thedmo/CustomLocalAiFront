# Schnittstellen

Referenz für Entwickler und KI-Werkzeuge. Quelle ist das Konzept, Kapitel 7.3 und 7.4; ändert sich hier etwas, wird das Konzept im selben Schritt angepasst (AGENTS.md Abschnitt 10). Namen im Code tragen das Suffix Async (`agent/StylingGuide.md` Abschnitt 1).

## Chat-Seite zu Backend: `IChatService` (Chat.Core)

| Methode | Funktion | Eingabe | Ausgabe |
|---|---|---|---|
| `NeueUnterhaltungAsync(ct)` | F03 | keine | Kennung der Unterhaltung |
| `SendeNachrichtAsync(unterhaltungId, text, ct)` | F01 | Kennung, Text, Abbruch-Token | `IAsyncEnumerable<string>`, die Teile der Antwort; am Ende steht Zustand und Kennung der Antwort im Store |
| `AbbrechenAsync(antwortId, ct)` | F02 | Kennung der Antwort | Zustand Abgebrochen; technisch löst die Seite den CancellationToken aus |
| `ListeUnterhaltungenAsync(ct)` | F04 | keine | Kennungen, Titel, Datum |
| `OeffneUnterhaltungAsync(id, ct)` | F04 | Kennung | Nachrichten und Antworten in Reihenfolge |
| `LoescheUnterhaltungAsync(id, ct)` | F05 | Kennung | Bestätigung |
| `StatusAsync(ct)` | F06, K07 | keine | Modellserver erreichbar, Modellname, Konfiguration gültig |

Fehler kommen als `StoerfallException` mit einem der vier Fälle: `ModellserverNichtErreichbar`, `ModellUnbekannt`, `KonfigurationUngueltig`, `Zeitueberschreitung`. Die Seite zeigt die Meldung mit Grund (M09), die Eingabe bleibt frei. Ein HTTP-Endpunkt `GET /status` mit demselben Inhalt wie `StatusAsync` dient Betrieb und Tests [Entscheid AP06].

## Backend zu Modellserver: `IModelServerClient` (Chat.Core), Umsetzung `ModelRunnerClient`

| Methode | Zweck | Eingabe | Ausgabe |
|---|---|---|---|
| `StreamAntwortAsync(verlauf, systemanweisung, ct)` | Antwort in Teilen holen | Verlauf als Liste von Nachrichten, Systemanweisung, Token | `IAsyncEnumerable<string>` |

Der Adapter sendet `POST chat/completions` mit Modellname, Systemanweisung und Verlauf, `stream=true`, und liest die Teile als Server-Sent Events. Adresse und Modellname kommen aus der Konfiguration. Aus dem Anwendungs-Container ist der Model Runner unter `http://model-runner.docker.internal` erreichbar, vom Host über den in Docker Desktop freigegebenen TCP-Port [Adresse im Spike prüfen, Benjamin]. Nur der Adapter kennt diese Adresse.

## Backend zu Datenhaltung: `IStore` (Chat.Core), Umsetzung `SqliteStore`

| Methode | Zweck |
|---|---|
| `SpeichernAsync(unterhaltung, ct)` | Unterhaltung mit Nachrichten und Antworten anlegen oder aktualisieren; eine Transaktion je Aufruf |
| `LadenAsync(id, ct)` | Unterhaltung mit Verlauf laden |
| `ListeAsync(ct)` | Kennung, Titel, Datum aller Unterhaltungen |
| `LoeschenAsync(id, ct)` | Unterhaltung mit Nachrichten und Antworten entfernen (Kaskade), Protokoll bleibt |
| `ProtokollAsync(eintrag, ct)` | technischer Eintrag ohne Chattext (Konzept 9.3) |

## Konfiguration (Abschnitt `Chat` in `appsettings.json`, Konzept 7.4)

`AdresseModellserver`, `Modellname`, `Systemanweisung`, `Temperatur`, `MaximaleAntwortlaenge`, `Eingabegrenze` (4000 Zeichen, Entscheid offen), `ZeitlimitSekunden` (60), `SpeicherortDb`, `Protokolldatei`. Überschreibbar per Umgebungsvariable `Chat__Modellname`. Beispiel in `appsettings.example.json`.

## Zustände der Antwort (Konzept 4.7.6)

`Angefordert` nach dem Speichern der Nachricht, `Laeuft` ab dem ersten Teil, `Fertig` nach dem Ende, `Abgebrochen` nach Abbruch, `Gestoert` bei einem Störfall. Aus `Fertig`, `Abgebrochen` und `Gestoert` gibt es keinen Übergang mehr; der ChatService prüft die Übergänge.
