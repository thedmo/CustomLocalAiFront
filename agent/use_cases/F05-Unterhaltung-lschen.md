# Karte F05: Unterhaltung löschen

**Status:** Implementiert und automatisiert geprüft; menschliches Review und manuelle Abnahme offen. Umsetzung und Bedien-/Grenzfallentscheidungen durch PS am 24.09.2026 beauftragt; notwendige Test-Store-Ergänzung zusätzlich ausdrücklich freigegeben.

| Feld | Inhalt |
|---|---|
| Verantwortlich | PS |
| Halbtag | Do AM |
| Referenz | F05; Z05, M08; Priorität 2 |
| Quellen | `agent/Use_Cases.md` F05/Z05; `agent/Design.md` «Kennung, Löschung, Neustart»; `agent/Schnittstellen.md` |
| Testfälle | T06; T01 bis T05 als Regression; Z05 |
| Voraussetzung | SQLite-Einbindung AP09a/AP09b und Unterhaltungsliste F04 vorhanden |

## 1. Ziel

Der Benutzer löscht eine ausgewählte Unterhaltung nach Bestätigung dauerhaft mit allen Nachrichten und Antworten.

## 2. Abgrenzung

Kein Sammellöschen, Wiederherstellen oder neues Dateiprotokoll. Keine neuen Abhängigkeiten, Schema- oder Konfigurationsänderungen; vorhandene SQLite-Kaskaden verwenden.

## 3. Betroffene Komponenten und Dateien

- `src/LocalAiFront/Components/Pages/Home.razor`, bei Bedarf `Home.razor.css`: Löschen, Bestätigung und Aktualisierung der Ansicht.
- Neu `src/LocalAiFront/Components/Pages/Home.Loeschen.cs` und `src/Chat.Core/UnterhaltungsZugriff.cs`: F05-Logik innerhalb derselben Komponenten aufteilen, damit Dateien unter 300 Zeilen bleiben (StylingGuide Abschnitt 3).
- `src/Chat.Core/IChatService.cs`, `ChatService.cs`, `IStore.cs`, `ArbeitsspeicherStore.cs`: dokumentierte Löschoperationen ergänzen.
- `src/Chat.Store.Sqlite/SqliteStore.cs`: gezieltes Löschen mit bestehender Kaskade.
- `tests/Chat.Tests/ChatServiceUnterhaltungenTests.cs`, `ChatSeiteUnterhaltungenTests.cs`, `SqliteStoreTests.cs`: Löschtests ergänzen; `FakeChatService.cs` und `SpeicherStore.cs` um die Verträge erweitern. Bestehende Testaussagen erhalten.
- `tests/Chat.Tests/ChatServicePersistenzTests.cs`: ausschliesslich `LoeschenAsync` im Test-Store weiterreichen; Testaussagen unverändert (Freigabe PS).
- Dokumentation: diese Karte, `agent/Schnittstellen.md`, ergänzende T06-Prüfungen in `agent/Testfaelle.md`, `agent/protokolle/KI-Einsatz.md` und `agent/protokolle/Test_und_Reviewprotokoll.md`.

Unberührt: Adapter, Migrationen, Projektdateien, `docker_compose.yml`, `appsettings*.json`, `global.json` und bestehende Daten-/Protokolldateien.

## 4. Akzeptanzkriterien

- [x] «Löschen» fragt für die ausgewählte Unterhaltung nach Bestätigung; «Abbrechen» verändert keine Daten.
- [x] Nach Bestätigung sind nur diese Unterhaltung und alle zugehörigen Nachrichten/Antworten entfernt, auch nach Neustart. Andere Unterhaltungen und technische Protokolle bleiben erhalten.
- [x] Liste und Verlauf werden aktualisiert; die gelöschte Unterhaltung verschwindet aus der Liste. Eine vorhandene Unterhaltung wird geöffnet, andernfalls bleibt die Liste leer. Eine neue Unterhaltung entsteht erst durch die vorhandene F03-Bedienung.
- [x] Während Senden, Laden oder Löschen ist die Löschbedienung gesperrt. Der Service weist Löschen einer Unterhaltung mit aktiver Antwort zurück; nach Abschluss oder Abbruch ist Löschen möglich. Gleichzeitiges Senden/Löschen darf keine gelöschten Daten wiederherstellen.
- [x] Bereits gelöschte Kennung: Löschaufruf endet erfolgreich. Bei Speicherfehler erscheint eine verständliche Meldung ohne Erfolgsmeldung; erneuter Versuch bleibt möglich.
- [x] Löschen funktioniert ohne erreichbaren Modellserver; die Seite verwendet ausschliesslich `IChatService`.

