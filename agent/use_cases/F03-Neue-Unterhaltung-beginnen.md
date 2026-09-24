# Karte F03: Neue Unterhaltung beginnen

**Status:** Umgesetzt; durch PS am 24.09.2026 präzisiert: Ein leerer Entwurf wird erst mit der ersten gesendeten Nachricht zur gespeicherten Unterhaltung. Automatisiert geprüft, manuelle Abnahme und menschliches Review offen.

| Feld           | Inhalt                                                                                                                                                                                          |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Verantwortlich | [BH]                                                                                                                                                                                            |
| Halbtag        | [Di PM]                                                                                                                                                                                         |
| Referenz       | F03 Neue Unterhaltung beginnen; Z05; M08                                                                                                                                                        |
| Quellen        | `agent/Use_Cases.md` Funktion F03, NF06 Persistenz, Ziel Z05; `agent/Design.md` Unterhaltungen, Store und Zustandsmodell; `agent/Schnittstellen.md` `IStore`, `IChatService` und `Unterhaltung` |
| Testfälle      | T01, T05, T06 aus `agent/Testfaelle.md`                                                                                                                                                         |

## 1. Ziel (ein Satz)

Der Benutzer startet einen neuen leeren Entwurf; erst beim Senden der ersten Nachricht entsteht die gespeicherte Unterhaltung und erscheint in der Liste, während bestehende Unterhaltungen unverändert bleiben.

## 2. Abgrenzung (gehört nicht dazu)

Keine Antwortgenerierung oder Streaming (F01), kein Abbruch einer laufenden Antwort (F02), kein Öffnen bzw. Löschen einer bestehenden Unterhaltung (F04/F05), keine Änderung der Modellkonfiguration (F07) und kein Protokoll mit Chattext (F08).

## 3. Betroffene Komponenten und Dateien

- `src/LocalAiFront`: Komponenten für die Chat-Ansicht und den Button „Neue Unterhaltung“
- Neu `src/LocalAiFront/Components/Pages/Home.Entwurf.cs`: Entwurfslogik als Teil derselben Komponente, damit `Home.razor` unter 300 Zeilen bleibt.
- Neu `tests/Chat.Tests/ChatSeitePersistenzTests.cs`: Seitenstart und erste Nachricht mit echter temporärer SQLite-Datei.
- `src/Chat.Core`: `Unterhaltung.cs`, `IStore.cs`, `IChatService.cs`, ggf. `ChatService.cs` für die Verwaltung der aktuellen Unterhaltung
- `src/Chat.Core/Modelle`: `Unterhaltung`, `Nachricht`, `Antwort` und damit verbundene Zustände
- Unberührt: Model-Runner-Adapter, Streaming und Antwortlogik aus F01/F02, Docker-Compose und Konfigurationsdateien

## 4. Akzeptanzkriterien (prüfbar, vorher festgelegt; beim Review abhaken)

- [x] Beim Laden der Seite und über „Neue Unterhaltung“ entsteht ein aktiver leerer Entwurf, aber noch kein Listeneintrag und kein Datenbankdatensatz.
- [ ] Bestehende Unterhaltungen bleiben nach dem Starten einer neuen Unterhaltung erhalten und sind weiterhin sichtbar.
- [ ] Die Unterhaltungen werden in einer senkrechten Liste dargestellt; alle Einträge haben das gleiche Button-Styling wie die Senden-Schaltfläche.
- [ ] Eine gespeicherte aktive Unterhaltung wird optisch hervorgehoben; während des ungespeicherten Entwurfs ist kein Listeneintrag aktiv.
- [x] Beim ersten gültigen Senden wird die Unterhaltung mit der Nachricht lokal gespeichert, sofort in der Liste angezeigt und bleibt nach einem Neustart verfügbar.
- [x] Wird vor der ersten Nachricht neu geladen oder erneut „Neue Unterhaltung“ gewählt, bleibt kein leerer Datensatz zurück. Scheitert das erste Senden vor der gemeinsamen Speicherung, wird die nur im Service reservierte Kennung verworfen.
- [ ] Ein weiterer Wechsel zwischen alten und neuen Unterhaltungen funktioniert, ohne dass Daten verloren gehen oder überschrieben werden.
- [x] Beim öffnen der Seite soll eine neue leere Unterhaltung angezeigt werden, die noch nicht in der Liste gespeichert ist. Erst beim Senden der ersten Nachricht wird die Unterhaltung in der Liste angezeigt und in der Datenbank gespeichert.

## 5. Schnittstellen und Konfiguration

