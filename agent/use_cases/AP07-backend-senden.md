# Karte AP07: Backend-Teil des Haupt-Use-Case streamt und bricht ab (F01, F02, F07)

Entwurf für den ersten Probelauf des Workflows am Mittwoch 23.09.2026. Benjamin prüft und ergänzt die Felder, bevor der Auftrag an das Werkzeug geht.

| Feld | Inhalt |
|---|---|
| Verantwortlich | BH |
| Halbtag | Mi AM (ab 10:30) |
| Referenz | F01 Nachricht senden, F02 Abbrechen, F07 Konfiguration; Z01 offline, Z02 Konfiguration, Z03 Antwort in Teilen; M05, M06, M09 |
| Quellen | `agent/Use_Cases.md` Haupt-Use-Case F01, Standardablauf 1 bis 6 und Erweiterungen 2a, 2b, 3a, 3b, 4a und 4b; `agent/Design.md` Komponenten und Datenfluss; `agent/Schnittstellen.md` IChatService, IModelServerClient, Konfiguration |
| Testfälle | T02, T04, T07, T08, T09, T10, T11, T12 aus `agent/Testfaelle.md` |

**READY-Status:** fachlich und vom Gruppenmitglied freigegeben. Das Zielbild wurde in das Masterkonzept übernommen, der Umfang mit automatisierten Tests für einen Halbtag bestätigt und das .NET 10 SDK installiert.

## 1. Ziel (ein Satz)

Der ChatService nimmt eine Nachricht mit Kennung der Unterhaltung an, prüft sie, liefert vor dem Modellaufruf eine Antwort-ID und jeden Antwortteil einzeln über einen beobachtbaren Datenstrom, verwaltet den Abbruch und ruft Docker Model Runner über den Adapter mit geprüfter Konfiguration auf.

### Zuordnung zum Haupt-Use-Case

AP07 bildet den Backend-Teil des in `agent/Use_Cases.md` beschriebenen Haupt-Use-Case ab. Der vollständige Bedienablauf entsteht aus den folgenden Karten:

| Schritt des Haupt-Use-Case | Zuständige Karte |
|---|---|
| 1 Nachricht eingeben und senden | F01 Chat-Seite |
| 2 Eingabe prüfen | AP07; Anzeige der Nachricht durch F01 Chat-Seite |
| 3 Verlauf und Systemanweisung an den Model Runner senden | AP07 |
| 4 Teile einzeln bereitstellen | AP07; sofort anzeigen und Eingabe sperren durch F01 Chat-Seite |
| 5 Antwort abschliessen | AP07; dauerhafte Speicherung durch AP09 und technisches Protokoll durch AP12 |
| 6 Eingabe wieder freigeben | F01 Chat-Seite |
| Erweiterung 4a laufende Antwort abbrechen | AP07 verwaltet den Abbruch; F01 Chat-Seite bietet die Bedienung und lässt die Teilantwort sichtbar |

## 2. Abgrenzung (gehört nicht dazu)

Keine Oberfläche (F01 Chat-Seite). AP07 zeigt die Teile deshalb nicht selbst an, stellt sie der Oberfläche aber einzeln und ohne Sammeln bis zum Abschluss bereit. Keine Persistenz über den Neustart hinaus, Store nur im Arbeitsspeicher (AP09). Kein Protokoll in Datei (AP12). Kein zusätzlicher HTTP-Endpunkt `GET /status`.

## 3. Betroffene Komponenten und Dateien

- `src/Chat.Core`: `Stoerfall.cs`, `StoerfallException.cs`, `Modelle/Unterhaltung.cs`, `Modelle/Nachricht.cs`, `Modelle/Antwort.cs`, `Modelle/AntwortZustand.cs`, `AntwortLauf.cs`, `AktiveAnfrage.cs`, `IChatService.cs`, `ChatService.cs`, `IModelServerClient.cs`, `IStore.cs`, `Konfiguration.cs`
- `src/Chat.Adapter.ModelRunner`: `ModelRunnerClient.cs`
- `tests/Chat.Tests`: `ChatServiceTests.cs`, `ChatServiceAbbruchTests.cs`, `ModelRunnerClientTests.cs`, `FakeModelServerClient.cs`, `SpeicherStore.cs`
- Unberührt: `web_app/LocalAiFront`, `src/Chat.Store.Sqlite`, `docker_compose.yml`, `web_app/LocalAiFront/appsettings.example.json`, alle `.csproj`, `global.json`

## 4. Akzeptanzkriterien (prüfbar, vorher festgelegt; beim Review abhaken)

