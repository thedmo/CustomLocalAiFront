# Karte F04: Frühere Unterhaltung öffnen

**Status:** Implementiert und automatisiert geprüft; bereit für menschliches Review. Visuelle Abnahme offen, nicht als Erledigt freigegeben.

| Feld | Inhalt |
|---|---|
| Verantwortlich | PS |
| Halbtag | Mi PM |
| Referenz | F04; Z05, M08; NF06 Persistenz |
| Quellen | `agent/Use_Cases.md` F04 und Z05; `agent/Design.md` Oberfläche, Datenhaltung und «Kennung, Löschung, Neustart»; `agent/Schnittstellen.md` |
| Testfälle | T05, Z05; T01 bis T04 als Regression; N03 für die Oberfläche |
| Abhängigkeiten | `AP09a-sqlite-grundlage.md`, danach `AP09b-sqlite-einbindung.md`; vorhandene F03-Oberfläche |

## 1. Ziel (ein Satz)

Der Benutzer öffnet eine gespeicherte Unterhaltung aus der Liste und sieht ihre Nachrichten und Antworten in der richtigen Reihenfolge, auch nach einem Neustart der Anwendung.

## 2. Abgrenzung (gehört nicht dazu)

F04 umfasst das Auflisten und Laden gespeicherter Unterhaltungen aus SQLite sowie deren Anzeige. AP09 liefert dafür die gemeinsame Persistenzgrundlage: EF Core, Datenbankschema, Migrationen, Speicherung und Wiederherstellung der Antwortzustände beim Neustart. Diese Grundlage muss vor der Abnahme von F04 verfügbar sein.

F03 ist laut Benutzer bereits umgesetzt. Ihre vorhandene senkrechte Liste, aktive Hervorhebung und der Button bleiben Grundlage. F03 und F04 werden nacheinander auf demselben Stand umgesetzt und verwenden denselben Store. Nachrichten senden und abbrechen (F01/F02), Löschen (F05) und Dateiprotokoll (AP12/F08) bleiben eigene Aufgaben; die Synchronisation der Anzeige mit dem gespeicherten Stand gehört zu F04.

**Ist-Abgleich vom 23.09.2026:** `Home.razor` hält Liste und sichtbare Verläufe bisher im Komponentenzustand und legt beim Start eine neue Unterhaltung an. `Program.cs` registriert `ArbeitsspeicherStore`; ein SQLite-Projekt fehlt. Auch die fachlich dokumentierten Listen- und Öffnungsoperationen fehlen noch in den C#-Verträgen. Die vorhandene F03-Oberfläche belegt deshalb noch keine Persistenz über einen Neustart.

## 3. Betroffene Komponenten und Dateien

- `src/LocalAiFront/Components/Pages/Home.razor`: Liste laden, Auswahl asynchron öffnen, gespeicherten Verlauf samt Zuständen anzeigen; leere und ladende Ansicht absichern.
- `src/LocalAiFront/Components/Pages/Home.razor.css`: bei Bedarf Datum und Zustandsanzeige im bestehenden Stil ergänzen.
- `src/Chat.Core/IChatService.cs`, `src/Chat.Core/ChatService.cs`: dokumentierte Listen- und Öffnungsoperation ergänzen und an `IStore` delegieren.
- `tests/Chat.Tests/FakeChatService.cs`: Liste, Laden, Verzögerung und Aufrufzähler ergänzen.
- `tests/Chat.Tests/ChatSeiteTests.cs`: Testvorbereitung an den ausdrücklich vorgeschlagenen Seitenstart anpassen; bestehende Aussagen zu F01/F02/F03 erhalten.
- Neu `tests/Chat.Tests/ChatSeiteUnterhaltungenTests.cs` und `ChatServiceUnterhaltungenTests.cs`: F04-Nachweise.
- Diese Karte, `agent/Schnittstellen.md`, `agent/protokolle/KI-Einsatz.md`, `agent/protokolle/Test_und_Reviewprotokoll.md`: Verträge, Ergebnis und Prüfungen nachführen.

Store, Projektdateien und Startinitialisierung sind den AP09-Karten zugeordnet. Adapter, `Konfiguration.cs`, `global.json` und Docker-Konfiguration bleiben ausserhalb von F04. Die Pfade in `src/LocalAiFront/appsettings.example.json` passen bereits zum Mount und brauchen keine Änderung.

## 4. Akzeptanzkriterien

**Vorbedingung:** Die Anwendung läuft und die lokale SQLite-Datenbank ist verfügbar; für das Öffnen ist mindestens eine Unterhaltung gespeichert.

**Ablauf:** Beim Start lädt die Chat-Seite über den ChatService die Unterhaltungsliste aus SQLite. Der Benutzer wählt einen Eintrag. Der ChatService lädt anhand der Kennung den zugehörigen Verlauf über den Store; die Seite zeigt die Nachrichten und Antworten an.