Die Seite hält den leeren Entwurf mit `Guid.Empty` nur in ihrem Zustand. Beim ersten Senden ruft sie unmittelbar vor `SendeNachrichtAsync` die vorhandene Methode `NeueUnterhaltungAsync` auf; diese reserviert die Kennung nur im ChatService. `SendeNachrichtAsync` speichert Unterhaltung und erste Nachricht gemeinsam, bevor es den Antwortlauf liefert. Erst dann aktualisiert die Seite die Liste. Scheitert das Senden vorher, verwirft sie die Reservierung über `LoescheUnterhaltungAsync`. Die Methodensignaturen bleiben unverändert.

## 6. Tests

- Automatisiert: Seitenstart mit leerer echter SQLite-Datenbank erzeugt keinen Datensatz; „Neue Unterhaltung“ erhält die bestehende Liste ohne Neuanlage; erste Nachricht legt genau eine Unterhaltung an; Fehler vor Nachrichtenspeicherung hinterlässt keine leere Unterhaltung; vorhandene F01/F04/F05-Regressionen.
- Manuell: Seite mehrfach laden und „Neue Unterhaltung“ mehrfach betätigen; Liste und Datenbank bleiben unverändert. Danach erste Frage senden: genau ein neuer Listeneintrag erscheint sofort und bleibt nach Neustart erhalten. Mit einer zweiten Unterhaltung, vertikaler Stapelung und aktiver Hervorhebung vergleichen.

## 7. Grösse

Ein Halbtag.
Teilschritte: 1. lokalen leeren Entwurf anzeigen, 2. erste Nachricht mit neuer Unterhaltung speichern und Liste aktualisieren, 3. Persistenzprüfung nach Neustart und kurzer Review.

## 8. Board

Projektplan Blatt Arbeitspakete, F03, Status In Arbeit, verantwortliche Person [BH], WIP je Person max. zwei.

---

## Ergebnis (das Werkzeug schreibt seinen Bericht nach AGENTS.md Abschnitt 9 hierher; der Mensch prüft und hakt ab)

- Was gebaut wurde: Neue Unterhaltung beginnt als ungespeicherter Entwurf direkt aus der Chat-Seite. Beim Seitenladen oder erneuten Starten entsteht kein Listeneintrag und kein SQLite-Datensatz. Die erste gesendete Nachricht legt genau eine Unterhaltung an und aktualisiert die Liste sofort; bestehende Unterhaltungen bleiben erhalten.
- Tests gelaufen (aktueller Abschlusslauf): `dotnet build CustomLocalAiFront.slnx --no-restore --disable-build-servers -m:1` -> 0 Warnungen, 0 Fehler; `dotnet test --project tests/Chat.Tests/Chat.Tests.csproj --no-build` -> 65 bestanden, 0 fehlgeschlagen, 1 vorhandener expliziter Modellserver-Test nicht ausgeführt; `dotnet format ... --verify-no-changes` -> keine Änderung erforderlich.
- Manuell geprüft (Zeile im Reviewprotokoll): offen; keine manuelle Browser-Prüfung im Verlauf dokumentiert.
- Review durch: Stichprobe, menschliche Freigabe offen.
- KI beteiligt: GitHub Copilot bei der ursprünglichen Umsetzung; Codex bei der Präzisierung auf einen ungespeicherten Entwurf, bei Regressionstests und Dokumentation. Geprüft über vollständigen Build/Test/Format-Lauf und Diff.
- Abweichung vom früheren Designstand: Die automatische leere Unterhaltung beim Seitenstart wurde auf ausdrücklichen Auftrag von PS entfernt; `Design.md`, `Testfaelle.md`, `Schnittstellen.md` und `README.md` wurden nachgeführt. Übernahme in das Masterkonzept und Änderungsnachweis 14.3 bleibt bei der Gruppe.
- Dauer effektiv: ca. 1 Halbtag inklusive Layout- und Testkorrektur.
- Definition of Done Punkt 1 bis 4 und 7 technisch erfüllt; manueller Test, menschliches Review, Board-/Dauerabschluss und Commit stehen noch aus.

### Bericht zur Präzisierung vom 24.09.2026

Karte: F03 Neue Unterhaltung beginnen

Umgesetzt:

- Seitenstart und „Neue Unterhaltung“ erzeugen nur einen lokalen Entwurf ohne Kennung, Listeneintrag oder Datenbankdatensatz.
- Beim ersten Senden wird genau eine Unterhaltung erzeugt, die Nachricht vor Rückgabe des Antwortlaufs gespeichert und die Liste sofort aktualisiert.
- Scheitert das erste Senden vor der gemeinsamen Speicherung, verwirft die Seite die reservierte Kennung und behält die Eingabe für einen neuen Versuch; ein leerer Datensatz entsteht nicht.

Geänderte Dateien:

