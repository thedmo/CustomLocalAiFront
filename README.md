# Lokaler KI-Chat HFU Uster: Umsetzung

Prototyp eines lokal betriebenen KI-Chats (Browser-Frontend, eigenes Backend, lokaler Modellserver) für die Projektwoche Informatik HFU, 21. bis 25.09.2026, Gruppe 1. Die Wahrheit zum Was und Wie steht im Konzept `docs\20260921-010-RL-Konzept.docx` im Projektordner: Kapitel 4.7 Haupt-Use-Case, 5 Ziele, 7 Design, 9 Daten, 11 Tests.

Dieses Verzeichnis ist der Vorschlag für die Wurzel des Repositorys (Arbeitspaket AP06). Zusammengeführt aus zwei Entwürfen (Rod mit Claude, Pascal mit seinem Werkzeug), siehe unten. Alles ist Entwurf, bis die Gruppe es in AP06 gelesen, geändert und freigegeben hat (erste Zeile im Reviewprotokoll).

## Verbindliche Regeln

| Datei                                          | Zweck                                                                                                                                             | Pflicht aus       |
| ---------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------- |
| `AGENTS.md` (Root)                             | Verhaltens- und Prozessregeln für KI-Werkzeuge: Umgebung, Ablage, erlaubt, verboten, Unsicherheit, Ablauf, Abbruchkriterien, Berichtsform, Review | S. 18, Folie 10   |
| `agent/StylingGuide.md`                        | Codekonventionen für C#, Razor, HTML, CSS, Tests, Git; Muster-Code; gilt für KI-Code und Handcode gleich                                          | S. 18, Folie 11   |
| `agent/Uebersicht.md`                          | Wie das Setup zusammenhängt: Dokumentfluss zwischen Konzept, Plan und Repository, Phasen mit ihren Dateien, Änderungsrechte                       | Folie 10          |
| `agent/Ablauf.md`                              | Projektmethode: wer macht was wann, als Tabelle und Sequenz                                                                                       | Folie 10          |
| `agent/DoR_DoD.md`                             | Wann eine Karte begonnen werden darf und wann sie fertig ist                                                                                      | S. 18             |
| `agent/Use_Case_Karte_Vorlage.md`              | Vorlage für jeden Entwicklungsschritt, Felder der Definition of Ready                                                                             | S. 18             |
| `agent/Review_Checkliste.md`                   | Selbstprüfung des Werkzeugs und Review durch Menschen                                                                                             | S. 19, Folie 11   |
| `agent/protokolle/Test_und_Reviewprotokoll.md` | Manuelle Tests und Reviews mit Mängeln, Korrekturen, Nachprüfung                                                                                  | S. 18 f.          |
| `agent/protokolle/KI-Einsatz.md`               | Wofür KI verwendet wurde und wie das Ergebnis geprüft wurde                                                                                       | S. 19             |
| `agent/Drittkomponenten.md`                    | Bibliotheken, Images, Modell mit Version, Lizenz, Weitergabe                                                                                      | S. 13             |
| `agent/Schnittstellen.md`                      | IChatService, IModelServerClient, IStore, Konfiguration, Zustände; Auszug aus Konzept 7.3 und 7.4                                                 | Konzept 7         |
| `agent/Use_Cases.md`                           | Das Was: Funktionsliste, nichtfunktionale Anforderungen, Haupt-Use-Case F01 mit SSD und Zuständen, Ziele mit Nachweis; Export aus dem Konzept     | Konzept 4, 5      |
| `agent/Design.md`                              | Das Wie: Architektur, Komponenten, Klassendiagramm, Datenfluss, Konfiguration, Oberfläche, Daten; Export aus dem Konzept, Diagramme als Mermaid   | Konzept 7 bis 9   |
| `agent/Testfaelle.md`                          | Nummerierte Testfälle, funktional und nichtfunktional, mit Art und Nachweis; Karten verweisen auf die Nummern                                     | Konzept 11, S. 12 |
| `docker_compose.yml`                           | Compose-Konfiguration für Anwendung und Docker Model Runner                                                                                       | Konzept 6.5, 12   |

## Der ganze Workflow: vom Konzept zum Commit

| Schritt               | Quelle                                                                                    | Ergebnis                                                                                                             | Wer                                       |
| --------------------- | ----------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- | ----------------------------------------- |
| Was und Wie festlegen | Konzept (Word, Master)                                                                    | Export nach `agent/Use_Cases.md`, `agent/Design.md`, `agent/Schnittstellen.md`; Testfälle nach `agent/Testfaelle.md` | Projektleitung nach jeder Konzeptänderung |
| Karte anlegen         | Projektplan (Arbeitspaket oder Teilschritt)                                               | `agent/use_cases/APxx-titel.md` nach `agent/Use_Case_Karte_Vorlage.md`, acht Felder der Definition of Ready          | Karteninhaber                             |
| READY prüfen          | `agent/DoR_DoD.md`                                                                        | Karte READY oder Blockiert                                                                                           | Karteninhaber, Daily                      |
| Bauen                 | `AGENTS.md`, `agent/StylingGuide.md`, Karte, `agent/Design.md`, `agent/Schnittstellen.md` | Code, Tests, Diff; Bericht im Abschnitt Ergebnis der Karte; Zeile in `agent/protokolle/KI-Einsatz.md`                | KI-Werkzeug mit Karteninhaber             |
| Prüfen                | `agent/Testfaelle.md`, `agent/Review_Checkliste.md`                                       | Zeilen im `agent/protokolle/Test_und_Reviewprotokoll.md` (manuelle Tests, Review)                                    | Karteninhaber, Reviewer                   |
| DONE                  | `agent/DoR_DoD.md`                                                                        | Kontrollkästchen in der Karte, Commit `APxx: ...`, Board Erledigt, Dauer effektiv                                    | Karteninhaber                             |
| Abschliessen          | Projektplan, Konzept                                                                      | Soll-Ist im Plan, Abweichungen in Konzept 14.3, Einschränkungen in 14.4                                              | Projektleitung, Abendblock                |

