# Karte AP07: Backend nimmt eine Nachricht an, streamt und bricht ab (F01, F02, F07)

Entwurf für den ersten Probelauf des Workflows am Mittwoch 23.09.2026. Benjamin prüft und ergänzt die Felder, bevor der Auftrag an das Werkzeug geht.

| Feld | Inhalt |
|---|---|
| Verantwortlich | BH |
| Halbtag | Mi AM (ab 10:30) |
| Referenz | F01 Nachricht senden, F02 Abbrechen, F07 Konfiguration; Z01 offline, Z02 Konfiguration, Z03 Antwort in Teilen; M05, M06, M09 |
| Quellen | `agent/Use_Cases.md` Haupt-Use-Case Standardablauf 1 bis 6 und Erweiterungen 2a, 2b, 3a; `agent/Design.md` Komponenten und Datenfluss; `agent/Schnittstellen.md` IChatService, IModelServerClient, Konfiguration |
| Testfälle | T02, T04, T07, T08, T09, T10, T11, T12 aus `agent/Testfaelle.md` |

**READY-Status:** fachlich und technisch abgeglichen. Vor der Code-Umsetzung muss ein Gruppenmitglied noch bestätigen, dass das Zielbild aus `agent/Design.md` in das Masterkonzept Kapitel 7 und 14.3 übernommen wurde und dass der erweiterte Umfang mit neun automatisierten Tests in einen Halbtag passt. Andernfalls wird AP07 vor Beginn in Teilkarten getrennt. Bis dahin ist die Karte gemäss `AGENTS.md` Abschnitt 6 blockiert.

## 1. Ziel (ein Satz)

Der ChatService nimmt eine Nachricht mit Kennung der Unterhaltung an, prüft sie, liefert vor dem Modellaufruf eine Antwort-ID mit Datenstrom, verwaltet den Abbruch und ruft Docker Model Runner über den Adapter mit geprüfter Konfiguration auf.

## 2. Abgrenzung (gehört nicht dazu)

Keine Oberfläche (AP08). Keine Persistenz über den Neustart hinaus, Store nur im Arbeitsspeicher (AP09). Kein Protokoll in Datei (AP12). Kein zusätzlicher HTTP-Endpunkt `GET /status`.

## 3. Betroffene Komponenten und Dateien

- `src/Chat.Core`: `Stoerfall.cs`, `StoerfallException.cs`, `Modelle/Unterhaltung.cs`, `Modelle/Nachricht.cs`, `Modelle/Antwort.cs`, `Modelle/AntwortZustand.cs`, `AntwortLauf.cs`, `IChatService.cs`, `ChatService.cs`, `IModelServerClient.cs`, `IStore.cs`, `Konfiguration.cs`
- `src/Chat.Adapter.ModelRunner`: `ModelRunnerClient.cs`
- `tests/Chat.Tests`: `ChatServiceTests.cs`, `FakeModelServerClient.cs`, `SpeicherStore.cs`
- Unberührt: `web_app/LocalAiFront`, `src/Chat.Store.Sqlite`, `docker_compose.yml`, `web_app/LocalAiFront/appsettings.example.json`, alle `.csproj`, `global.json`

## 4. Akzeptanzkriterien (prüfbar, vorher festgelegt; beim Review abhaken)

- [ ] Eine Nachricht mit Text liefert innerhalb des Zeitlimits mindestens einen Teil; am Ende steht die Antwort mit Zustand Fertig und Dauer im Store (T02).
- [ ] Die Antwort-ID ist vor dem Modellaufruf verfügbar; Abbruch vor und während des Streams beendet die Anfrage und speichert Zustand Abgebrochen (T04).
- [ ] Leerer Text und Text über der Eingabegrenze werden abgewiesen, ohne den Modellserver zu rufen (T09, T10).
- [ ] Modellserver nicht erreichbar: StoerfallException ModellserverNichtErreichbar mit Grund, kein Absturz, Antwort im Zustand Gestört (T07).
- [ ] Ungültige Konfiguration (Adresse leer, Modellname leer) verhindert die Modellanfrage mit Störfall KonfigurationUngueltig und nennt den betroffenen Wert (T08).
- [ ] Zeitlimit überschritten: Zustand Gestört mit Grund Zeitüberschreitung (T11).
- [ ] Nur die sechs dokumentierten Zustandsübergänge sind möglich (T12).

## 5. Schnittstellen und Konfiguration

`IChatService.SendeNachrichtAsync(unterhaltungId, text, ct)` liefert `Task<AntwortLauf>` mit Antwort-ID und Teilen. `IChatService.AbbrechenAsync(antwortId, ct)` verwaltet den Benutzerabbruch im ChatService. `IModelServerClient.StreamAntwortAsync(verlauf, systemanweisung, ct)`, `IStore.SpeichernAsync` und `LadenAsync` bleiben produktneutral. Konfigurationswerte: AdresseModellserver, Modellname, Systemanweisung, Temperatur, MaximaleAntwortlaenge, Eingabegrenze, ZeitlimitSekunden (Abschnitt `Chat`). Keine Änderung an den Signaturen aus `agent/Schnittstellen.md`; ist eine nötig, anhalten und melden.

