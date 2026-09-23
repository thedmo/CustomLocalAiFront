# Karte AP07: Backend nimmt eine Nachricht an und streamt die Antwort (F01, F07)

Entwurf für den ersten Probelauf des Workflows am Mittwoch 23.09.2026. Benjamin prüft und ergänzt die Felder, bevor der Auftrag an das Werkzeug geht.

| Feld | Inhalt |
|---|---|
| Verantwortlich | BH |
| Halbtag | Mi AM (ab 10:30) |
| Referenz | F01 Nachricht senden, F07 Konfiguration; Z01 offline, Z02 Konfiguration, Z03 Antwort in Teilen; M05, M06, M09 |
| Quellen | `agent/Use_Cases.md` Haupt-Use-Case Standardablauf 1 bis 6 und Erweiterungen 2a, 2b, 3a; `agent/Design.md` Komponenten und Datenfluss; `agent/Schnittstellen.md` IChatService, IModelServerClient, Konfiguration |
| Testfälle | T02, T07, T08, T09, T10, T11, T12 aus `agent/Testfaelle.md` |

## 1. Ziel (ein Satz)

Der ChatService nimmt eine Nachricht mit Kennung der Unterhaltung an, prüft sie, ruft den Docker Model Runner über den Adapter und liefert die Antwort als Datenstrom von Teilen; die Konfiguration kommt aus appsettings.json und wird beim Start geprüft.

## 2. Abgrenzung (gehört nicht dazu)

Keine Oberfläche (AP08). Keine Persistenz über den Neustart hinaus, Store nur im Arbeitsspeicher (AP09). Kein Abbruch über die Oberfläche; der CancellationToken wird durchgereicht, aber nicht bedient (AP08). Kein Protokoll in Datei (AP12).

## 3. Betroffene Komponenten und Dateien

- `src/Chat.Core`: `Stoerfall.cs`, `StoerfallException.cs`, `Modelle/Unterhaltung.cs`, `Modelle/Nachricht.cs`, `Modelle/Antwort.cs`, `Modelle/AntwortZustand.cs`, `IChatService.cs`, `ChatService.cs`, `IModelServerClient.cs`, `IStore.cs`, `Konfiguration.cs`
- `src/Chat.Adapter.ModelRunner`: `ModelRunnerClient.cs`
- `tests/Chat.Tests`: `ChatServiceTests.cs`, `FakeModelServerClient.cs`, `SpeicherStore.cs`
- Unberührt: `src/Chat.Web`, `src/Chat.Store.Sqlite`, `docker-compose.yml`, `appsettings.example.json`

## 4. Akzeptanzkriterien (prüfbar, vorher festgelegt; beim Review abhaken)

- [ ] Eine Nachricht mit Text liefert innerhalb des Zeitlimits mindestens einen Teil; am Ende steht die Antwort mit Zustand Fertig und Dauer im Store (T02).
- [ ] Leerer Text und Text über der Eingabegrenze werden abgewiesen, ohne den Modellserver zu rufen (T09, T10).
- [ ] Modellserver nicht erreichbar: StoerfallException ModellserverNichtErreichbar mit Grund, kein Absturz, Antwort im Zustand Gestört (T07).
- [ ] Ungültige Konfiguration (Adresse leer, Modellname leer): Start bricht mit Störfall KonfigurationUngueltig und Nennung des Werts ab (T08).
- [ ] Zeitlimit überschritten: Zustand Gestört mit Grund Zeitüberschreitung (T11).
- [ ] Nur die acht erlaubten Zustandsübergänge sind möglich (T12).

## 5. Schnittstellen und Konfiguration

`IChatService.SendeNachrichtAsync(unterhaltungId, text, ct)` liefert `IAsyncEnumerable<string>`. `IModelServerClient.StreamAntwortAsync(verlauf, systemanweisung, ct)`. `IStore.SpeichernAsync`, `LadenAsync`. Konfigurationswerte: AdresseModellserver, Modellname, Systemanweisung, Temperatur, MaximaleAntwortlaenge, Eingabegrenze, ZeitlimitSekunden (Abschnitt `Chat`). Keine Änderung an den Signaturen aus `agent/Schnittstellen.md`; ist eine nötig, anhalten und melden.

## 6. Tests

- Automatisiert (xUnit, Fake-Adapter, Speicher im Arbeitsspeicher): `SendeNachricht_LeererText_WirftStoerfall`, `SendeNachricht_ZuLang_WirftStoerfall`, `SendeNachricht_FakeAdapter_LiefertTeileUndFertig`, `SendeNachricht_AdapterWirft_ZustandGestoert`, `SendeNachricht_Zeitlimit_ZustandGestoert`, `Konfiguration_AdresseLeer_WirftKonfigurationUngueltig`, `AntwortZustand_NurErlaubteUebergaenge`.
- Integration, nicht im Standardlauf: `ModelRunnerClient_EchterServer_LiefertTeile` mit `[Trait("Art", "Integration")]`.
- Manuell (Jemand, Zeile im Protokoll): `docker compose up`, Anfrage an `GET /status` zeigt Modellserver erreichbar und Modellname; Integrationstest gegen den laufenden Model Runner grün.

## 7. Grösse und Teilschritte

Ein Halbtag. Teilschritt 1: Chat.Core mit Modellen, Störfall, Schnittstellen, Konfiguration mit Prüfung; Build grün. Teilschritt 2: ChatService mit Fake-Adapter und den sieben Tests. Teilschritt 3: ModelRunnerClient gegen den echten Model Runner, Integrationstest, Status-Endpunkt.

## 8. Board

Projektplan Blatt Arbeitspakete, AP07, Status In Arbeit ab Mi 10:30, Verantwortlich BH; WIP von BH damit 1.

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
