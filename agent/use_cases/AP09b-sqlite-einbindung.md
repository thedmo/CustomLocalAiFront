# Karte AP09b: Anwendung nutzt SQLite und stellt Antworten nach Neustart wieder her

**Status:** Ausgearbeiteter Vorschlag; Blockiert bis AP09a geprüft, Umfang bestätigt und Board-Angaben ergänzt sind. Teilkarte von AP09; keine Umsetzung erfolgt.

| Feld | Inhalt |
|---|---|
| Verantwortlich | Von der Gruppe zu benennen |
| Halbtag | Ein Halbtag nach AP09a und vor F04, Termin zu bestätigen |
| Referenz | AP09; F01/F02/F03/F04, NF06, Z05, M08 |
| Quellen | `agent/Design.md` Datenhaltung und Neustart; `agent/Schnittstellen.md`; AP09a |
| Testfälle | T01 bis T05, T12; Z05, N05 |

## 1. Ziel (ein Satz)

Die Anwendung verwendet den gemeinsamen SQLite-Store und markiert beim Start zurückgebliebene offene Antworten als Gestört mit Grund «Neustart», ohne gespeicherten Text zu verlieren.

## 2. Abgrenzung (gehört nicht dazu)

Kein neuer Store, keine neue Oberfläche, kein Löschen und kein Dateiprotokoll. F04 bindet Auflisten und Öffnen an die Chat-Seite. Senden und Abbruch werden nur für dauerhafte Speicherung angepasst, nicht fachlich umgebaut.

F03 speichert beim Neuanlegen bereits über IStore. Der Austausch der registrierten Umsetzung macht diesen Vorgang dauerhaft; die Wiederherstellung der Liste gehört F04.

## 3. Betroffene Komponenten und Dateien

- `src/LocalAiFront/Program.cs`, `src/LocalAiFront/LocalAiFront.csproj`: SQLite-Projektreferenz, DI und Initialisierung vor Annahme von Benutzeranfragen; Komponenten kennen weiterhin nur Core.
- `src/LocalAiFront/Dockerfile`: neues Store-Projekt im Restore-Schritt; keine neuen Images.
- Neu `src/Chat.Store.Sqlite/SqliteInitialisierung.cs`: Schema anwenden und offene Antworten einmal beim Prozessstart wiederherstellen, niemals bei jedem Laden oder Circuit-Start.
- `src/Chat.Store.Sqlite/SqliteStore.cs`, `ChatDbContext.cs`: nur notwendige Einbindung von Initialisierung und laufender Speicherung.
- `src/Chat.Core/ChatService.cs`, `AktiveAnfrage.cs`: konsistente Speicherung von angefordertem Zustand, empfangenen Teilen und Endzustand; Abschluss-/Abbruchsynchronisation erhalten.
- Neu `tests/Chat.Tests/SqliteNeustartTests.cs`, `ChatServicePersistenzTests.cs`; Testaufbau in `SqliteTestdatenbank.cs` aus AP09a.
- `tests/Chat.Tests/ChatServiceTests.cs`, `ChatServiceAbbruchTests.cs`: nur nötige Annahmen über zusätzliche Speicheraufrufe anpassen; fachliche Aussagen erhalten.
- `README.md`: initiale Datenbankanlage, konfigurierte Pfade und lokaler Start dokumentieren.
- Diese Karte, `agent/protokolle/KI-Einsatz.md`, `agent/protokolle/Test_und_Reviewprotokoll.md`.

Unberührt: Adapter, `IModelServerClient`, Modellparameter, `Konfiguration.cs`, `appsettings*.json`, `global.json`, `docker_compose.yml`. Keine direkten Eingriffe in bestehende Datenbanken oder technische Protokolle durch das Werkzeug.

## 4. Akzeptanzkriterien

- [ ] Composition Root verwendet SqliteStore; Neuanlegen und Senden nutzen dieselbe Datenbank. Fehlender/unbrauchbarer Speicherpfad führt zu nachvollziehbarem Startfehler, niemals zu stillem Rückfall auf flüchtigen Speicher.
- [ ] Migration und Wiederherstellung laufen vor Benutzeranfragen einmal pro Prozessstart. Neue Circuits lösen keine Wiederherstellung laufender Antworten aus.
- [ ] Gespeicherte Antworten in Angefordert oder Laeuft erhalten Gestoert und Grund «Neustart»; Text und Kennungen bleiben erhalten. Fertig, Abgebrochen und Gestoert bleiben unverändert. Erneute Initialisierung verändert diese Endzustände nicht.
- [ ] Nachricht und Angefordert werden vor dem Modellaufruf gespeichert, empfangene Teile samt Laeuft und schliesslich der Endzustand ebenfalls. Verspätete Teilspeicherung darf keinen bereits gespeicherten Endzustand überschreiben.
- [ ] Nach Neustart mit derselben Datei bleiben fünf Unterhaltungen einschliesslich leerer vollständig ladbar. Container-Neustart verwendet den bestehenden Mount.
- [ ] Migration, Speicherung und Wiederherstellung benötigen keinen erreichbaren Modellserver; übrige Konfigurationsvalidierung bleibt erhalten.
- [ ] Kein Context wird parallel verwendet; Datenbank-/Frameworklogs enthalten keine Chattexte oder Parameterwerte. Bestehende F01/F02-Tests bleiben grün.