## 6. Tests

- Automatisiert (xUnit, Fake-Adapter, Speicher im Arbeitsspeicher): `SendeNachricht_LeererText_WirftStoerfall`, `SendeNachricht_ZuLang_WirftStoerfall`, `SendeNachricht_FakeAdapter_LiefertAntwortIdTeileUndFertig`, `Abbrechen_VorErstemTeil_SpeichertAbgebrochen`, `Abbrechen_WaehrendStream_SpeichertTeilUndAbgebrochen`, `SendeNachricht_AdapterWirft_ZustandGestoert`, `SendeNachricht_Zeitlimit_ZustandGestoert`, `Konfiguration_AdresseLeer_WirftKonfigurationUngueltig`, `AntwortZustand_NurErlaubteUebergaenge`.
- Integration, nicht im Standardlauf: `ModelRunnerClient_EchterServer_LiefertTeile` mit `[Trait("Art", "Integration")]`.
- Manuell (Jemand, Zeile im Protokoll): `docker compose -f docker_compose.yml up`, Integrationstest gegen den laufenden Model Runner grün; danach `docker compose -f docker_compose.yml down`.

## 7. Grösse und Teilschritte

Ein Halbtag. Teilschritt 1: Chat.Core mit Modellen, Störfall, Schnittstellen und Konfigurationsprüfung; Build und Tests grün. Teilschritt 2: ChatService mit Fake-Adapter, Zuständen, Abbruch und automatisierten Tests; Build und Tests grün. Teilschritt 3: ModelRunnerClient mit SSE und Integrationstest; Build und Tests grün.

## 8. Board

Projektplan Blatt Arbeitspakete, AP07, Status In Arbeit ab Mi 10:30, Verantwortlich BH; WIP von BH damit 1.

## Vorbereitung vor Umsetzung (23.09.2026)

- Projektgerüste, Lösung und `global.json` sind vorhanden; AP07 muss keine Projekt- oder Betriebskonfiguration ändern.
- LiteLLM ist aus Compose und Beispielkonfiguration entfernt. Der bestehende Verbindungstest spricht Docker Model Runner direkt an.
- `Design.md`, `Schnittstellen.md`, `Use_Cases.md`, `Testfaelle.md` und diese Karte verwenden denselben Vertrag: Antwort-ID vor dem Stream, serviceverwalteter Abbruch und sechs erlaubte Zustandsübergänge.
- `GET /status` ist aus AP07 entfernt, weil dafür die in dieser Karte bewusst unberührte Web-Schicht nötig wäre.
- Technischer Blocker auf dem geprüften Rechner: Es ist kein .NET SDK installiert. Vor der Umsetzung .NET 10 SDK installieren und `dotnet --info` prüfen.
- Freigabeblocker: Das Masterkonzept ist nicht im Repository vorhanden und laut `Design.md` noch nicht nachgeführt. Ein Gruppenmitglied muss die Übernahme in Kapitel 7 und 14.3 bestätigen.
- Grössenblocker: Die Schätzung von einem Halbtag muss für Core, Abbruch, neun Tests und echten SSE-Adapter bestätigt oder vor Beginn in Teilkarten aufgeteilt werden.
- Prüfungen der Vorbereitung: `git diff --check` ohne Fehler. `dotnet build`, `dotnet test` und `dotnet format --verify-no-changes` konnten nicht starten; alle drei melden, dass kein kompatibles .NET SDK installiert ist.

---

## Auftrag an das Werkzeug (Text, den jemand in den Chat des Werkzeugs gibt)

```text
Lies AGENTS.md und agent/README.md. Setze die Karte agent/use_cases/AP07-backend-senden.md um.
Halte dich an die Grenzen in Feld 2 und 3. Nenne zuerst deinen Plan in drei bis fünf Sätzen und warte auf mein OK.
Baue in den drei Teilschritten aus Feld 7, nach jedem Teilschritt dotnet build und dotnet test.
Die Tests aus Feld 6 müssen grün sein. Schreibe deinen Bericht nach AGENTS.md Abschnitt 9 in den Abschnitt Ergebnis dieser Karte
und eine Zeile in agent/protokolle/KI-Einsatz.md. Kein Commit.
```

## Ergebnis (das Werkzeug schreibt seinen Bericht nach AGENTS.md Abschnitt 9 hierher; der Mensch prüft und hakt ab)

- Was gebaut wurde: [ ]
- Tests gelaufen (Ausgabe): [ ]
- Manuell geprüft (Zeile im Reviewprotokoll): [ ]
- Review durch: [PS, Pflicht: Schnittstelle 7.3 und Architektur betroffen]
- KI beteiligt: [Werkzeug, wofür, wie geprüft; Zeile in KI-Einsatz.md]
- Abweichung vom Konzept: [keine] oder [Kapitel, was, warum; Eintrag in 14.3]
- Dauer effektiv: [ ]
- Definition of Done Punkt 1 bis 8: [ ]
