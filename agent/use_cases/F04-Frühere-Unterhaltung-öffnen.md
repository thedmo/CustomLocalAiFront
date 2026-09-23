# Karte F04: Frühere Unterhaltung öffnen

**Status:** Arbeitsgrundlage ausgearbeitet; Blockiert für die Implementierung bis zur Bestätigung der vorgeschlagenen Entscheide und Board-Angaben sowie geprüfter AP09-Grundlage. Keine menschliche Freigabe vorweggenommen.

| Feld | Inhalt |
|---|---|
| Verantwortlich | PS, aus bisheriger Karte; zu bestätigen |
| Halbtag | Mittwochnachmittag, aus bisheriger Karte; Termin zu bestätigen |
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
- [ ] Eine gespeicherte leere Unterhaltung lässt sich mit leerem Verlauf öffnen; ohne gespeicherte Unterhaltungen bleibt die Liste leer.
- [ ] Die Anzeige übernimmt die durch die Persistenzgrundlage wiederhergestellten Zustände: Nach Neustart stehen zuvor `Angefordert` oder `Laeuft` gebliebene Antworten auf `Gestoert` mit Grund «Neustart»; bereits gespeicherter Text bleibt sichtbar.
- [ ] Auflisten und Öffnen funktionieren bei gestopptem Modellserver. Die Seite nutzt ausschliesslich `IChatService`, der Service den Store.
- [ ] Nach Neuanlegen und erster Nachricht stimmen Liste und gespeicherter Titel überein; die aktive Hervorhebung bleibt erhalten.
- [ ] Eine weitere Nachricht nach dem Öffnen verwendet genau die gewählte Kennung und erhält den bisherigen Verlauf.
- [ ] Während Streaming wird nicht gewechselt. Überlappende Ladevorgänge können keine falsche Auswahl anzeigen; unbekannte Kennungen führen zu einer verständlichen Meldung und lassen die Seite bedienbar.

### Vorgeschlagene Bedienentscheide zur Bestätigung

1. Neueste Unterhaltung zuerst (`ErstelltAm` absteigend, bei Gleichstand `Id`); Nachrichten nach `Zeit` aufsteigend, danach `Id` als stabile Zweitsortierung.
2. Seitenstart lädt nur die Liste, legt nichts an und wählt noch nichts aus. Senden bleibt bis zum Öffnen oder expliziten Neuanlegen deaktiviert. Browser-Neuladen erzeugt damit keine zusätzliche leere Unterhaltung.
3. Während Laden sind Auswahl, Neuanlegen und Senden vorübergehend gesperrt; die bestehende Wechselsperre während Streaming bleibt erhalten.

Diese Präzisierungen sind vor Implementierung zu bestätigen. Punkt 2 ändert bewusst die bisherige Initialisierung aus F01 und ihre Testvorbereitung, nicht den Sendeablauf.

## 5. Schnittstellen und Konfiguration

Die fachlichen Operationen aus Konzept 7.3 bleiben erhalten. Im C#-Vertrag fehlen sie noch; F04 ergänzt ausdrücklich folgende vorgeschlagene Signaturen:

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
| `ChatSeiteUnterhaltungenTests.cs` | Start ohne Neuanlegen; Titel/Datum/Sortierung; Auswahl; leerer Verlauf; gespeicherte Zustände samt Grund; aktive Hervorhebung; verzögertes Laden sperrt konkurrierende Aktionen; Fehler gibt Bedienung frei; Senden verwendet geöffnete Kennung |
| `ChatSeiteTests.cs` | Bestehende F01/F02/F03-Aussagen beibehalten; nötige Auswahl oder Neuanlage explizit im Arrange-Schritt |

Die Integration verwendet die temporäre Testdatenbank-Hilfe aus AP09a, keine produktiven Daten und keinen In-Memory-Ersatz für den Neustartnachweis. Neustart sowohl der Anwendung als auch des Containers manuell prüfen; Tastaturbedienung und beide Projekt-Fensterbreiten prüfen (N03). Ergebnisse erst nach tatsächlicher Durchführung protokollieren.

Nach jedem Implementierungsschritt `dotnet build`, `dotnet test`; abschliessend `dotnet format --verify-no-changes`.

## 7. Grösse

Ziel: höchstens ein Halbtag auf vorhandener Chat-Seite und SQLite-Persistenz. Vor READY Aufwand bestätigen; fehlende Grundlagen separat umsetzen.

## 8. Board