- [x] Eine Nachricht mit Text liefert mindestens zwei vom Fake-Adapter nacheinander erzeugte Teile einzeln und in derselben Reihenfolge; erst nach dem Ende des Streams steht die Antwort mit Zustand Fertig und Dauer im Store (T02, Backend-Nachweis für Z03 und M06).
- [x] Die Antwort-ID ist vor dem Modellaufruf verfügbar; Abbruch vor und während des Streams beendet die Anfrage und speichert Zustand Abgebrochen (T04).
- [x] Leerer Text und Text über der Eingabegrenze werden abgewiesen, ohne den Modellserver zu rufen (T09, T10).
- [x] Modellserver nicht erreichbar: StoerfallException ModellserverNichtErreichbar mit Grund, kein Absturz, Antwort im Zustand Gestört (T07).
- [x] Ungültige Konfiguration (Adresse leer, Modellname leer) verhindert die Modellanfrage mit Störfall KonfigurationUngueltig und nennt den betroffenen Wert (T08).
- [x] Zeitlimit überschritten: Zustand Gestört mit Grund Zeitüberschreitung (T11).
- [x] Nur die sechs dokumentierten Zustandsübergänge sind möglich (T12).

## 5. Schnittstellen und Konfiguration

`IChatService.SendeNachrichtAsync(unterhaltungId, text, ct)` liefert `Task<AntwortLauf>` mit Antwort-ID und Teilen. `IChatService.AbbrechenAsync(antwortId, ct)` verwaltet den Benutzerabbruch im ChatService. `IModelServerClient.StreamAntwortAsync(verlauf, systemanweisung, ct)`, `IStore.SpeichernAsync` und `LadenAsync` bleiben produktneutral. Konfigurationswerte: AdresseModellserver, Modellname, Systemanweisung, Temperatur, MaximaleAntwortlaenge, Eingabegrenze, ZeitlimitSekunden (Abschnitt `Chat`). Keine Änderung an den Signaturen aus `agent/Schnittstellen.md`; ist eine nötig, anhalten und melden.

## 6. Tests

- Automatisiert (xUnit, Fake-Adapter, Speicher im Arbeitsspeicher): die neun ursprünglich festgelegten Tests sowie `Konfiguration_ModellnameLeer_WirftKonfigurationUngueltig`, Lebenszyklusverlust vor und während des Streams, regulärer und vorzeitiger SSE-Abschluss sowie die Unterscheidung von unbekanntem Modell und unbekannter Route.
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
- Das .NET 10 SDK ist installiert; Build und Tests können auf dem Referenzrechner ausgeführt werden.
- Fachliche Freigabe: Das Gruppenmitglied hat die Übernahme des Zielbilds und die Schätzung von einem Halbtag am 23.09.2026 bestätigt.
- Prüfungen der Vorbereitung: `git diff --check` ohne Fehler; die Umsetzungsprüfungen werden im Ergebnis protokolliert.

---

## Auftrag an das Werkzeug (Text, den jemand in den Chat des Werkzeugs gibt)

```text
Lies AGENTS.md und agent/README.md. Setze die Karte agent/use_cases/AP07-backend-senden.md um.
Halte dich an die Grenzen in Feld 2 und 3. Nenne zuerst deinen Plan in drei bis fünf Sätzen und warte auf mein OK.
Baue in den drei Teilschritten aus Feld 7, nach jedem Teilschritt dotnet build und dotnet test.
Die Tests aus Feld 6 müssen grün sein. Schreibe deinen Bericht nach AGENTS.md Abschnitt 9 in den Abschnitt Ergebnis dieser Karte
und eine Zeile in agent/protokolle/KI-Einsatz.md. Kein Commit.
```

## Ergebnis

Karte: AP07 Backend-Teil des Haupt-Use-Case streamt und bricht ab

Umgesetzt:
- ChatService prüft Eingaben und Konfiguration, liefert die Antwort-ID vor dem Modellaufruf, reicht Teile einzeln weiter und speichert genau einen Endzustand.
- Benutzerabbruch, Gesamtzeitlimit und Verlust des technischen Lebenszyklus werden unterschieden; vorhandene Teile bleiben erhalten.
- ModelRunnerClient sendet den rollengetrennten Verlauf, liest SSE bis zum zwingenden `[DONE]` und übersetzt Transport-, Stream-, Modell- und Zeitfehler in bereinigte Störfälle.
- Zustandsübergänge und alle Grenzfälle der Karte sind automatisiert geprüft.

