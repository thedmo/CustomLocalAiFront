# Karte F04: Frühere Unterhaltung öffnen

**Status:** Entwurf, noch nicht READY. Konkrete Umsetzungsdateien, Board-Eintrag und Abstimmung mit F03 sind vor der Umsetzung festzulegen.

| Feld | Inhalt |
|---|---|
| Verantwortlich | Pascal Schmidiger |
| Halbtag | Mittwochnachmittag |
| Referenz | F04; Z05, M08; NF06 Persistenz |
| Quellen | `agent/Use_Cases.md` F04 und Z05; `agent/Design.md` Oberfläche, Datenhaltung und «Kennung, Löschung, Neustart»; `agent/Schnittstellen.md` |
| Testfälle | T05 aus `agent/Testfaelle.md`; Neustart-Nachweis aus Z05 |

## 1. Ziel (ein Satz)

Der Benutzer öffnet eine gespeicherte Unterhaltung aus der Liste und sieht ihre Nachrichten und Antworten in der richtigen Reihenfolge, auch nach einem Neustart der Anwendung.

## 2. Abgrenzung (gehört nicht dazu)

F04 umfasst das Auflisten und Laden gespeicherter Unterhaltungen aus SQLite sowie deren Anzeige. AP09 liefert dafür die gemeinsame Persistenzgrundlage: EF Core, Datenbankschema, Migrationen, Speicherung und Wiederherstellung der Antwortzustände beim Neustart. Diese Grundlage muss vor der Abnahme von F04 verfügbar sein.

Neue Unterhaltung anlegen (F03, parallele Arbeit), Nachrichten senden und abbrechen (F01/F02) und Löschen (F05) bleiben eigene Aufgaben. F03 und F04 verwenden denselben Store und dieselbe Datenbank.

## 3. Betroffene Komponenten und Dateien

- `web_app/LocalAiFront` (Chat.Web): Chat-Seite, Unterhaltungsliste und Verlauf.
- `src/Chat.Core/ChatService.cs`: Liste und Öffnen über `IStore`.
- `src/Chat.Store.Sqlite/SqliteStore.cs` (Grundlage aus AP09): Leseoperationen für Liste und Verlauf nutzen beziehungsweise ergänzen; kein zweiter Store.
- `web_app/LocalAiFront/appsettings.example.json`: Speicherpfade an den vorhandenen Docker-Mount angleichen.
- `tests/Chat.Tests`: Tests für F04; konkrete Testdateien vor READY festlegen.

Gemeinsam mit F03 genutzte Dateien und die konkreten Razor-Dateien vor READY abstimmen. Adapter, `Konfiguration.cs`, Projektdateien und Docker-Konfiguration bleiben unverändert.

## 4. Akzeptanzkriterien

**Vorbedingung:** Die Anwendung läuft und die lokale SQLite-Datenbank ist verfügbar; für das Öffnen ist mindestens eine Unterhaltung gespeichert.

**Ablauf:** Beim Start lädt die Chat-Seite über den ChatService die Unterhaltungsliste aus SQLite. Der Benutzer wählt einen Eintrag. Der ChatService lädt anhand der Kennung den zugehörigen Verlauf über den Store; die Seite zeigt die Nachrichten und Antworten an.

- [ ] Gespeicherte Unterhaltungen erscheinen mit Titel und Datum; die Sortierung verwendet `ErstelltAm`.
- [ ] Die Auswahl lädt genau die gewählte Unterhaltung: Nachrichten nach `Zeit`, jeweils mit zugehöriger Antwort und gespeichertem Antwortzustand (T05).
- [ ] Nach Neustart der Anwendung beziehungsweise des Containers sind fünf zuvor in SQLite gespeicherte Unterhaltungen weiterhin gelistet und mit ihrem Verlauf einzeln zu öffnen (Z05, Anteil F04).
- [ ] Eine gespeicherte leere Unterhaltung lässt sich mit leerem Verlauf öffnen; ohne gespeicherte Unterhaltungen bleibt die Liste leer.
- [ ] Die Anzeige übernimmt die durch die Persistenzgrundlage wiederhergestellten Zustände: Nach Neustart stehen zuvor `Angefordert` oder `Laeuft` gebliebene Antworten auf `Gestoert` mit Grund «Neustart»; bereits gespeicherter Text bleibt sichtbar.
- [ ] Auflisten und Öffnen funktionieren bei gestopptem Modellserver. Die Seite nutzt ausschliesslich `IChatService`, der Service den Store.

