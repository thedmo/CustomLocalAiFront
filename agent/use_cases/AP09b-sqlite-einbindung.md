# Karte AP09b: Anwendung nutzt SQLite und stellt Antworten nach Neustart wieder her

**Status:** Implementiert und automatisiert geprüft; bereit für menschliches Review. Visuelle Abnahme offen, nicht als Erledigt freigegeben.

| Feld | Inhalt |
|---|---|
| Verantwortlich | PS |
| Halbtag | Mi PM |
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
- `docker_compose.yml`: gemäss Fortsetzungsauftrag von PS am 24.09.2026 den SQLite-Pfad `/app/data/chat.db` explizit als `Chat__SpeicherortDb` übergeben. Die Beispielkonfiguration wird nicht automatisch geladen; diese Ergänzung behebt den gemeldeten Container-Startfehler.
- Neu `src/Chat.Store.Sqlite/SqliteInitialisierung.cs`: Schema anwenden und offene Antworten einmal beim Prozessstart wiederherstellen, niemals bei jedem Laden oder Circuit-Start.
- `src/Chat.Store.Sqlite/SqliteStore.cs`, `ChatDbContext.cs`: nur notwendige Einbindung von Initialisierung und laufender Speicherung.
- `src/Chat.Core/ChatService.cs`, `AktiveAnfrage.cs`: konsistente Speicherung von angefordertem Zustand, empfangenen Teilen und Endzustand; Abschluss-/Abbruchsynchronisation erhalten.
- Neu `tests/Chat.Tests/SqliteNeustartTests.cs`, `ChatServicePersistenzTests.cs`; Testaufbau in `SqliteTestdatenbank.cs` aus AP09a.
- `tests/Chat.Tests/ChatServiceTests.cs`, `ChatServiceAbbruchTests.cs`: nur nötige Annahmen über zusätzliche Speicheraufrufe anpassen; fachliche Aussagen erhalten.
- `README.md`: initiale Datenbankanlage, konfigurierte Pfade und lokaler Start dokumentieren.
- Diese Karte, `agent/protokolle/KI-Einsatz.md`, `agent/protokolle/Test_und_Reviewprotokoll.md`.

Unberührt: Adapter, `IModelServerClient`, Modellparameter, `Konfiguration.cs`, `appsettings*.json`, `global.json`. Bestehender Docker-Mount bleibt erhalten; keine direkten Eingriffe in bestehende Datenbanken oder technische Protokolle durch das Werkzeug.

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

**Durch PS freigegebene Speicherstrategie:** Jeden angenommenen Textteil vor Weitergabe dauerhaft speichern. Das erhält bei Prozessabbruch die bereits ausgegebenen Teile. Abschluss und Teilspeicherung sind pro aktiver Anfrage geordnet. Antwortzeit und Abbruch auf dem Referenzgerät mit echtem Modell messen; bei unvertretbarer Verzögerung vor einer anderen Strategie erneut abstimmen.

Vor AP09 speicherte der Service nur vor Beginn und beim Abschluss. Der begrenzte Eingriff in F01/F02 wurde mit AP09b ausdrücklich freigegeben und ist umgesetzt.

## 6. Tests

- `SqliteNeustartTests`: Datei mit Angefordert, Laeuft samt Teiltext und allen Endzuständen vorbereiten; Store/Context schliessen, Initialisierung und Laden mit neuen Instanzen prüfen. Zweite Initialisierung darf Endzustände nicht verändern. Fünf Unterhaltungen einschliesslich leerer laden.
- `ChatServicePersistenzTests`: echte SQLite-Datei und kontrollierter Fake-Adapter; nach erstem ausgegebenen Teil über unabhängigen Context Text und Zustand prüfen, dann Fertig/Abgebrochen/Gestoert vergleichen. Vor und während Ausgabe abbrechen; gezielt verzögerte Teilspeicherung mit gleichzeitigem Abbruch darf Endzustand nicht zurücksetzen.
- Vorhandene Service-/Abbruchtests vollständig ausführen. Speicheraufrufzahlen nur gemäss neuer Speichergarantie anpassen, keine Erwartungen an Inhalt, Zustände oder Abbruch abschwächen.
- Manuell auf Referenzgerät: Start mit temporärer Testdatei, kontrollierter Neustart mit offenen Zuständen, Start ohne Modellserver, Container-Neustart mit eigenem Testdatenbestand und Logprüfung (N05). Erwartetes/tatsächliches Ergebnis protokollieren; UI-Nachweis folgt in F04.

Nach jedem Codeschritt `dotnet build`, `dotnet test`; abschliessend `dotnet format --verify-no-changes`. Datenbank-/Containerprüfungen nur mit Testdaten. Pflicht-Review für Startinitialisierung, DI und Synchronisation.

## 7. Grösse

Geplant und durch PS bestätigt: Mi PM, ein Halbtag auf geprüfter AP09a-Grundlage für Einbindung/Start, dauerhafte Teilspeicherung und Integrations-/Regressionstests. Effektiven Aufwand trägt PS nach.

## 8. Board und Freigabe

PS hat AP09a/AP09b und F04 am 23.09.2026 technisch freigegeben und die Umsetzung beauftragt; Verantwortlicher PS, Halbtag Mi PM. Die Karten wurden nacheinander umgesetzt. F04 startet nach ausdrücklichem Benutzerentscheid mit einer neuen aktiven leeren Unterhaltung und der gespeicherten Liste. Am 24.09. wurde die Wiederherstellung des ausgefallenen Hauptcontainers zusätzlich beauftragt. Ein externer Board-Eintrag wurde nicht durch das Werkzeug geändert; Review und Erledigt entscheidet die Gruppe.

## Ergebnis

Karte: AP09b Anwendung nutzt SQLite und stellt Antworten nach Neustart wieder her

Umgesetzt:
- DI verwendet SQLite; Migration und Wiederherstellung laufen einmal vor Annahme von Benutzeranfragen.
- Antwortteile werden vor Ausgabe gespeichert; eine gemeinsame asynchrone Sperre ordnet Teilspeicherung und Abschluss.
- Gemeldeten Startfehler vom 24.09. behoben: Compose übergibt den Speicherpfad explizit. appsettings.example.json wird nicht automatisch geladen. Kein Fallback oder unterdrückter Fehler.

Geänderte Dateien:
- `src/LocalAiFront/Program.cs`, `LocalAiFront.csproj`, `Dockerfile`, `docker_compose.yml` (nur Speicherpfad).
- `src/Chat.Store.Sqlite/SqliteInitialisierung.cs`, `src/Chat.Core/ChatService.cs`, `AktiveAnfrage.cs`.
- `tests/Chat.Tests/SqliteNeustartTests.cs`, `ChatServicePersistenzTests.cs`; ChatServiceTests.cs ausschliesslich Zeilenenden.
- `README.md`, Karte und Protokolle; gemeinsame Design-/Schnittstellendokumentation unter AP09a/F04.

Nicht angefasst (bewusst): Adapter, Modellparameter, appsettings-Dateien, bestehender Daten-Mount; keine manuellen Eingriffe in Nutzdaten.

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

Vorschlag Commit-Nachricht: AP09b: SQLite-Persistenz und gespeicherte Unterhaltungen bereitstellen / KI: Codex, geprueft von XX
