# Karte AP09a: SQLite-Grundlage speichert und lädt Unterhaltungen

**Status:** Implementiert und automatisiert geprüft; bereit für menschliches Review. Visuelle Abnahme offen, nicht als Erledigt freigegeben.

| Feld | Inhalt |
|---|---|
| Verantwortlich | PS |
| Halbtag | Mi PM |
| Referenz | AP09; F03/F04, NF06, Z05, M08 |
| Quellen | `agent/Design.md` Datenhaltung; `agent/Schnittstellen.md`; `agent/Drittkomponenten.md` |
| Testfälle | T01, T05; Z05; Zustandsabbildung aus T12 |

## 1. Ziel (ein Satz)

Ein gemeinsamer SQLite-Store speichert Unterhaltungen mit Nachrichten und Antworten dauerhaft und lädt sie vollständig über den Store-Vertrag.

## 2. Abgrenzung (gehört nicht dazu)

Keine UI-Änderung, produktive DI-Umstellung oder Streaming-Änderung. AP09b übernimmt Anwendungsstart und Wiederherstellung; F04 ergänzt Service-Operationen und Oberfläche. Löschen und Dateiprotokoll bleiben F05 beziehungsweise AP12; keine leeren Methoden dafür ergänzen. Fremdschlüssel und Kaskaden gehören bereits zum Schema.

## 3. Betroffene Komponenten und Dateien

- Neu in `src/Chat.Store.Sqlite/`: `Chat.Store.Sqlite.csproj`, `ChatDbContext.cs`, `ChatDbContextFactory.cs` für Migrationserzeugung, `SqliteStore.cs`, `UnterhaltungDatensatz.cs`, `NachrichtDatensatz.cs`, `AntwortDatensatz.cs`, `Migrationen/<Zeitstempel>_Initial.cs`, zugehörige Designer-Datei und `Migrationen/ChatDbContextModelSnapshot.cs`.
- `src/Chat.Core/IStore.cs`: Listenoperation ergänzen; neu `src/Chat.Core/Modelle/UnterhaltungInfo.cs` mit `Guid Id`, `string Titel`, `DateTimeOffset ErstelltAm`.
- `src/Chat.Core/Modelle/Unterhaltung.cs`, `Antwort.cs`: falls für verlustfreie Rekonstruktion nötig, klar benannte Wiederherstellungsfactory; keine Änderung der erlaubten fachlichen Zustandsübergänge, keine EF-Abhängigkeit in Core.
- `src/Chat.Core/ArbeitsspeicherStore.cs`, `tests/Chat.Tests/SpeicherStore.cs`: neuen Listenvertrag implementieren, vorhandenes Verhalten erhalten.
- `CustomLocalAiFront.slnx`, `tests/Chat.Tests/Chat.Tests.csproj`: Projekt einbinden.
- Neu `tests/Chat.Tests/SqliteStoreTests.cs`, `SqliteTestdatenbank.cs`: isolierte temporäre Testdateien und Migration.
- Neu `.config/dotnet-tools.json`: lokal gepinntes Migrationstool.
- `AGENTS.md` Abschnitt 2: ausschliesslich nach ausdrücklicher Freigabe die unten genannten Entwicklungsbefehle ergänzen.
- `agent/Schnittstellen.md`, `agent/Drittkomponenten.md`, bei bestätigter Schema-Präzisierung `agent/Design.md`, diese Karte und KI-/Testprotokolle.

Unberührt: Webprojekt, Adapter, Docker, `global.json` und produktive Daten. Das Masterkonzept führt die Gruppe nach.

## 4. Akzeptanzkriterien

- [ ] Migration legt Unterhaltung, Nachricht und Antwort mit GUID-Schlüsseln, Fremdschlüsseln, Nachrichtenindex und eindeutiger Antwortzuordnung gemäss Design an. Erneutes Anwenden verändert keine Daten.
- [ ] Leere Unterhaltung wird gespeichert; erneutes Speichern derselben Kennung aktualisiert statt zu duplizieren. Eine Transaktion je Speichern verhindert teilweise gespeicherte Aggregate.
- [ ] Vollständiger Verlauf mit Kennungen, Titel, Zeitpunkten, Text, Zuständen, Dauer, Störfall und Grund wird nach Schliessen und erneutem Öffnen derselben Datei unverändert rekonstruiert.
- [ ] Liste liefert nur Kennung, Titel und Datum, neueste zuerst; Laden sortiert Nachrichten aufsteigend. Bei gleichen Zeitpunkten entscheidet jeweils die Kennung stabil.
- [ ] Leere Liste funktioniert; unbekannte Kennung führt wie bisher zu `KeyNotFoundException`. CancellationToken wird weitergereicht.
- [ ] Gespeicherte Werte sind unabhängig von danach veränderten In-Memory-Objekten. Wiederholtes Speichern erzeugt keine doppelten Nachrichten/Antworten.
- [ ] Kein Modellaufruf und kein Logging von Chattext, SQL-Parameterwerten oder Systemanweisungen.

