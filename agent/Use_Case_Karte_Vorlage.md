# Karte APxx: Titel im Präsens (Beispiel: AP07 Backend sendet Nachricht und streamt Antwort)

Datei nach `agent/use_cases/APxx-kurztitel.md` kopieren. Alle Felder der Definition of Ready ausfüllen, bevor gebaut wird. Was in eckigen Klammern steht, ist auszufüllen.

| Feld | Inhalt |
|---|---|
| Verantwortlich | [Kürzel] |
| Halbtag | [Di PM] |
| Referenz | [F01, F07; Z01, Z02, Z03; M05, M06] |
| Quellen | [`agent/Use_Cases.md` Haupt-Use-Case Schritte 1 bis 6; `agent/Design.md` Datenfluss; `agent/Schnittstellen.md` SendeNachricht; Konfiguration 7.4] |
| Testfälle | [T02, T07, T09, T10, T11 aus `agent/Testfaelle.md`] |

## 1. Ziel (ein Satz)

[Der ChatService nimmt eine Nachricht an, ruft den Model Runner über den Adapter und liefert die Antwort in Teilen.]

## 2. Abgrenzung (gehört nicht dazu)

[Keine Oberfläche (AP08), keine Persistenz über den Neustart hinaus (AP09), kein Abbruch (eigene Karte oder Teilschritt 2).]

## 3. Betroffene Komponenten und Dateien

[Chat.Core: IChatService, ChatService, Konfiguration, Störfall. Chat.Adapter.ModelRunner: ModelRunnerClient. Unberührt: Chat.Web, Chat.Store.Sqlite, docker-compose.yml.]

## 4. Akzeptanzkriterien (prüfbar, vorher festgelegt; beim Review abhaken)

- [ ] [Eine Nachricht mit Text liefert innerhalb des Zeitlimits mindestens einen Teil und endet mit Zustand Fertig.]
- [ ] [Leerer Text wird abgewiesen, ohne den Modellserver zu rufen (4.7 Erweiterung 2a).]
- [ ] [Modellserver nicht erreichbar: Störfall ModellserverNichtErreichbar mit Grund, kein Absturz (M09).]

## 5. Schnittstellen und Konfiguration

[IChatService.SendeNachrichtAsync (7.3). IModelServerClient.StreamAntwortAsync. Werte: AdresseModellserver, Modellname, Systemanweisung, Eingabegrenze, Zeitlimit (7.4).]

## 6. Tests

- Automatisiert: [SendeNachricht_LeererText_WirftStoerfall; SendeNachricht_FakeAdapter_LiefertTeileUndFertig; SendeNachricht_AdapterWirft_ZustandGestoert]
- Manuell: [Nachricht über Status-Endpunkt oder Testkonsole an den echten Model Runner, Teile erscheinen, Protokolleintrag ohne Text]

## 7. Grösse

[Ein Halbtag. Teilschritte: 1 Schnittstellen und Modelle, 2 ChatService mit Fake-Adapter und Tests, 3 Adapter gegen den echten Model Runner.]

## 8. Board

[Im Projektplan Blatt Arbeitspakete, Status In Arbeit, WIP je Person höchstens zwei.]

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