Die Haken bezeichnen Umsetzung und automatisierte Nachweise. Neustart wurde durch erneutes Öffnen der SQLite-Datei und Startup-Initialisierung geprüft; manueller Prozessneustart und Protokollvergleich stehen noch aus. Der Löschpfad greift ausschliesslich auf Chatentitäten zu.

## 5. Schnittstellen und Konfiguration

Die bereits dokumentierten Zielverträge als `Task LoescheUnterhaltungAsync(Guid id, CancellationToken ct)` in `IChatService` und `Task LoeschenAsync(Guid id, CancellationToken ct)` in `IStore` ergänzen. Erfolgreicher Abschluss gilt als Bestätigung; fehlende Kennungen sind bereits gelöscht. Aktive Antwort: `InvalidOperationException` mit verständlicher Anzeige, kein neuer Modell-Störfall. Den Implementierungsstand in `agent/Schnittstellen.md` nachführen. Keine neuen Konfigurationswerte.

## 6. Tests

- Automatisiert (T06): Bestätigen/Abbrechen, Ansicht nach Löschen, Bedienungssperre, aktive Antwort und gleichzeitiges Senden/Löschen, unbekannte Kennung, Speicherfehler und kein Modellaufruf. In isolierter SQLite-Testdatenbank vollständige Kaskade, Erhalt einer zweiten Unterhaltung und Fortbestand der Löschung nach erneutem Öffnen prüfen.
- Manuell (T06/Z05): Fünf Unterhaltungen anlegen, Anwendung neu starten, eine über die Oberfläche löschen; vier bestehende bleiben unverändert, gelöschte bleibt nach erneutem Neustart weg. Die Seite darf keinen zusätzlichen leeren Datensatz anlegen. Bestätigung auch abbrechen und Löschen bei gestopptem Modellserver prüfen; vorhandene technische Protokolle bleiben erhalten.
- Bei Umsetzung: `dotnet build`, `dotnet test`, `dotnet format`; manuelle Ergebnisse ins Test- und Reviewprotokoll eintragen.

## 7. Grösse

Ein Halbtag (Do AM): Löschpfad, Oberfläche, Tests und Dokumentation.

## 8. Board und Freigabe

PS hat die Karte mit dem Umsetzungsauftrag übergeben und die zusätzliche Test-Store-Anpassung bestätigt. Zuordnung: F05, PS, Do AM; technischer Stand bereit für Review. Der externe Projektplan wurde nicht geprüft oder geändert; Boardstatus und effektive Dauer trägt PS nach.

## Ergebnis

Karte: F05 Unterhaltung löschen

Umgesetzt:

- Löschbutton für die ausgewählte Unterhaltung mit Browser-Bestätigung und Bedienungssperre. Nach Erfolg verschwindet sie aus der Liste; eine vorhandene Unterhaltung wird geöffnet oder die Liste bleibt leer. Nach Rückmeldung von PS wurde die zunächst automatische Ersatz-Unterhaltung entfernt, weil sie wie eine nicht gelöschte Unterhaltung wirkte.
- Service-/Store-Verträge ergänzt; SQLite entfernt die Unterhaltung mit vorhandenen Kaskaden. Fehlende Kennungen gelten als bereits gelöscht.
- Gemeinsame Koordination je Singleton-Store schützt auch zwischen zwei Circuits vor Löschen aktiver Antworten und Wiederherstellung durch gleichzeitiges Senden. Kein Modellaufruf zum Löschen.
- Speicherfehler, fehlgeschlagene Neuanlage nach Löschung und inzwischen in anderer Sitzung gelöschte Unterhaltung lassen die Seite bedienbar.
- 18 neue Testfälle; bestehende Testaussagen erhalten. F05-Code zur Einhaltung der Dateigrösse in zwei kleine Dateien derselben Komponenten aufgeteilt.