## 5. Schnittstellen, Datenabbildung und Abhängigkeiten

`SpeichernAsync` und `LadenAsync` behalten ihre Signaturen. Ergänzung der bereits fachlich dokumentierten Operation:

```csharp
Task<IReadOnlyList<UnterhaltungInfo>> ListeAsync(CancellationToken ct);
```

Store-eigene Datensätze bilden auf Core-Modelle ab. Pro Operation ein eigener kurzlebiger Context; kein geteilter Context über einen Blazor Circuit oder parallele Operationen. Siehe [Microsoft: Blazor und EF Core](https://learn.microsoft.com/en-us/aspnet/core/blazor/blazor-ef-core?view=aspnetcore-10.0).

Zeitpunkte bleiben gemäss Design ISO-8601-TEXT. UTC-Normalisierung und zeitliche Sortierung sind ausdrücklich zu testen; kein ungeprüftes SQL-OrderBy auf `DateTimeOffset`. Für den kleinen lokalen Datenbestand ist Sortierung nach Materialisierung zulässig. Siehe [Microsoft: SQLite-Einschränkungen](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations).

**Durch PS freigegebene Schema-Präzisierungen:** `Antwort.ErstelltAm` übernimmt `Nachricht.Zeit`, da Nachricht und Antwort gemeinsam angelegt werden. `Antwort.Fall` wird zusätzlich zu `Grund` als nullable INTEGER gespeichert. `agent/Design.md` ist nachgeführt; die Übernahme ins Masterkonzept bleibt bei der Gruppe.

Verwendete, durch PS freigegebene Pakete: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design` (PrivateAssets, nur Entwicklung) und lokales `dotnet-ef`, jeweils **10.0.12**, MIT. Quellen und transitive SQLite-Komponenten stehen in `agent/Drittkomponenten.md`.

Zusätzliche, in AGENTS.md eingetragene Befehle: `dotnet tool restore` und `dotnet ef migrations add Initial --project src/Chat.Store.Sqlite --output-dir Migrationen`. Projekt-/Paketverweise wurden als Dateien editiert. Keine CLI-Migration gegen produktive Daten.

## 6. Tests

In `SqliteStoreTests.cs`: `Migration_LeereDatei_ErstelltSchema`, `Speichern_LeereUnterhaltung_BleibtNachNeuoeffnenErhalten`, `Speichern_Zweimal_KeineDuplikate`, `Laden_Verlaeufe_BehaeltZuordnungUndZustaende`, `Liste_Zeitpunkte_SindStabilSortiert`, `Laden_UnbekannteKennung_Wirft`, `Speichern_Fehlgeschlagen_RolltTransaktionZurueck`. Alle Endzustände sowie Angefordert und Laeuft abdecken; unterschiedliche Zeit-Offsets und identische Zeitpunkte prüfen. Den Transaktionsfehler nach einer begonnenen Änderung innerhalb derselben Transaktion gezielt auslösen.

Dateien ausschliesslich in einem eigens erzeugten temporären Testverzeichnis; Connections/Contexts vor erneutem Öffnen und Aufräumen freigeben. Keine produktiven Daten und kein In-Memory-Ersatz für Persistenznachweise.

Nach jedem Codeschritt `dotnet build`, `dotnet test`; abschliessend `dotnet format --verify-no-changes`. Menschliches Pflicht-Review von Mapping, Migration und Transaktionsgrenzen; manueller Gesamtnachweis in AP09b/F04.

## 7. Grösse

Geplant und durch PS bestätigt: Mi PM, ein Halbtag für Schema/Migration, Store/Mapping und Tests. Effektiven Aufwand trägt PS nach.

## 8. Board und Freigabe

PS hat AP09a/AP09b und F04 am 23.09.2026 technisch freigegeben und die Umsetzung beauftragt; Verantwortlicher PS, Halbtag Mi PM. Die Karten wurden nacheinander umgesetzt. F04 startet nach ausdrücklichem Benutzerentscheid mit einer neuen aktiven leeren Unterhaltung und der gespeicherten Liste. Am 24.09. wurde die Wiederherstellung des ausgefallenen Hauptcontainers zusätzlich beauftragt. Ein externer Board-Eintrag wurde nicht durch das Werkzeug geändert; Review und Erledigt entscheidet die Gruppe.

## Ergebnis

Karte: AP09a SQLite-Grundlage speichert und lädt Unterhaltungen

Umgesetzt:
- EF-Core-SQLite-Projekt mit explizitem Mapping, Initialmigration, Listenoperation, transaktionalem Speichern und verlustfreier Wiederherstellung.
- Echte temporäre SQLite-Tests für Wiederöffnung, Zustände, Reihenfolge, leere Daten, Aktualisierung und Rollback nach ausgeführtem SQL.

Geänderte Dateien:
- `src/Chat.Store.Sqlite/Chat.Store.Sqlite.csproj`, `ChatDbContext.cs`, `ChatDbContextFactory.cs`, `SqliteStore.cs`, die drei Datensatzklassen und drei Dateien in `Migrationen/`.
- `src/Chat.Core/IStore.cs`, `ArbeitsspeicherStore.cs`, `Modelle/UnterhaltungInfo.cs`, `Modelle/Unterhaltung.cs`, `Modelle/Antwort.cs`.
- `CustomLocalAiFront.slnx`, `.config/dotnet-tools.json`, `tests/Chat.Tests/Chat.Tests.csproj`, `SpeicherStore.cs`, `SqliteTestdatenbank.cs`, `SqliteStoreTests.cs`.
- `AGENTS.md` (freigegebene EF-Befehle), `agent/Design.md`, `agent/Schnittstellen.md`, `agent/Drittkomponenten.md`, Karte und Protokolle.

Nicht angefasst (bewusst): Adapter, global.json, produktive Datenbanken; Web-Anbindung erfolgt unter AP09b.

Prüfungen (Abschlussstand 24.09.2026):
- `dotnet build -m:1 -p:UseSharedCompilation=false`: OK, 0 Warnungen, 0 Fehler.
- `dotnet test --no-build --verbosity minimal`: 46 gesamt, 45 bestanden, 0 fehlgeschlagen, 1 übersprungen. Der bereits vorhandene explizite Test `ModelRunnerClient_EchterServer_LiefertTeile` wurde nicht angefordert; kein Test abgeschwächt oder neu übersprungen.
- `dotnet format --verify-no-changes --no-restore`: OK, keine Änderung. Zeilenenden der im Umfang genannten ChatSeiteTests/ChatServiceTests und Format der generierten Initialmigration zuvor angeglichen; Assertions unverändert.
- Isolierter Container aus dem Projekt-Dockerfile: gebaut, gestartet, HTTP 200. Fünf synthetische Unterhaltungen vor Neustart gespeichert, danach über den Store vollständig verglichen: leere Unterhaltung, Texte, Endzustände und Neustartgrund korrekt. Testcontainer anschliessend entfernt, Hauptdienst nicht beeinflusst.
- Hauptdienst nach Compose-Korrektur neu gebaut: `http://localhost:80/` liefert HTTP 200 und Blazor-Startskript.

### Selbstprüfung nach agent/Review_Checkliste.md

| Punkt | Ja/Nein und Begründung |
|---|---|
| Fachlichkeit: Karte und Use Case | Ja, SQLite-Grundlage und F04 entlang der freigegebenen Karten umgesetzt. |
| Fachlichkeit: alle Akzeptanzkriterien | Nein, funktionale Nachweise bestehen; visuelle Abnahme und menschliches Review sind noch offen. |
| Fachlichkeit: keine eigenmächtige Erweiterung | Ja, technische Freigabe vom 23.09. und Startfehler-Korrekturauftrag vom 24.09. berücksichtigt. |
| Codequalität: Namen | Ja, Fachbegriffe und Dateinamen abgeglichen; Migration durch EF generiert. |
| Codequalität: Schichten | Ja, Razor verwendet IChatService; nur Program.cs verdrahtet den Store. |
| Codequalität: Verständlichkeit | Ja, kurze Store-Operationen und explizites Mapping; vorhandene längere Streamingmethoden nicht fachfremd umgebaut. |
| Codequalität: Format und Komplexität | Ja, globale dotnet-format-Prüfung ohne Änderungen, kein zweiter produktiver Store. |
| Codequalität: Kommentare | Ja, Kommentare erläutern Wiederherstellung, Sperre und Lebenszyklus; kein auskommentierter Code. |
| Fehlerbehandlung: vier Modell-Störfälle | Ja, vorhandene Behandlung erhalten und Regression grün; Laden benötigt keinen Modellaufruf. |
| Fehlerbehandlung: Abbruch | Ja, Teiltext und Endzustand auch bei verzögerter Speicherung geprüft; Ladeschäden werden in der UI gemeldet. |
| Fehlerbehandlung: Eingabegrenzen | Ja, bestehende Tests für leere/zu lange Eingabe weiterhin grün. |
| Fehlerbehandlung: Zustandsübergänge | Ja, vorhandene Übergangstests grün; Neustart ändert nur Angefordert/Laeuft. |
| Auswirkungen: Umfang | Ja, Diff gegen Karten geprüft; Compose-Ergänzung in AP09b dokumentiert. |
| Auswirkungen: Schnittstellen | Ja, freigegebene Listen-/Öffnungsoperationen konkretisiert und dokumentiert. |
| Auswirkungen: Konfiguration/Start | Ja, README und expliziter Compose-Speicherpfad korrigiert; kein stiller In-Memory-Fallback. |
| Sicherheit: keine vertraulichen Daten | Ja, nur synthetische Testdaten; keine persönlichen Pfade oder Nutzdaten in Änderungen übernommen. |
| Sicherheit: Netzwerk | Ja, Anwendung erhält keine externen Dienste; Modellzugriff bleibt im Adapter. |
| Sicherheit: Bibliotheken | Ja, EF-Pakete und Tool freigegeben; Versionen/Lizenzen und transitive SQLite-Komponenten dokumentiert. |
| Sicherheit: Prompt-Trennung | Ja, unverändert. |
| Oberfläche: Semantik/Tastatur | Ja, vorhandene Buttons und Labels erhalten, aktive Auswahl über aria-pressed, Datum als time. |
| Oberfläche: Gestaltung | Ja, vorhandene CSS-/Bootstrap-Klassen weiterverwendet; keine neuen Farb- oder Schriftwerte. |
| Oberfläche: beide Fensterbreiten | Nein, Browser-Schnittstelle meldet keine verbundenen Tabs; visuelle Prüfung offen. |
| Tests: dotnet test grün | Ja, 45 bestanden, 0 fehlgeschlagen; 1 schon vorher expliziter Modellserver-Test nicht ausgeführt. |
| Tests: neue Logik | Ja, 23 neue Tests gegenüber dem Ausgangsstand, einschliesslich echter SQLite-Dateien und bUnit. |
| Tests: manuelle Nachweise | Nein, technischer Start-/Containerneustartnachweis protokolliert; visuelle T05/N03-Abnahme durch Mensch offen. |
| Tests: Einschränkungen dokumentiert | Ja, unten und im Test-/Reviewprotokoll; Masterkonzept durch Gruppe nachzuführen. |
| Nachvollziehbarkeit: KI-Einsatz | Ja, Eintrag zu AP09a/AP09b/F04 ergänzt. |
| Nachvollziehbarkeit: Commit | Nein, kein Commit beauftragt; Vorschlag angegeben, Review durch Gruppe offen. |

Offen, Risiken, Befunde ausserhalb der Karte:
- Menschliches Pflicht-Review von Schema/DI/Synchronisation, visuelle Desktop-/Mobilprüfung und vollständige manuelle T05-Abnahme stehen aus. Browserwerkzeug meldet keine verbundenen Browser; daraus wird keine erfolgreiche Sichtprüfung abgeleitet.
- Masterkonzept Kapitel 7/9 und 14.3 durch die Gruppe mit den dokumentierten Präzisierungen abgleichen.
- Zeitaufwand der dauerhaften Speicherung jedes Antwortteils auf dem Referenzgerät mit echtem Modell messen; automatisierte Funktionsprüfung ersetzt diese Messung nicht.
- Dauer effektiv durch PS nachtragen; kein Commit erstellt und keine menschliche Freigabe vorweggenommen.

Vorschlag Commit-Nachricht: AP09a: SQLite-Persistenz und gespeicherte Unterhaltungen bereitstellen / KI: Codex, geprueft von XX