- [ ] Gespeicherte Unterhaltungen erscheinen mit Titel und Datum; die Sortierung verwendet `ErstelltAm`.
- [ ] Die Auswahl lädt genau die gewählte Unterhaltung: Nachrichten nach `Zeit`, jeweils mit zugehöriger Antwort und gespeichertem Antwortzustand (T05).
- [ ] Nach Neustart der Anwendung beziehungsweise des Containers sind fünf zuvor in SQLite gespeicherte Unterhaltungen weiterhin gelistet und mit ihrem Verlauf einzeln zu öffnen (Z05, Anteil F04).
- [ ] Eine gespeicherte leere Unterhaltung lässt sich mit leerem Verlauf öffnen; der Store liefert bei leerer Datenbank eine leere Liste; die Seite ergänzt beim Start die neue leere Unterhaltung.
- [ ] Die Anzeige übernimmt die durch die Persistenzgrundlage wiederhergestellten Zustände: Nach Neustart stehen zuvor `Angefordert` oder `Laeuft` gebliebene Antworten auf `Gestoert` mit Grund «Neustart»; bereits gespeicherter Text bleibt sichtbar.
- [ ] Auflisten und Öffnen funktionieren bei gestopptem Modellserver. Die Seite nutzt ausschliesslich `IChatService`, der Service den Store.
- [ ] Nach Neuanlegen und erster Nachricht stimmen Liste und gespeicherter Titel überein; die aktive Hervorhebung bleibt erhalten.
- [ ] Eine weitere Nachricht nach dem Öffnen verwendet genau die gewählte Kennung und erhält den bisherigen Verlauf.
- [ ] Während Streaming wird nicht gewechselt. Überlappende Ladevorgänge können keine falsche Auswahl anzeigen; unbekannte Kennungen führen zu einer verständlichen Meldung und lassen die Seite bedienbar.

### Durch PS bestätigte Bedienentscheide

1. Neueste Unterhaltung zuerst (`ErstelltAm` absteigend, bei Gleichstand `Id`); Nachrichten nach `Zeit` aufsteigend, danach `Id` als stabile Zweitsortierung.
2. Seitenstart legt genau eine neue leere Unterhaltung an und aktiviert sie; zusätzlich lädt er die Liste der gespeicherten Unterhaltungen. Prerendering darf keine zweite Unterhaltung erzeugen.
3. Während Laden sind Auswahl, Neuanlegen und Senden vorübergehend gesperrt; die bestehende Wechselsperre während Streaming bleibt erhalten.

PS hat diese Präzisierungen bestätigt und den Seitenstart ausdrücklich auf eine neue aktive leere Unterhaltung mit gespeicherter Liste festgelegt. Vorhandene F01/F02/F03-Testaussagen bleiben erhalten.

## 5. Schnittstellen und Konfiguration

Die fachlichen Operationen aus Konzept 7.3 bleiben erhalten. F04 ergänzt die bisher fehlenden C#-Signaturen wie freigegeben:

```csharp
Task<IReadOnlyList<UnterhaltungInfo>> ListeUnterhaltungenAsync(CancellationToken ct);
Task<Unterhaltung> OeffneUnterhaltungAsync(Guid id, CancellationToken ct);
```

AP09a stellt `UnterhaltungInfo` mit `Id`, `Titel`, `ErstelltAm` und `IStore.ListeAsync(ct)` bereit. Der Service delegiert an diese Operation beziehungsweise `LadenAsync(id, ct)`, ohne Modellaufruf oder Prüfung der Modellserver-Erreichbarkeit. Unbekannte Kennungen verwenden die bereits im Store vorhandene `KeyNotFoundException`, die F04 in der Oberfläche behandelt; kein neuer Modell-Störfall.

**Datenhaltung:** `SqliteStore` liest über Entity Framework Core die Tabellen `Unterhaltung`, `Nachricht` und `Antwort`. SQLite ist die alleinige Quelle des gespeicherten Verlaufs; im Browser wird kein Verlauf dauerhaft gespeichert. Es ist kein separater Datenbankserver nötig.

**Speicherort:** `Chat:SpeicherortDb` wird aus der Anwendungskonfiguration oder `Chat__SpeicherortDb` gelesen. Die Eigenschaft in `Konfiguration.cs` allein legt noch keine Datenbank an. Gemäss `docker_compose.yml` liegt das persistente Verzeichnis im Container unter `/app/data` und auf dem Referenzgerät im Projektordner `data/`. Die Dateipfade in `appsettings.example.json` müssen dazu passen. Die technische Protokolldatei liegt getrennt von der SQLite-Datei im selben eingebundenen Verzeichnis.

## 6. Tests

