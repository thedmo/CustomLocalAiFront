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
- [ ] Die neue Unterhaltung wird lokal gespeichert und bleibt nach einem Neustart der Anwendung verfügbar.
- [ ] Ein weiterer Wechsel zwischen alten und neuen Unterhaltungen funktioniert, ohne dass Daten verloren gehen oder überschrieben werden.

## 5. Schnittstellen und Konfiguration

Die Funktion nutzt die vorhandenen Store- und Unterhaltungskonzepte aus `agent/Schnittstellen.md`: `IStore.SpeichernAsync(...)` und `LadenAsync(...)` sowie die zentrale Darstellung einer `Unterhaltung` mit ihrer Kennung, dem Titel und den Nachrichten. Wenn die aktuelle Unterhaltung im Backend verwaltet wird, ist eine kleine neue Operation im `IChatService` oder ein entsprechender Service-Wrapper nötig; die bestehenden Nachrichtenschnittstellen aus F01 bleiben unverändert.

## 6. Tests

- Automatisiert: `NeuesGespraech_StartetLeereUnterhaltung`, `BestehendeUnterhaltungen_BleibenErhalten`, `Unterhaltung_NachNeustart_WiederVerfuegbar`
- Manuell: Button „Neue Unterhaltung“ in der Chat-Seite, anschließend neue Frage senden und mit einer zweiten Unterhaltung vergleichen; nach Neustart bleibt die Liste vollständig erhalten

## 7. Grösse

Ein Halbtag.
Teilschritte: 1. neue leere Unterhaltung im Modell und Store 2. UI-Integration mit aktiver Unterhaltung, 3. Persistenzprüfung nach Neustart und kurzer Review.

## 8. Board

Projektplan Blatt Arbeitspakete, F03, Status In Arbeit, verantwortliche Person [BH], WIP je Person max. zwei.

---

## Ergebnis (das Werkzeug schreibt seinen Bericht nach AGENTS.md Abschnitt 9 hierher; der Mensch prüft und hakt ab)

- Was gebaut wurde: [ ]
- Tests gelaufen (Ausgabe): [ ]
- Manuell geprüft (Zeile im Reviewprotokoll): [ ]
- Review durch: [Kürzel, Datum] oder [Stichprobe]
- KI beteiligt: [Werkzeug, wofür, wie geprüft; Zeile in KI-Einsatz.md]
- Abweichung vom Konzept: [keine] oder [Kapitel, was, warum; Eintrag in 14.3]
- Dauer effektiv: [ ]
- Definition of Done Punkt 1 bis 8: [alle ja]
