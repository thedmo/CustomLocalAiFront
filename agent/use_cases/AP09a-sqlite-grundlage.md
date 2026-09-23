# Karte AP09a: SQLite-Grundlage speichert und lädt Unterhaltungen

**Status:** Ausgearbeiteter Vorschlag; Blockiert bis Paket-, Design- und Werkzeugfreigabe sowie Board-Bestätigung. Teilkarte von AP09; keine Umsetzung erfolgt.

| Feld | Inhalt |
|---|---|
| Verantwortlich | Von der Gruppe zu benennen |
| Halbtag | Ein Halbtag vor AP09b, Termin zu bestätigen |
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

**Schema-Präzisierungen zur Gruppenfreigabe:** Das Design nennt `Antwort.ErstelltAm`, das Core-Modell bisher nicht: vorgeschlagen ist die Übernahme von `Nachricht.Zeit`, da Nachricht und Antwort gemeinsam angelegt werden. Das Core-Modell besitzt hingegen `Antwort.Fall` zusätzlich zu `Grund`: vorgeschlagen ist eine nullable INTEGER-Spalte `Fall`, damit diese Information verlustfrei gespeichert wird. Vor Umsetzung bestätigen und im Masterkonzept nachführen; keine eigenständige Schemafreigabe durch das Werkzeug.

Paketvorschlag: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design` (PrivateAssets, nur Entwicklung) und lokales `dotnet-ef`, jeweils **10.0.12**, MIT. Quellen und Freigabestatus in `agent/Drittkomponenten.md`; nichts installiert oder bereits genehmigt.

Zusätzliche Befehle für Migrationserzeugung: `dotnet tool restore` und `dotnet ef migrations add Initial --project src/Chat.Store.Sqlite --output-dir Migrationen`. Vor Ausführung in AGENTS.md freigeben. Projekt-/Paketverweise als Dateien editieren; `dotnet add package` ist nicht nötig. Keine CLI-Migration gegen produktive Daten.

## 6. Tests

In `SqliteStoreTests.cs`: `Migration_LeereDatei_ErstelltSchema`, `Speichern_LeereUnterhaltung_BleibtNachNeuoeffnenErhalten`, `Speichern_Zweimal_KeineDuplikate`, `Laden_Verlaeufe_BehaeltZuordnungUndZustaende`, `Liste_Zeitpunkte_SindStabilSortiert`, `Laden_UnbekannteKennung_Wirft`, `Speichern_Fehlgeschlagen_RolltTransaktionZurueck`. Alle Endzustände sowie Angefordert und Laeuft abdecken; unterschiedliche Zeit-Offsets und identische Zeitpunkte prüfen. Den Transaktionsfehler nach einer begonnenen Änderung innerhalb derselben Transaktion gezielt auslösen.

Dateien ausschliesslich in einem eigens erzeugten temporären Testverzeichnis; Connections/Contexts vor erneutem Öffnen und Aufräumen freigeben. Keine produktiven Daten und kein In-Memory-Ersatz für Persistenznachweise.

Nach jedem Codeschritt `dotnet build`, `dotnet test`; abschliessend `dotnet format --verify-no-changes`. Menschliches Pflicht-Review von Mapping, Migration und Transaktionsgrenzen; manueller Gesamtnachweis in AP09b/F04.

## 7. Grösse

Schätzung ein Halbtag: Schema/Migration, Store/Mapping, Tests. Vor READY durch Karteninhaber bestätigen; bei höherem Aufwand weiter teilen, keine Abnahme weglassen.

## 8. Board

Erste Teilkarte von AP09. Gruppe benennt Verantwortlichen, Termin und Status In Arbeit; WIP höchstens zwei. READY erfordert diese Angaben sowie Paket-, Werkzeug- und Schemafreigabe. AP09b beginnt nach den grünen Prüfungen dieser Karte.

## Ergebnis

Vorbereitung vom 23.09.2026: Karte anhand von Code, Design und Schnittstellen erstellt. Datenmodell-Unterschiede und fehlende Listenoperation sichtbar gemacht. Keine Implementierung, Installation, Migration oder Datenänderung; Build, Tests und Formatierung nicht ausgeführt. Dokumentationsprüfung und Selbstprüfung siehe F04-Karte. Menschliches Review und Implementierungsnachweise offen.
