# Karte F01: Chat-Seite zeigt die Antwort schrittweise und bricht sie ab (F01, F02)

**Status:** Umgesetzt, Review offen. Verantwortlichkeit, Halbtag, Gesamtumfang und bUnit als neue Testabhängigkeit wurden durch PS am 23.09.2026 freigegeben.

| Feld | Inhalt |
|---|---|
| Verantwortlich | PS (Pascal Schmidiger) |
| Halbtag | Mi PM |
| Referenz | F01 Nachricht senden, F02 Abbrechen; Z03 Antwort in Teilen; M05, M06 |
| Quellen | `agent/Use_Cases.md` Haupt-Use-Case Schritte 1, 2, 4 und 6 sowie Erweiterungen 2a, 2b, 3a, 3b, 4a und 4b; `agent/Design.md` Oberfläche und Datenfluss; `agent/Schnittstellen.md` `IChatService` |
| Testfälle | T02, T03, T04, T07, T09, T10, T11 aus `agent/Testfaelle.md` |

## 1. Ziel (ein Satz)

Die Chat-Seite sendet eine gültige Nachricht über `IChatService`, zeigt jeden gelieferten Antwortteil sofort und in derselben Antwort an, sperrt währenddessen weitere Eingaben und ermöglicht den Abbruch der laufenden Antwort.

## 2. Abgrenzung (gehört nicht dazu)

Kein direkter HTTP-Aufruf der Seite zum Model Runner. Keine Änderung am ModelRunnerClient (AP07), keine dauerhafte Speicherung über einen Neustart hinaus (AP09) und kein technisches Dateiprotokoll (AP12). Der für F01 notwendige Arbeitsspeicher-Store und `NeueUnterhaltungAsync` gehören nach ausdrücklicher Freigabe zu dieser Karte.

## 3. Betroffene Komponenten und Dateien

- `src/Chat.Core`: `IChatService.cs`, `ChatService.cs`, neuer `ArbeitsspeicherStore.cs`
- `web_app/LocalAiFront`: `LocalAiFront.csproj`, `Program.cs`, `Components/_Imports.razor`, `Components/Pages/Home.razor`, neuer `Components/Pages/Home.razor.css`, `Dockerfile`
- `tests/Chat.Tests`: `Chat.Tests.csproj`, neue `ChatSeiteTests.cs`, neuer `FakeChatService.cs`
- Betrieb und Dokumentation: `.dockerignore`, `docker_compose.yml`, `README.md`, `agent/Drittkomponenten.md`, `agent/protokolle/KI-Einsatz.md`
- Unberührt: `src/Chat.Adapter.ModelRunner/ModelRunnerClient.cs`, `src/Chat.Store.Sqlite`, `global.json`, `appsettings*.json`

## 4. Akzeptanzkriterien

- [x] Die eigene Nachricht erscheint nach dem Senden sofort im Verlauf.
- [x] Mindestens zwei zeitlich getrennt gelieferte Teile werden einzeln und in Reihenfolge in derselben Antwort angezeigt; der erste Teil ist sichtbar, bevor der Stream abgeschlossen ist (T03).
- [x] Während der Stream läuft, sind Nachrichteneingabe und erneutes Senden gesperrt; nach Abschluss oder Störfall sind sie wieder verfügbar.
- [x] Die Abbruchbedienung beendet die laufende Antwort über ihre Antwort-ID. Bereits angezeigte Teile bleiben sichtbar, die Eingabe wird wieder frei und eine weitere Frage kann gesendet werden (T04).
- [x] Leere oder zu lange Eingaben und die dokumentierten Störfälle werden verständlich angezeigt, ohne die Seite unbedienbar zu machen.
- [x] Die Seite spricht ausschliesslich `IChatService` an und kennt weder Adapter noch Store.

## 5. Schnittstellen und Konfiguration

