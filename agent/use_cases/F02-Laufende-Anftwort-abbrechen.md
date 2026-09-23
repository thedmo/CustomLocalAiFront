# Karte F02: Laufende Antwort abbrechen

| Feld           | Inhalt                                                                                                                                                |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| Verantwortlich | [festzulegen]                                                                                                                                         |
| Halbtag        | [festzulegen]                                                                                                                                         |
| Referenz       | F02; Z03; M06                                                                                                                                         |
| Quellen        | `agent/Use_Cases.md` F02 und Erweiterung 4a; `agent/Design.md` Datenfluss und Abbruch; `agent/Schnittstellen.md` `AbbrechenAsync` und Antwortzustände |
| Testfälle      | T04 aus `agent/Testfaelle.md`                                                                                                                         |

## 1. Ziel (ein Satz)

Der ChatService bricht eine laufende Modellantwort über ihre Antwort-ID ab, beendet die Modellanfrage und lässt die bisher gelieferte Teilantwort sichtbar und als abgebrochen gespeichert.

## 2. Abgrenzung (gehört nicht dazu)

Keine neue HTTP-Schnittstelle, keine Oberfläche und keine Änderung am Modellserver-Protokoll. Die technische Behandlung von Zeitüberschreitung und Verbindungsverlust gehört zu F01/F06.

## 3. Betroffene Komponenten und Dateien

- `src/Chat.Core`: `IChatService`, `ChatService`, `AntwortLauf`, `AntwortZustand`, aktive Anfrage mit `CancellationTokenSource`
- `src/Chat.Adapter.ModelRunner`: Weitergabe des `CancellationToken` an HTTP-Anfrage und Stream
- `tests/Chat.Tests`: Abbruch vor dem ersten Teil und während des Streams
- Unberührt: `web_app/LocalAiFront`, `src/Chat.Store.Sqlite`, `docker_compose.yml`, Konfiguration und Projektdateien

## 4. Akzeptanzkriterien (prüfbar, vorher festgelegt; beim Review abhaken)

- [ ] Die Antwort-ID ist vor dem Modellaufruf verfügbar; `AbbrechenAsync` kann damit eine vorbereitete Anfrage abbrechen.
- [ ] Abbruch vor dem ersten Teil speichert Zustand `Abgebrochen` und ruft den Modellserver nicht auf.
- [ ] Abbruch während des Streams beendet die Anfrage; bereits gelieferte Teile bleiben sichtbar und werden als `Abgebrochen` gespeichert.
- [ ] Nach dem Abbruch ist die Eingabe frei und eine weitere Nachricht ohne Neustart möglich.
- [ ] Wiederholte Abbrüche, unbekannte IDs und Abbrüche nach einem Endzustand ändern nichts und erzeugen keinen zweiten Abschluss.

## 5. Schnittstellen und Konfiguration

`IChatService.AbbrechenAsync(antwortId, ct)` löst die interne Anfrage-`CancellationTokenSource` aus. `SendeNachrichtAsync` liefert vorher ein `AntwortLauf` mit `AntwortId` und `Teile`. `IModelServerClient.StreamAntwortAsync(verlauf, systemanweisung, ct)` erhält den abgeleiteten Token; der Adapter schliesst damit HTTP-Anfrage und SSE-Stream. Es werden keine neuen Konfigurationswerte benötigt.

## 6. Tests

- Automatisiert: T04 mit Abbruch vor dem ersten Teil und während des Streams; vorhandene Teilantwort und Zustand `Abgebrochen` prüfen.
- Manuell: Gegen den lokalen Model Runner nachweisen, dass die Modellerzeugung nach dem Abbruch endet; danach eine weitere Nachricht senden.

## 7. Grösse

Ein Halbtag. Teilschritte: aktive Anfrage und Zustandsabschluss, Token-Weitergabe im Adapter, automatisierte Tests und Nachweis am lokalen Model Runner.

## 8. Board

[Im Projektplan eintragen: Verantwortlicher und Status In Arbeit.]

---

## Ergebnis (das Werkzeug schreibt seinen Bericht nach AGENTS.md Abschnitt 9 hierher; der Mensch prüft und hakt ab)

- Was gebaut wurde: Karte F02 erstellt; keine Code- oder Teständerung.
- Geänderte Dateien: `agent/use_cases/F02-Laufende-Anftwort-abbrechen.md`
- Nicht angefasst (bewusst): alle anderen Dateien.
- Prüfungen: `dotnet build`, `dotnet test` und `dotnet format` nicht ausgeführt, da nur eine Kartendatei erstellt wurde.
- Selbstprüfung (`agent/Review_Checkliste.md`): noch offen; keine Implementierung geprüft.
- Offen, Risiken, Befunde ausserhalb der Karte: Verantwortlicher, Halbtag und Boardstatus sind einzutragen; manuelle Prüfung steht aus.
- Vorschlag Commit-Nachricht: `F02: Laufende Antwort abbrechen / KI: GitHub Copilot, geprüft von [Kürzel]`