## 5. Schnittstellen und Konfiguration

Keine neue öffentliche Operation in IChatService oder IStore. Initialisierung gehört zur technischen Store-Einbindung. Wiederherstellung nutzt die vorgesehenen Übergänge nach Gestoert; «Neustart» braucht keinen neuen Modell-Störfall (`Fall` bleibt dabei null).

`Chat:SpeicherortDb` beziehungsweise `Chat__SpeicherortDb` wird wie bereits in `Program.cs` gelesen verwendet. Die Beispielkonfiguration enthält `/app/data/chat.db`; bestehendes Volume bleibt massgeblich. Tests übergeben einen expliziten temporären Pfad. Für Betrieb ohne Docker einen konfigurierbaren relativen Pfad dokumentieren, keine persönlichen Pfade eintragen.

**Speicherstrategie zur Bestätigung:** Jeden angenommenen Textteil vor Weitergabe dauerhaft speichern. Das ist für den ersten lokalen Stand einfach prüfbar und erhält bei Prozessabbruch die bereits ausgegebenen Teile. Abschluss und Teilspeicherung müssen pro aktiver Anfrage geordnet sein; keine nebenläufigen unbeobachteten Speicheraufgaben. Antwortzeit und Abbruch auf dem Referenzgerät prüfen; bei unvertretbarer Verzögerung vor einer anderen Strategie erneut abstimmen.

Der vorhandene Service speichert nur vor Beginn und beim Abschluss. Dieser begrenzte Eingriff in F01/F02 ist ausdrücklich im vorgeschlagenen AP09b-Umfang enthalten und mitfreizugeben.

## 6. Tests

- `SqliteNeustartTests`: Datei mit Angefordert, Laeuft samt Teiltext und allen Endzuständen vorbereiten; Store/Context schliessen, Initialisierung und Laden mit neuen Instanzen prüfen. Zweite Initialisierung darf Endzustände nicht verändern. Fünf Unterhaltungen einschliesslich leerer laden.
- `ChatServicePersistenzTests`: echte SQLite-Datei und kontrollierter Fake-Adapter; nach erstem ausgegebenen Teil über unabhängigen Context Text und Zustand prüfen, dann Fertig/Abgebrochen/Gestoert vergleichen. Vor und während Ausgabe abbrechen; gezielt verzögerte Teilspeicherung mit gleichzeitigem Abbruch darf Endzustand nicht zurücksetzen.
- Vorhandene Service-/Abbruchtests vollständig ausführen. Speicheraufrufzahlen nur gemäss neuer Speichergarantie anpassen, keine Erwartungen an Inhalt, Zustände oder Abbruch abschwächen.
- Manuell auf Referenzgerät: Start mit temporärer Testdatei, kontrollierter Neustart mit offenen Zuständen, Start ohne Modellserver, Container-Neustart mit eigenem Testdatenbestand und Logprüfung (N05). Erwartetes/tatsächliches Ergebnis protokollieren; UI-Nachweis folgt in F04.

Nach jedem Codeschritt `dotnet build`, `dotnet test`; abschliessend `dotnet format --verify-no-changes`. Datenbank-/Containerprüfungen nur mit Testdaten. Pflicht-Review für Startinitialisierung, DI und Synchronisation.

## 7. Grösse

Schätzung ein Halbtag auf geprüfter AP09a-Grundlage: Einbindung/Start, dauerhafte Teilspeicherung, Integrations-/Regressionstests. Karteninhaber bestätigt; bei grösserem Synchronisationsbedarf weiter aufteilen.

## 8. Board

Reihenfolge AP09a → AP09b → F04. Verantwortlicher, Termin und Status In Arbeit fehlen. READY nach Bestätigung dieser Angaben, des Eingriffs in F01/F02-Speicherung und grünen AP09a-Prüfungen. Gesamt-AP09 nicht aufgrund dieser Arbeitsgrundlage als erledigt markieren.

## Ergebnis

Vorbereitung vom 23.09.2026: Einbindung, Neustartverhalten, Persistenz während Streaming und Tests beschrieben. Keine Implementierung, Datenänderung oder Testausführung. Dokumentationsprüfung und Selbstprüfung siehe F04-Karte. Gruppenbestätigung, Umsetzung und Nachweise offen.