Die Seite verwendet nur `IChatService.NeueUnterhaltungAsync`, `SendeNachrichtAsync` und `AbbrechenAsync`. Sie liest `AntwortLauf.AntwortId` und konsumiert `AntwortLauf.Teile` fortlaufend. `NeueUnterhaltungAsync` ist bereits in `agent/Schnittstellen.md` dokumentiert und wird in dieser Karte im bestehenden C#-Vertrag ergänzt. Program.cs verdrahtet die Umsetzungen; die Razor-Komponente kennt weder Adapter noch Store.

## 6. Tests

- Automatisiert mit bUnit 2.9.0: Komponententest mit steuerbarem Fake-Service für zwei getrennte Teile; Eingabesperre; Abschluss; Abbruch mit sichtbarer Teilantwort; Störfallanzeige.
- Manuell: T03 und T04 im Browser gegen den lokalen Model Runner; erwartetes und tatsächliches Ergebnis in `agent/protokolle/Test_und_Reviewprotokoll.md` eintragen.

## 7. Grösse

Ein Halbtag. PS hat am 23.09.2026 bestätigt, dass Oberfläche, notwendige Backend-Verdrahtung und Komponententests gemeinsam in F01 umgesetzt und nicht aufgeteilt werden.

## 8. Board

Projektplan: F01, Mi PM, Verantwortlich PS, Status In Arbeit; WIP von PS damit 1.

---

## Ergebnis

Karte: F01 Chat-Seite zeigt die Antwort schrittweise und bricht sie ab

Umgesetzt:
- Die bisherige direkte HTTP-Testseite wurde durch eine Chat-Seite ersetzt, die ausschliesslich `IChatService` verwendet.
- Eigene Nachricht und Antwort erscheinen im Verlauf; jeder Streamteil aktualisiert dieselbe Antwort sofort.
- Während der Antwort sind Eingabe und Senden gesperrt. Abbrechen beendet die aktive Antwort, markiert und erhält den vorhandenen Text und erlaubt eine weitere Frage.
- Störfälle erscheinen mit bereinigtem Grund und geben die Eingabe wieder frei.
- `NeueUnterhaltungAsync`, ein Arbeitsspeicher-Store bis AP09 und die vollständige Dependency Injection stellen einen lauffähigen F01-Ablauf bereit.
- Der Docker-Buildkontext umfasst die drei referenzierten Projekte; das Image wurde gebaut und der Container als gesund gestartet.

Geänderte Dateien:
- `src/Chat.Core/IChatService.cs`, `src/Chat.Core/ChatService.cs`, `src/Chat.Core/ArbeitsspeicherStore.cs`
- `web_app/LocalAiFront/LocalAiFront.csproj`, `Program.cs`, `Components/_Imports.razor`, `Components/Pages/Home.razor`, `Components/Pages/Home.razor.css`, `Dockerfile`
- `tests/Chat.Tests/Chat.Tests.csproj`, `ChatServiceTests.cs`, `ChatSeiteTests.cs`, `FakeChatService.cs`
- `.dockerignore`, `docker_compose.yml`, `README.md`, `agent/Drittkomponenten.md`, `agent/protokolle/KI-Einsatz.md`, diese Karte

Nicht angefasst (bewusst):
- `src/Chat.Adapter.ModelRunner/ModelRunnerClient.cs`, `src/Chat.Store.Sqlite`, `global.json`, `appsettings*.json`
- Dauerhafte Speicherung, Unterhaltungsverwaltung über mehrere Unterhaltungen und technisches Dateiprotokoll

Prüfungen:
- `dotnet build CustomLocalAiFront.slnx --no-restore --disable-build-servers -m:1`: OK, 0 Fehler; eine NU1900-Warnung, weil der NuGet-Sicherheitsindex in der Umgebung nicht erreichbar ist
- `dotnet test --project tests/Chat.Tests/Chat.Tests.csproj --no-build`: 21 bestanden, 0 fehlgeschlagen, 1 expliziter AP07-Integrationstest nicht ausgeführt
- `dotnet format` für `Chat.Core`, `LocalAiFront` und `Chat.Tests`: keine Änderung
- `docker compose -f docker_compose.yml up -d --build`: Image gebaut, Container gestartet
- `docker compose -f docker_compose.yml up -d --wait`: Container `Healthy`
- `git diff --check`: OK