Reihenfolge: **AP09a → AP09b → F04**. Board wurde nicht geändert. Verantwortlichkeit, Termin, Status In Arbeit und Aufwand durch die Gruppe bestätigen.

| Definition of Ready | Stand |
|---|---|
| 1 Ziel | beschrieben |
| 2 Abgrenzung | beschrieben, vorhandene F03 berücksichtigt |
| 3 Dateien | konkrete Dateien und betroffene Tests benannt |
| 4 Akzeptanzkriterien | prüfbar beschrieben; Bedienentscheide zur Bestätigung |
| 5 Schnittstellen | konkrete Ergänzungen vorgeschlagen |
| 6 Tests | automatisiert und manuell zugeordnet |
| 7 Grösse | ein Halbtag geschätzt; Bestätigung offen |
| 8 Board | Bestätigung durch Gruppe offen |

READY erst nach Bestätigung der offenen Entscheide und DoR-Angaben. Implementierung auf geprüfter AP09-Grundlage. Review und manuelle Nachweise gehören zur späteren Definition of Done und sind noch nicht erfüllt.

---

## Ergebnis

Karte: F04, Vorbereitung vom 23.09.2026

Umgesetzt: Arbeitsgrundlage mit Ist-Abgleich, aktuellen Pfaden, konkreten Verträgen, Tests, Abhängigkeiten und offenen Freigaben. Kein Anwendungscode implementiert.

Geänderte Dateien dieses Vorbereitungsauftrags: diese Karte, neue Karten `AP09a-sqlite-grundlage.md` und `AP09b-sqlite-einbindung.md`, `agent/Drittkomponenten.md` (Vorschläge), `agent/protokolle/KI-Einsatz.md`.

Nicht angefasst: Anwendungscode, bestehende Tests, Konfiguration, Projektdateien, Masterkonzept, Board und Nutzdaten.

Prüfungen: Dokumente gegen Code, Schnittstellen, Design und Testfälle abgeglichen; Diff und `git diff --check`. Build, Tests und Formatierung nicht ausgeführt, da ausschliesslich Arbeitsgrundlagen geändert wurden. Keine funktionale Abnahme behauptet.

Selbstprüfung nach `agent/Review_Checkliste.md`, in deren Reihenfolge, für diesen Dokumentationsschritt:

- Fachlichkeit 1: ja, F04 und Z05 bleiben Ziel. 2: nein, Implementierung/Abnahme folgen. 3: ja, Präzisierungen als Vorschläge markiert.
- Codequalität 1: ja, Begriffe abgeglichen. 2: ja, geplanter Zugriff über Service. 3: ja, Umfang auf Teilkarten verteilt. 4: nein, keine Codeformatprüfung; Dokument-Diff geprüft. 5: ja, kein Code auskommentiert.
- Fehlerbehandlung 1: nein, noch keine Ausführung. 2: nein, Abbruch unverändert und Regression geplant. 3: nein, Eingabeprüfung unverändert, nicht erneut getestet. 4: nein, Wiederherstellung erst geplant.
- Auswirkungen 1: ja, nur beauftragte Arbeitsgrundlagen. 2: ja, keine freigegebenen Verträge geändert, Ergänzungen vorgeschlagen. 3: ja, vorhandene Beispielpfade geprüft.
- Sicherheit 1: ja, keine Nutzdaten, Geheimnisse oder persönlichen Pfade ergänzt. 2: ja, keine Anwendungs-Netzwerkaufrufe ergänzt. 3: ja, Abhängigkeiten mit offenem Freigabestatus dokumentiert, nichts installiert. 4: ja, Trennung von Systemanweisung und Eingabe unverändert.
- Oberfläche 1: nein, noch keine UI umgesetzt. 2: nein, CSS unverändert. 3: nein, visuelle Prüfung für F04 eingeplant.
- Tests 1: nein, kein Testlauf bei reiner Dokumentation. 2: nein, keine neue C#-Logik, Testfälle geplant. 3: nein, manuelle Nachweise offen. 4: ja, Einschränkungen dokumentiert.
- Nachvollziehbarkeit 1: ja, KI-Einsatz ergänzt. 2: nein, kein Commit beauftragt; Vorschlag unten.

Offen: Gruppenbestätigung, AP09-Implementierung, funktionale Prüfungen und menschliches Review.

Vorschlag Commit-Nachricht: `F04: Arbeitsgrundlage und AP09-Teilschritte konkretisieren` / `KI: Codex, geprueft von XX`.