Geänderte Dateien:

- Core: `src/Chat.Core/IChatService.cs`, `IStore.cs`, `ChatService.cs`, `ArbeitsspeicherStore.cs`, neu `UnterhaltungsZugriff.cs`.
- Store: `src/Chat.Store.Sqlite/SqliteStore.cs`.
- Oberfläche: `src/LocalAiFront/Components/Pages/Home.razor`, neu `Home.Loeschen.cs`.
- Tests unter `tests/Chat.Tests/`: `ChatServiceUnterhaltungenTests.cs`, `ChatSeiteUnterhaltungenTests.cs`, `SqliteStoreTests.cs`, `FakeChatService.cs`, `SpeicherStore.cs`, `ChatServicePersistenzTests.cs` (nur freigegebene Weiterleitung).
- Dokumentation: diese Karte, `agent/Schnittstellen.md`, `agent/Testfaelle.md`, `agent/protokolle/KI-Einsatz.md`, `agent/protokolle/Test_und_Reviewprotokoll.md`.

Nicht angefasst (bewusst):

- Adapter, Antwortzustandsmodell, Schema/Migrationen, Projektdateien, Konfiguration, Docker Compose und bestehende Daten-/Protokolldateien. Kein Commit und kein Neustart des laufenden Dienstes.

Prüfungen:

- `dotnet build CustomLocalAiFront.slnx --no-restore --disable-build-servers -m:1`: OK, 0 Warnungen, 0 Fehler.
- `dotnet test --project tests/Chat.Tests/Chat.Tests.csproj --no-build`: 65 bestanden, 0 fehlgeschlagen, 1 vorhandener expliziter Integrationstest nicht ausgeführt (`ChatServiceTests.ModelRunnerClient_EchterServer_LiefertTeile`).
- `dotnet format CustomLocalAiFront.slnx --no-restore --verify-no-changes --verbosity minimal`: OK, keine Änderung erforderlich. Zuvor gefundene Feldbenennung und fehlende Klammern korrigiert.
- `git diff --check`: OK. Neue Dateien zusätzlich gelesen; bestehender Diff auf Umfang und unveränderte Testaussagen geprüft.
- T06/Z05 automatisiert: echte temporäre SQLite-Datei, Kaskade, fünf Originalverläufe über die Komponente, Neuinitialisierung, zwei Service-Instanzen und gezielt verzögerte konkurrierende Operationen.
- API-Abgleich: Microsoft-Dokumentation zu [ExecuteDelete](https://learn.microsoft.com/en-us/ef/core/saving/execute-insert-update-delete) und [Kaskaden](https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete).

Diff-Auszug (vollständiger Diff im Arbeitsverzeichnis):

```diff
+ Task LoescheUnterhaltungAsync(Guid id, CancellationToken ct);
+ Task LoeschenAsync(Guid id, CancellationToken ct);
+ await db.Unterhaltungen.Where(u => u.Id == id).ExecuteDeleteAsync(ct);
```

Selbstprüfung (`agent/Review_Checkliste.md`), in Reihenfolge der Checkliste:

| Punkt | Ergebnis und Grund |
|---|---|
| Fachlichkeit: Karte/Konzept/Ziele | Ja; F05/T06/Z05 umgesetzt. Die Rückmeldung von PS präzisiert: Löschen legt keine Ersatz-Unterhaltung an. |
| Alle Akzeptanzkriterien | Nein, noch nicht vollständig abgenommen; automatisiert erfüllt, manuelle Nachweise offen. |
| Keine fachliche Erweiterung | Ja; ausschliesslich Löschen und notwendige Konkurrenz-/Fehlerbehandlung. |
| Namen/Fachbegriffe | Ja; deutsche Fachbegriffe, Async-Suffixe; Partial-Datei enthält den F05-Teil von Home. |
| Schichtgrenzen | Ja; Seite verwendet IChatService, nur Store kennt SQLite. |
| Verständlichkeit/Wartbarkeit | Ja, als Selbstprüfung; kleine neue Methoden, Dateien unter 300 Zeilen. Verständnis bestätigt die Reviewperson. |
| Format/Komplexität | Ja; Formatprüfung grün, eine gemeinsame Start-/Löschsperre pro Store. |
| Kommentare | Ja; erklären Sperre/Kaskade, kein auskommentierter Code. |
| Vier Störfälle | Ja; bestehende Behandlung unverändert, Regression grün; Löschfehler separat. |
| Abbruch/Teilantwort | Ja; bestehende Tests grün, Löschen nach gespeicherten Endzuständen geprüft. |
| Eingabeprüfung | Ja; unverändert, Regression grün. |
| Zustandsübergänge | Ja; keine Änderungen am Zustandsmodell. |
| Umfang/Dateiliste | Ja; nur genannte Komponenten, zusätzliche Test-Weiterleitung ausdrücklich freigegeben. |
| Schnittstellen | Ja; vorhandene Zielverträge umgesetzt, Implementierungsstand nachgeführt. |
| Konfiguration/Installation | Ja; unverändert, keine neuen Werte. |
| Geheimnisse/persönliche Daten | Ja; keine hinzugefügt; Tests mit synthetischen Daten. |
| Netzwerk/Modellwerkzeuge | Ja; Löschpfad ohne Netzwerk oder Modellaufruf. |
| Bibliotheken | Ja; keine neuen Abhängigkeiten. |
| Systemanweisung/Eingaben | Ja; Trennung und Validierung unverändert. |
| HTML/Beschriftung/Tastatur | Ja hinsichtlich Code; beschrifteter Button, nativer Bestätigungsdialog. Manuelle Tastaturprüfung offen. |
| Gestaltung/CSS-Variablen | Ja; vorhandene Bootstrap-Klassen, keine neuen Farben oder CSS-Werte. |
| Beide Fensterbreiten | Nein; kein Browser verbunden. |
| Automatisierte Tests | Ja; 65 bestanden, 0 fehlgeschlagen; expliziter Modelltest unverändert ausgenommen. |
| Neue Logik getestet | Ja; 18 neue Fälle für Löschen, Abbruch, Fehler, Kaskade, Konkurrenz und fehlende Ersatz-Unterhaltung; keine neuen Zustandspfeile. |
| Manuelle Tests protokolliert | Nein; ausstehender Prüfumfang samt erwartetem Ergebnis im Protokoll vermerkt. |
| Einschränkungen dokumentiert | Ja; unten genannt, kein neues dauerhaftes Produktdefizit festgestellt. |
| KI-Einsatz | Ja; Vorbereitung und Umsetzung eingetragen. |
| Commit-Nachricht | Ja als Vorschlag; kein Commit ohne Auftrag und Review. |

Offen, Risiken, Befunde ausserhalb der Karte:

- Kein Browser verbunden: T06/Z05 manuell mit echtem Prozessneustart, Tastatur, beiden Fensterbreiten und unveränderten technischen Protokollen prüfen. Der laufende Dienst enthält F05 erst nach Neubau/Neustart durch den Betreiber.
- Unabhängiges menschliches Review der gemeinsamen Service-Koordination erforderlich. Die Sperre schützt den vorgesehenen einzelnen Backend-Prozess mit gemeinsamem Singleton-Store; mehrere Prozesse sind nicht abgedeckt.
- Die Gruppe übernimmt die freigegebenen Bedien-/Synchronisationspräzisierungen aus dieser Karte und `Schnittstellen.md` in den Masterkonzept-Abgleich (Kapitel 7/9, Änderungsnachweis 14.3).
- Boardstatus, effektive Dauer und abschliessende Freigabe durch PS/Gruppe offen. Keine Freigabe durch Codex.

Vorschlag Commit-Nachricht: `F05: Unterhaltung nach Bestätigung vollständig löschen` / `KI: Codex, geprueft von XX` (XX erst nach tatsächlichem Review ersetzen).

Definition of Done: automatisierter Build/Test, Formatierung, Datenschutz und Dokumentation technisch erfüllt; Containerstart dieses Stands, manuelle Abnahme, unabhängiges Review sowie Board-/Commit-Abschluss offen.