Selbstprüfung (`agent/Review_Checkliste.md`):
- Fachlichkeit: ja; F01/F02, Z03 und M06 sind in Komponententests abgedeckt.
- Akzeptanzkriterien und Umfang: ja; alle Kriterien automatisiert erfüllt, keine Funktion ausserhalb der freigegebenen Gesamtkarte ergänzt.
- Namen und Verständlichkeit: ja; deutsche Fachbegriffe, technische Begriffe und Dateigrössen entsprechen dem StylingGuide.
- Schichtgrenzen: ja; `Home.razor` injiziert nur `IChatService`; Adapter und Store werden ausschliesslich im Composition Root verdrahtet.
- Format und Kommentare: ja; Formatprüfung grün, kein auskommentierter Code.
- Fehlerbehandlung: ja; Störfallgrund wird angezeigt, Eingabe danach freigegeben.
- Abbruch und Teilantwort: ja; sichtbarer Teil bleibt markiert, zweite Frage funktioniert.
- Eingaben: ja; leere Eingabe ist gesperrt, zu lange Eingabe wird vom ChatService abgewiesen und als Störfall angezeigt.
- Auswirkungen und Schnittstellen: ja; `NeueUnterhaltungAsync` entspricht dem bereits dokumentierten Vertrag.
- Konfiguration und Installation: ja; vorhandene Werte werden im Composition Root gebunden, Container-Build und README angepasst.
- Sicherheit und Datenschutz: ja; keine Geheimnisse, persönlichen Daten oder Chattexte in Protokollen; kein direkter Modellserverzugriff aus der Seite.
- Neue Abhängigkeit: ja; bUnit 2.9.0, MIT, durch PS freigegeben und in `Drittkomponenten.md` dokumentiert.
- Semantik und Tastatur: ja; beschriftetes Textfeld, echte Buttons, Statusbereich und `role=alert`.
- Gestaltung und Fensterbreiten: CSS verwendet Bootstrap-Variablen und einen mobilen Breakpoint; visuelle Prüfung beider Breiten steht aus.
- Automatisierte Tests: ja; Streaming vor Abschluss, Sperre, Abbruch, zweite Frage, Störfall und leere Eingabe geprüft.
- Manuelle Tests: nein; Browser-Automationsdienst war nicht verfügbar, T03/T04 sind durch PS im Browser zu prüfen.
- Nachvollziehbarkeit: ja; Karte, Drittkomponente, README und KI-Einsatz aktualisiert.

Offen, Risiken, Befunde ausserhalb der Karte:
- PS prüft T03 und T04 unter `http://localhost` bei Desktop- und Mobilbreite und trägt das Ergebnis ins Test- und Reviewprotokoll ein.
- Review und Freigabe durch ein Gruppenmitglied stehen aus; das Werkzeug gibt die Karte nicht selbst frei.
- Docker meldet bereits vorhandene verwaiste Container `litellm` und `e41776f9cc56_llm-response-demo-container`. Sie wurden nicht verändert oder entfernt.
- Die NuGet-Auditabfrage war in der Umgebung nicht erreichbar; Paketwiederherstellung und Build waren erfolgreich.

Vorschlag Commit-Nachricht: `F01: Chat-Seite zeigt Antworten schrittweise` / `KI: Codex, geprueft von PS`

Definition of Done:
- Punkte 1 bis 4 und 7 technisch erfüllt.
- Punkt 5 benötigt den manuellen Eintrag von PS.
- Punkt 6 benötigt Review und Freigabe durch ein Gruppenmitglied.
- Punkt 8 wird nach Review mit effektiver Dauer und Boardstatus abgeschlossen.