- `src/LocalAiFront/Components/Pages/Home.razor`, neu `Home.Entwurf.cs`.
- `src/Chat.Core/ChatService.cs`, neu `ChatService.Entwurf.cs`, `IChatService.cs` und `UnterhaltungsZugriff.cs`.
- `tests/Chat.Tests/ChatSeiteTests.cs`, `ChatSeiteUnterhaltungenTests.cs`, `ChatServiceTests.cs`, `ChatServiceUnterhaltungenTests.cs`, neu `ChatSeitePersistenzTests.cs`.
- `README.md`, `agent/Design.md`, `agent/Schnittstellen.md`, `agent/Testfaelle.md`, F03/F04/F05-Karten sowie KI- und Reviewprotokoll.

Nicht angefasst (bewusst):

- Store-Vertrag und dessen Umsetzung, Adapter, Migrationen, Projekt- und Konfigurationsdateien, Docker Compose sowie bestehende Datenbanken und Protokolldateien.

Prüfungen:

- `dotnet build CustomLocalAiFront.slnx --no-restore --disable-build-servers -m:1`: OK, 0 Warnungen, 0 Fehler.
- `dotnet test --project tests/Chat.Tests/Chat.Tests.csproj --no-build`: 65 bestanden, 0 fehlgeschlagen, 1 vorhandener expliziter Modellserver-Test nicht ausgeführt.
- `dotnet format CustomLocalAiFront.slnx --no-restore --verify-no-changes --verbosity minimal`: OK, keine Änderung erforderlich.
- `git diff --check`: OK. `ChatService.cs` 269, `ChatService.Entwurf.cs` 48, `Home.razor` 292, `Home.Entwurf.cs` 50, `ChatSeiteUnterhaltungenTests.cs` 299 und `ChatSeitePersistenzTests.cs` 74 Zeilen.

Selbstprüfung (`agent/Review_Checkliste.md`), Punkt für Punkt:

| Punkt                                                  | Ergebnis und Grund                                                                                                                                                                 |
| ------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Fachlichkeit, Akzeptanzkriterien, Umfang               | Ja für die beauftragte Präzisierung; manuelle Abnahme der übrigen vorhandenen F03-Kriterien bleibt offen. Keine zusätzliche Fachfunktion.                                          |
| Namen, Schichtgrenzen, Verständlichkeit                | Ja; der sichtbare Entwurf ist UI-Zustand, der ChatService koordiniert die reservierte Kennung, die Seite kennt nur `IChatService`, kleine Partial-Datei hält die Grössenregel ein. |
| Format, Duplikate, Kommentare                          | Ja; Formatprüfung grün, gemeinsamer bestehender Listenpfad verwendet, Kommentare erklären nur Lebenszyklusfälle.                                                                   |
| Vier Störfälle                                         | Ja; bestehende Behandlung unverändert und Regression grün. Ein Fehler beim ersten Senden hinterlässt keinen leeren Verlauf.                                                        |
| Abbruch und Teilantwort                                | Ja; bestehende Tests grün, keine Zustandsänderung.                                                                                                                                 |
| Leere/zu lange Eingabe und Zustandspfeile              | Ja; Senden bleibt bei leerer Eingabe gesperrt, Core-Validierung und Zustandsmodell unverändert.                                                                                    |
| Auswirkungen, Schnittstellen, Konfiguration            | Ja; betroffene F03/F04/F05-Dokumente nachgeführt, Semantik von `NeueUnterhaltungAsync` dokumentiert, keine Signatur oder Konfiguration geändert.                                   |
| Geheimnisse, Netzwerk, Abhängigkeiten, Eingabetrennung | Ja; keine Daten/Geheimnisse oder Abhängigkeiten ergänzt, kein neuer Netzwerkpfad.                                                                                                  |
| Semantik und Tastatur                                  | Ja hinsichtlich Code; vorhandenes beschriftetes Textfeld und echte Buttons. Manuelle Tastaturprüfung offen.                                                                        |
| Gestaltung und Fensterbreiten                          | CSS unverändert; Prüfung beider Fensterbreiten noch offen.                                                                                                                         |
| Automatisierte Tests und neue Logik                    | Ja; 65 bestanden. Echter temporärer SQLite-Test beweist leere DB beim Start und genau eine Unterhaltung nach erster Nachricht.                                                     |
| Manuelle Tests und Einschränkungen                     | Nein; Browserabnahme und echter Prozessneustart stehen offen und sind im Reviewprotokoll vermerkt.                                                                                 |
| Nachvollziehbarkeit und Commit                         | Ja; KI-Einsatz und Karten dokumentiert. Commit steht aus.                                                                                                                          |

Offen, Risiken, Befunde ausserhalb der Karte:

- PS prüft Seitenstart, mehrfaches „Neue Unterhaltung“, erste Nachricht und Neustart im Browser sowie beide Fensterbreiten.
- Ein Gruppenmitglied übernimmt die Präzisierung in das Masterkonzept und den Änderungsnachweis 14.3 und führt das unabhängige Review durch.

Vorschlag Commit-Nachricht: `F03: Unterhaltung erst mit erster Nachricht speichern` / `KI: Codex, geprueft von XX`.

### Bericht zur Darstellung des gesperrten Buttons vom 24.09.2026

Karte: F03 Neue Unterhaltung beginnen

Umgesetzt:

- Der gesperrte Button „Neue Unterhaltung“ verwendet während des Sendens oder Ladens das helle HFU-Rosa statt des Bootstrap-Blaus.
- Gedeckte Schrift, neutraler Rahmen und reduzierte Deckkraft kennzeichnen den Button weiterhin als nicht bedienbar.

Geänderte Dateien:

- `src/LocalAiFront/Components/Pages/Home.razor.css`
- `agent/use_cases/F03-Neue-Unterhaltung-beginnen.md`
- `agent/protokolle/KI-Einsatz.md`

Nicht angefasst (bewusst):

- `Home.razor` mit einer bereits vorhandenen fremden Änderung, Streaming- und Abbruchlogik, andere Schichten, Konfiguration und Tests.

Prüfungen:

- `dotnet build CustomLocalAiFront.slnx --no-restore --disable-build-servers -m:1`: OK, 0 Warnungen, 0 Fehler.
- `dotnet test --project tests/Chat.Tests/Chat.Tests.csproj --no-build`: 71 bestanden, 0 fehlgeschlagen, 1 vorhandener expliziter Modellserver-Test nicht ausgeführt.
- `dotnet format CustomLocalAiFront.slnx --no-restore --verify-no-changes`: Fehler wegen bereits vorhandener CRLF-Zeilenenden in unveränderten C#- und Testdateien; diese Dateien wurden ausserhalb des Kartenumfangs nicht geändert.
- `git diff --check`: keine Whitespace-Fehler; Git weist lediglich auf die lokale LF-zu-CRLF-Konvertierung der geänderten CSS-Datei hin.

Selbstprüfung (`agent/Review_Checkliste.md`), Punkt für Punkt:

| Punkt | Ergebnis und Grund |
| --- | --- |
| Fachlichkeit, Akzeptanzkriterien, Umfang | Ja; nur der beauftragte Disabled-Zustand wurde geändert, ohne neue Fachfunktion. |
| Namen, Schichtgrenzen, Verständlichkeit | Ja; ein eindeutiger CSS-Selektor betrifft nur den vorhandenen Button. |
| Format, Duplikate, Kommentare | Nein für die globale Formatprüfung wegen vorhandener Zeilenenden in unveränderten Dateien; der eigene CSS-Diff enthält keine Whitespace-Fehler und keine Duplikation. |
| Fehlerbehandlung und Grenzfälle | Ja; keine Logik oder Fehlerbehandlung geändert. |
| Auswirkungen, Schnittstellen, Konfiguration | Ja; Schnittstellen und Konfiguration unverändert. |
| Geheimnisse, Netzwerk, Abhängigkeiten, Eingabetrennung | Ja; keine Daten, Netzwerkpfade oder Abhängigkeiten ergänzt. |
| Semantik und Tastatur | Ja hinsichtlich Code; echtes `disabled` bleibt unverändert erhalten. |
| Gestaltung und Fensterbreiten | Ja hinsichtlich CSS-Variablen und responsivem Verhalten; manuelle Sichtprüfung beider Fensterbreiten offen. |
| Automatisierte Tests und neue Logik | Ja; 71 Tests bestanden, keine neue Logik, daher kein neuer Test erforderlich. |
| Manuelle Tests und Einschränkungen | Nein; Browser-Sichtprüfung ist noch offen. |
| Nachvollziehbarkeit und Commit | Ja; Karte und KI-Einsatz sind dokumentiert, Commit und menschliches Review offen. |

Offen, Risiken, Befunde ausserhalb der Karte:

- PS prüft den Disabled-Zustand während der Antwortgenerierung im Browser bei beiden Fensterbreiten.
- Die vorhandenen CRLF-Zeilenenden verhindern derzeit eine grüne globale Formatprüfung und müssen in einer eigenen freigegebenen Änderung bereinigt werden.
- Menschliches Review und Freigabe bleiben offen.

Vorschlag Commit-Nachricht: `F03: Gesperrten Neue-Unterhaltung-Button rosa darstellen` / `KI: Codex, geprueft von XX`.