- **Automatisiert (T05):** Zwei Unterhaltungen in einer temporären SQLite-Testdatei speichern, auflisten und gezielt die zweite laden; Kennung, Nachrichtenreihenfolge, Antwortzuordnung und Zustände prüfen. Store und DbContext schliessen und neu erzeugen, dann dieselbe Datei erneut öffnen. Leere Liste, leere Unterhaltung und wiederhergestellte Neustart-Zustände ergänzend prüfen; kein Modellaufruf beim Auflisten oder Öffnen. Keine produktiven Daten verwenden.
- **Manuell (T05/Z05):** Fünf Unterhaltungen vorbereiten, Anwendung neu starten, alle über die Liste öffnen und mit dem gespeicherten Stand vergleichen. Öffnen zusätzlich bei gestopptem Modellserver prüfen. Erwartetes und tatsächliches Ergebnis bei Umsetzung im Test- und Reviewprotokoll festhalten.

Konkrete Testzuordnung:

| Datei | Nachweise |
|---|---|
| `ChatServiceUnterhaltungenTests.cs` | Zweite von zwei Unterhaltungen gezielt laden; Nachrichtenreihenfolge und Antwortzuordnung; fünf Unterhaltungen nach Neuöffnen derselben SQLite-Datei; leere Liste/leere Unterhaltung; unbekannte Kennung; Cancellation; Modell-Fake darf nie aufgerufen werden |
| `ChatSeiteUnterhaltungenTests.cs` | Start mit genau einer neuen aktiven leeren Unterhaltung und vorhandener Liste; Titel/Datum/Sortierung; Auswahl; leerer Verlauf; gespeicherte Zustände samt Grund; aktive Hervorhebung; verzögertes Laden sperrt konkurrierende Aktionen; Fehler gibt Bedienung frei; Senden verwendet geöffnete Kennung |
| `ChatSeiteTests.cs` | Bestehende F01/F02/F03-Aussagen beibehalten; nötige Auswahl oder Neuanlage explizit im Arrange-Schritt |

Die Integration verwendet die temporäre Testdatenbank-Hilfe aus AP09a, keine produktiven Daten und keinen In-Memory-Ersatz für den Neustartnachweis. Neustart sowohl der Anwendung als auch des Containers manuell prüfen; Tastaturbedienung und beide Projekt-Fensterbreiten prüfen (N03). Ergebnisse erst nach tatsächlicher Durchführung protokollieren.

Nach jedem Implementierungsschritt `dotnet build`, `dotnet test`; abschliessend `dotnet format --verify-no-changes`.

## 7. Grösse

Geplant und durch PS bestätigt: Mi PM, höchstens ein Halbtag auf vorhandener Chat-Seite und SQLite-Persistenz. Die Grundlagen wurden in AP09a/AP09b umgesetzt; effektiven Aufwand trägt PS nach.

## 8. Board und Freigabe

PS hat AP09a/AP09b und F04 am 23.09.2026 technisch freigegeben und die Umsetzung beauftragt; Verantwortlicher PS, Halbtag Mi PM. Die Karten wurden nacheinander umgesetzt. F04 startet nach ausdrücklichem Benutzerentscheid mit einer neuen aktiven leeren Unterhaltung und der gespeicherten Liste. Am 24.09. wurde die Wiederherstellung des ausgefallenen Hauptcontainers zusätzlich beauftragt. Ein externer Board-Eintrag wurde nicht durch das Werkzeug geändert; Review und Erledigt entscheidet die Gruppe.

---

## Ergebnis

Karte: F04 Frühere Unterhaltung öffnen

Umgesetzt:
- Beim Start genau eine neue leere aktive Unterhaltung, dazu die gespeicherte Liste mit Titel/Datum, neueste zuerst. Prerendering deaktiviert, um doppelte Neuanlage zu verhindern.
- Auswahl lädt den SQLite-Verlauf über IChatService; gespeicherte Antwortzustände und Neustartgrund sichtbar. Leerer Verlauf, unbekannte Kennung und konkurrierende Ladeaktionen berücksichtigt.
- Fortsetzen einer geöffneten Unterhaltung erhält den vorhandenen Verlauf. F03-Gestaltung, Streaming und Abbruch bleiben nutzbar.

Geänderte Dateien:
- `src/Chat.Core/IChatService.cs`, `ChatService.cs`, `src/LocalAiFront/Components/Pages/Home.razor`.
- `tests/Chat.Tests/FakeChatService.cs`, `ChatSeiteUnterhaltungenTests.cs`, `ChatServiceUnterhaltungenTests.cs`; ChatSeiteTests.cs ausschliesslich Zeilenenden.
- `agent/Schnittstellen.md`, Karte und Protokolle; Voraussetzungen durch AP09a/AP09b.

Nicht angefasst (bewusst): Adapter, Löschen F05, Dateiprotokoll AP12, Modellkonfiguration; Home.razor.css unverändert.

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

Vorschlag Commit-Nachricht: F04: SQLite-Persistenz und gespeicherte Unterhaltungen bereitstellen / KI: Codex, geprueft von XX