Wer wann was tut, als Bild und Sequenz: `agent/Ablauf.md`.

## Geplante Struktur (Konzept 7.1)

```
002_Umsetzung/
  AGENTS.md                    Einstieg für jedes KI-Werkzeug (Folie 10: zentral in der Root), zeigt auf agent/
  README.md                    diese Datei
  .editorconfig, .gitignore, appsettings.example.json, docker_compose.yml, global.json [AP06]
  agent/                       alles, was das Werkzeug und die Menschen für eine Karte brauchen, an einem Ort
    README.md                  Leseordnung und Zweck jeder Datei
    Ablauf.md, DoR_DoD.md, StylingGuide.md, Review_Checkliste.md      Regeln (fest)
    Use_Cases.md, Design.md, Schnittstellen.md, Testfaelle.md         Was, Wie, Prüfung (Export aus dem Konzept)
    Use_Case_Karte_Vorlage.md, Drittkomponenten.md
    use_cases/                 eine Datei je Karte (lebend)
    protokolle/                KI-Einsatz.md, Test_und_Reviewprotokoll.md (lebend)
  src/
    Chat.Web/                  Blazor Interactive Server: Chat-Seite, Komponenten, CSS im HFU-Design
    Chat.Core/                 IChatService, ChatService, Modelle, IModelServerClient, IStore, Konfiguration, Störfall
    Chat.Adapter.ModelRunner/  einziger Ort, der die API des Docker Model Runner kennt
    Chat.Store.Sqlite/         Entity Framework Core, SQLite, Protokoll
  tests/
    Chat.Tests/                xUnit: ChatService mit Fake-Adapter und Speicher im Arbeitsspeicher
  data/                        SQLite-Datei und Protokoll zur Laufzeit, nie im Repository
```

Abhängigkeiten nur nach unten: `Chat.Web` kennt `Chat.Core`, nie den Adapter oder den Store direkt. Die Verdrahtung passiert in `Program.cs` per Dependency Injection.

## Start (wird in AP06 ergänzt)

### Entwicklung / Debugging (lokal, ausserhalb Docker)

Voraussetzung: Docker Model Runner ist in Docker Desktop aktiviert und der TCP-Zugriff ist freigegeben. Die lokale App verbindet sich direkt mit der OpenAI-kompatiblen API des Model Runners.

```
python install_environment.py          # einmalig: appsettings.Development.json anlegen, Modell laden
docker compose -f docker_compose.yml up           # Anwendung und Model Runner starten
```

Danach in VS Code **F5** drücken (nutzt das `https`-Profil aus `web_app/LocalAiFront/Properties/launchSettings.json`, Umgebung `Development`) oder:

```
cd web_app/LocalAiFront
dotnet run
```

### Produktion / gesamter Stack (Docker)

```
docker compose -f docker_compose.yml up      Anwendung und Docker Model Runner starten
dotnet test                                  alle Tests
dotnet format                                Formatierung nach .editorconfig
```

Das Gateway ist danach unter `http://localhost:80` erreichbar. Das zu ladende Modell wird zentral in `.env` über `MODEL_NAME` festgelegt. Der Wert wird sowohl für Docker Model Runner als auch für die Anwendung verwendet.

## Zusammenführung der zwei Entwürfe

Aus Pascals `Agent_Setup` übernommen: das Antwortmuster des Werkzeugs (jetzt `AGENTS.md` Abschnitt 9), das Übergabeformat mit Kontrollkästchen (in `agent/Use_Case_Karte_Vorlage.md`), die Review-Checkliste als eigene Datei, die Schnittstellen-Datei, das Abbruchkriterium "sicherheitsrelevante Auswirkungen nicht beurteilbar", die Regel "Verständlichkeit vor Raffinesse". Ersetzt: die allgemeinen Guidelines durch den StylingGuide mit C#-, Razor-, HTML- und CSS-Regeln (Folie 11 verlangt Regeln je Sprache); Dateinamen nach den Kursunterlagen (`agent/StylingGuide.md`) statt Grossbuchstaben. Ergänzt: Umgebung und erlaubte Befehle, Namenskonvention, Git-Regeln, Test- und Reviewprotokoll mit den neun Pflichtfeldern, KI-Einsatz, Drittkomponenten, Bezug auf Konzeptkapitel und Störfälle. Der Ordner `Agent_Setup` kann nach der Abnahme in AP06 gelöscht werden; hier steht die zusammengeführte Fassung.

## Was in AP06 zu tun ist (eine Stunde zu dritt)

1. `AGENTS.md`, `agent/StylingGuide.md`, `agent/DoR_DoD.md`, `agent/Ablauf.md` laut lesen, ändern, freigeben. Erste Zeile im Reviewprotokoll: das Gerüst selbst.
2. .NET-Version festlegen (`global.json`), Lösung mit den fünf Projekten anlegen, `dotnet build` grün.
3. `docker_compose.yml` mit Anwendung und Model Runner; `appsettings.example.json` gegen Konzept 7.4 prüfen; Adresse des Model Runner aus dem Spike in `agent/Schnittstellen.md` eintragen.
4. Erste Karte nach `agent/Use_Case_Karte_Vorlage.md` anlegen: AP07 Backend senden (F01, F07).
5. Konzept Kapitel 15: ein Absatz, der auf diese Dateien zeigt.
