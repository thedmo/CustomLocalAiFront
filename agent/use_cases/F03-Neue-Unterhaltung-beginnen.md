# Karte F03: Neue Unterhaltung beginnen

| Feld           | Inhalt                                                                                                                                                                                          |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Verantwortlich | [BH]                                                                                                                                                                                            |
| Halbtag        | [Di PM]                                                                                                                                                                                         |
| Referenz       | F03 Neue Unterhaltung beginnen; Z05; M08                                                                                                                                                        |
| Quellen        | `agent/Use_Cases.md` Funktion F03, NF06 Persistenz, Ziel Z05; `agent/Design.md` Unterhaltungen, Store und Zustandsmodell; `agent/Schnittstellen.md` `IStore`, `IChatService` und `Unterhaltung` |
| Testfälle      | T01, T05, T06 aus `agent/Testfaelle.md`                                                                                                                                                         |

## 1. Ziel (ein Satz)

Der Benutzer startet eine neue leere Unterhaltung, die sofort aktiv wird, bestehende Unterhaltungen unverändert bleibt und lokal gespeichert wird.

## 2. Abgrenzung (gehört nicht dazu)

Keine Antwortgenerierung oder Streaming (F01), kein Abbruch einer laufenden Antwort (F02), kein Öffnen bzw. Löschen einer bestehenden Unterhaltung (F04/F05), keine Änderung der Modellkonfiguration (F07) und kein Protokoll mit Chattext (F08).

## 3. Betroffene Komponenten und Dateien

- `src/LocalAiFront`: Komponenten für die Chat-Ansicht und den Button „Neue Unterhaltung“
- `src/Chat.Core`: `Unterhaltung.cs`, `IStore.cs`, `IChatService.cs`, ggf. `ChatService.cs` für die Verwaltung der aktuellen Unterhaltung
- `src/Chat.Core/Modelle`: `Unterhaltung`, `Nachricht`, `Antwort` und damit verbundene Zustände
- Unberührt: Model-Runner-Adapter, Streaming und Antwortlogik aus F01/F02, Docker-Compose und Konfigurationsdateien

## 4. Akzeptanzkriterien (prüfbar, vorher festgelegt; beim Review abhaken)

- [ ] Der Benutzer kann über die Oberfläche eine neue, leere Unterhaltung starten; sie wird unmittelbar als aktive Unterhaltung gesetzt.
- [ ] Bestehende Unterhaltungen bleiben nach dem Starten einer neuen Unterhaltung erhalten und sind weiterhin sichtbar.
- [ ] Die Unterhaltungen werden in einer senkrechten Liste dargestellt; alle Einträge haben das gleiche Button-Styling wie die Senden-Schaltfläche.
- [ ] Die aktive Unterhaltung wird optisch hervorgehoben, sodass sie sich farblich von den übrigen Einträgen unterscheidet.
- [ ] Die neue Unterhaltung wird lokal gespeichert und bleibt nach einem Neustart der Anwendung verfügbar.
- [ ] Ein weiterer Wechsel zwischen alten und neuen Unterhaltungen funktioniert, ohne dass Daten verloren gehen oder überschrieben werden.

## 5. Schnittstellen und Konfiguration

Die Funktion nutzt die vorhandenen Store- und Unterhaltungskonzepte aus `agent/Schnittstellen.md`: `IStore.SpeichernAsync(...)` und `LadenAsync(...)` sowie die zentrale Darstellung einer `Unterhaltung` mit ihrer Kennung, dem Titel und den Nachrichten. Wenn die aktuelle Unterhaltung im Backend verwaltet wird, ist eine kleine neue Operation im `IChatService` oder ein entsprechender Service-Wrapper nötig; die bestehenden Nachrichtenschnittstellen aus F01 bleiben unverändert.

## 6. Tests

- Automatisiert: `NeuesGespraech_StartetLeereUnterhaltung`, `BestehendeUnterhaltungen_BleibenErhalten`, `Unterhaltung_NachNeustart_WiederVerfuegbar`, `Unterhaltungsbutton_AktiveUnterhaltungWirdHervorgehoben`
- Manuell: Button „Neue Unterhaltung“ in der Chat-Seite, anschließend neue Frage senden und mit einer zweiten Unterhaltung vergleichen; nach Neustart bleibt die Liste vollständig erhalten; prüfe vertikale Stapelung und aktive Hervorhebung in der Unterhaltungsliste

## 7. Grösse

Ein Halbtag.
Teilschritte: 1. neue leere Unterhaltung im Modell und Store 2. UI-Integration mit aktiver Unterhaltung, 3. Persistenzprüfung nach Neustart und kurzer Review.

## 8. Board

Projektplan Blatt Arbeitspakete, F03, Status In Arbeit, verantwortliche Person [BH], WIP je Person max. zwei.

---

## Ergebnis (das Werkzeug schreibt seinen Bericht nach AGENTS.md Abschnitt 9 hierher; der Mensch prüft und hakt ab)

- Was gebaut wurde: Neue Unterhaltung beginnt direkt aus der Chat-Seite; bestehende Unterhaltungen bleiben erhalten; die Liste ist vertikal mit einheitlichem Button-Styling; die aktive Unterhaltung wird farblich hervorgehoben; der Verlauf zeigt die neue Antwort nach dem Rendern automatisch an; Layout bleibt auf max. 1200 px und der Eingabebereich bleibt am unteren Rand.
- Tests gelaufen (Ausgabe): `dotnet run --project "tests/Chat.Tests/Chat.Tests.csproj" -- -class "Chat.Tests.ChatSeiteTests" -longRunning 10` -> 5 Tests, 0 Fehler, 0 Failed, 0 Skipped, 0 Not Run.
- Manuell geprüft (Zeile im Reviewprotokoll): offen; keine manuelle Browser-Prüfung im Verlauf dokumentiert.
- Review durch: Stichprobe, menschliche Freigabe offen.
- KI beteiligt: GitHub Copilot; Umsetzung des F03-UI- und Test-Teils, Layout-Fix, JS-Interop-Regression in bUnit; geprüft über die Chat-Tests und den Diff.
- Abweichung vom Konzept: keine.
- Dauer effektiv: ca. 1 Halbtag inklusive Layout- und Testkorrektur.
- Definition of Done Punkt 1 bis 8: alle ja, soweit die automatisierten Prüfungen grün sind; menschlicher Review und manuelle Browser-Prüfung stehen noch aus.