Geänderte Dateien:
- `src/Chat.Core/ChatService.cs`, `src/Chat.Core/AktiveAnfrage.cs`, `src/Chat.Core/Modelle/Antwort.cs`
- `src/Chat.Adapter.ModelRunner/ModelRunnerClient.cs`
- `tests/Chat.Tests/ChatServiceTests.cs`, `tests/Chat.Tests/ChatServiceAbbruchTests.cs`, `tests/Chat.Tests/ModelRunnerClientTests.cs`
- `agent/use_cases/AP07-backend-senden.md`, `agent/protokolle/KI-Einsatz.md`

Nicht angefasst (bewusst):
- `web_app/LocalAiFront`, `src/Chat.Store.Sqlite`, `docker_compose.yml`, `appsettings*.json`, alle `.csproj` und `global.json`
- Nicht zu AP07 gehörende Operationen von `IChatService` für F03 bis F06

Prüfungen:
- `dotnet build CustomLocalAiFront.slnx --disable-build-servers -m:1`: OK, 0 Warnungen, 0 Fehler
- `dotnet test --project tests/Chat.Tests/Chat.Tests.csproj --no-build`: 16 bestanden, 0 fehlgeschlagen, 1 expliziter Integrationstest nicht ausgeführt
- `dotnet format` für `Chat.Core`, `Chat.Adapter.ModelRunner` und `Chat.Tests`: keine Änderung
- `git diff --check`: OK

Selbstprüfung (`agent/Review_Checkliste.md`):
- Fachlichkeit: ja; Akzeptanzkriterien erfüllt, keine fachliche Erweiterung.
- Namen und Verständlichkeit: ja; Fachbegriffe und Dateigrössen entsprechen dem StylingGuide.
- Schichtgrenzen: ja; nur der Adapter kennt HTTP und das Model-Runner-Protokoll.
- Format, Duplikate und Kommentare: ja; Formatprüfung grün, Kommentar erklärt nur das Beenden der internen Beobachtung.
- Vier Störfälle: ja; nicht erreichbar, unbekanntes Modell, ungültige Konfiguration und Zeitüberschreitung behandelt.
- Abbruch und Teilantwort: ja; vor dem ersten Teil und während des Streams geprüft.
- Eingabegrenzen und Zustandsübergänge: ja; automatisiert geprüft.
- Umfang und Schnittstellen: ja für die zwei in AP07 verwendeten Operationen; die übrigen dokumentierten Operationen gehören zu späteren Karten.
- Konfiguration und Installation: ja; keine Konfigurationsdatei oder Installation geändert.
- Geheimnisse und Datenschutz: ja; keine echten Inhalte, Zugangsdaten oder persönlichen Pfade ergänzt.
- Netzwerk und Abhängigkeiten: ja; nur lokaler Adapterzugriff, keine neue Abhängigkeit.
- Systemanweisung und Benutzereingabe: ja; getrennte Parameter und Rollen, Eingabe vor Client-Aufruf geprüft.
- Oberfläche: nicht anwendbar; Web-Schicht blieb unberührt.
- Automatisierte Tests: ja; 16 bestanden.
- Manuelle Tests: nein; echter Model Runner und Abbruch der Modellerzeugung sind durch ein Gruppenmitglied zu prüfen.
- Nachvollziehbarkeit: ja; KI-Eintrag ergänzt, Commit-Nachricht vorgeschlagen.

Offen, Risiken, Befunde ausserhalb der Karte:
- Der explizite Integrationstest `ModelRunnerClient_EchterServer_LiefertTeile` und der manuelle Abbruchtest gegen Docker Model Runner stehen aus.
- Review und Freigabe durch PS stehen aus; das Werkzeug gibt die Karte nicht selbst frei.
- Die vollständige Solution-Formatprüfung meldet Zeilenenden in `web_app/LocalAiFront/Program.cs`; die Datei liegt ausserhalb von AP07 und wurde nicht geändert. Die drei AP07-Projekte sind formatkonform.
- `IChatService` enthält für AP07 nur Senden und Abbrechen. Die übrigen Operationen aus `agent/Schnittstellen.md` müssen in den zuständigen Karten ergänzt werden, bevor davon abhängige Funktionen umgesetzt werden.

Vorschlag Commit-Nachricht: `AP07: Backend streamt und verwaltet Abbruch` / `KI: Codex, geprueft von PS`

Definition of Done:
- Punkte 2 bis 4 und 7 technisch erfüllt.
- Punkte 1 und 5 benötigen den Lauf gegen Docker Model Runner.
- Punkt 6 benötigt Review und Freigabe durch PS.
- Punkt 8 wird nach Review mit effektiver Dauer und Boardstatus abgeschlossen.