## 5. Schnittstellen und Konfiguration

`IChatService.ListeUnterhaltungenAsync(ct)` und `OeffneUnterhaltungAsync(id, ct)` verwenden `IStore.ListeAsync(ct)` und `LadenAsync(id, ct)`. Die Schnittstellen bleiben unverändert.

**Datenhaltung:** `SqliteStore` liest über Entity Framework Core die Tabellen `Unterhaltung`, `Nachricht` und `Antwort`. SQLite ist die alleinige Quelle des gespeicherten Verlaufs; im Browser wird kein Verlauf dauerhaft gespeichert. Es ist kein separater Datenbankserver nötig.

**Speicherort:** `Chat:SpeicherortDb` wird aus der Anwendungskonfiguration oder `Chat__SpeicherortDb` gelesen. Die Eigenschaft in `Konfiguration.cs` allein legt noch keine Datenbank an. Gemäss `docker_compose.yml` liegt das persistente Verzeichnis im Container unter `/app/data` und auf dem Referenzgerät im Projektordner `data/`. Die Dateipfade in `appsettings.example.json` müssen dazu passen. Die technische Protokolldatei liegt getrennt von der SQLite-Datei im selben eingebundenen Verzeichnis.

## 6. Tests

- **Automatisiert (T05):** Zwei Unterhaltungen in einer temporären SQLite-Testdatei speichern, auflisten und gezielt die zweite laden; Kennung, Nachrichtenreihenfolge, Antwortzuordnung und Zustände prüfen. Store und DbContext schliessen und neu erzeugen, dann dieselbe Datei erneut öffnen. Leere Liste, leere Unterhaltung und wiederhergestellte Neustart-Zustände ergänzend prüfen; kein Modellaufruf beim Auflisten oder Öffnen. Keine produktiven Daten verwenden.
- **Manuell (T05/Z05):** Fünf Unterhaltungen vorbereiten, Anwendung neu starten, alle über die Liste öffnen und mit dem gespeicherten Stand vergleichen. Öffnen zusätzlich bei gestopptem Modellserver prüfen. Erwartetes und tatsächliches Ergebnis bei Umsetzung im Test- und Reviewprotokoll festhalten.

## 7. Grösse

Ziel: höchstens ein Halbtag auf vorhandener Chat-Seite und SQLite-Persistenz. Vor READY Aufwand bestätigen; fehlende Grundlagen separat umsetzen.

## 8. Board

[Board-Eintrag vor Umsetzung ergänzen: Pascal Schmidiger, Mittwochnachmittag.] Abhängigkeit zu AP09 und gemeinsame Dateien mit F03 abstimmen; erst danach READY prüfen.

---

## Ergebnis

Kartenentwurf mit Vorlage, Design, Schnittstellen und T05 abgeglichen; SQLite-Datenhaltung und Abgrenzung zu AP09 konkretisiert. Speicherpfade in `appsettings.example.json` an den bestehenden Docker-Mount angeglichen. Keine Implementierung von F04; Akzeptanzkriterien und Definition of Done sind noch offen. Build, Tests und Formatierung nicht ausgeführt (Dokumentation und Beispielkonfiguration). KI beteiligt: Codex, Formulierung und Quellenabgleich. Review und Freigabe durch ein Gruppenmitglied stehen aus.
