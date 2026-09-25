# Lokaler KI-Chat HFU Uster

Prototyp eines lokal betriebenen KI-Chats: Blazor-Chat-Seite im Browser, eigenes .NET-Backend, lokaler Modellserver über Docker Model Runner. Läuft ohne Internetzugriff auf einem Referenzgerät (Projektwoche Informatik HFU, Gruppe 1). Massgeblich für Fachanforderungen und Architektur ist das Konzept; im Repository liegen Was und Wie als Markdown unter `agent/Use_Cases.md` und `agent/Design.md`.

## Bekannte Einschränkungen

- Benutzerkonten sind noch nicht implementiert.
- Während der Laufzeit kann noch nicht zwischen verschiedenen Modellen umgeschaltet werden.
- Der Systemprompt kann noch nicht während der Laufzeit angepasst werden.

## Installation und Start

### Voraussetzungen (Windows)

- Docker Engine & Docker Desktop, Model Runner in den Docker Desktop Settings aktiviert
- Python (für das Setup-Skript)
- .NET 10 SDK (nur für lokales Debugging)

### Normaler Start

1. `python install_environment.py` ausführen (erstellt `.env` aus `.env.example` und lädt das Modell über den Model Runner).
2. `docker compose build` ausführen, um den Container zu bauen.
3. `docker compose up -d` ausführen, um den Stack im Hintergrund zu starten.

### Lokales Debugging

1. `python install_environment.py -d` ausführen (kopiert die Environment-Variablen in die Host-Umgebung).
2. In VS Code: Debugger starten mit vorbereitetem Profil (**F5**, nutzt das `https`-Profil aus `src/LocalAiFront/Properties/launchSettings.json`, Umgebung `Development`).
3. Alternativ per Terminal:
   ```bash
   cd src/LocalAiFront
   dotnet run
   ```

### Tests

```bash
dotnet test
```

## Projektaufbau

```
AGENTS.md                    Regeln für KI-Werkzeuge, Einstieg für jedes Tool
README.md                    diese Datei
docker-compose.yml           Anwendung und Docker Model Runner
agent/                       alles für eine Karte an einem Ort (siehe agent/README.md)
  Use_Cases.md, Design.md    Was und Wie, Export aus dem Konzept
  Schnittstellen.md          IChatService, IModelServerClient, IStore, Konfiguration
  use_cases/                 eine Datei je Karte (Auftrag, Ready, Ergebnis)
  protokolle/                KI-Einsatz.md, Test_und_Reviewprotokoll.md
src/
  Chat.Core/                 IChatService, ChatService, Modelle, Konfiguration, Störfall
  Chat.Adapter.ModelRunner/  einziger Ort, der die API des Docker Model Runner kennt
  Chat.Store.Sqlite/         EF Core, SQLite-Store, Migrationen
  LocalAiFront/              Blazor-Chat-Seite, über IChatService mit dem Backend verbunden
tests/
  Chat.Tests/                xUnit-Tests für Service, Adapter, Store und Chat-Seite mit Fakes
data/                        SQLite-Datei und Protokoll zur Laufzeit, nie im Repository
```

Abhängigkeiten zeigen nur nach unten: Die Blazor-Komponenten kennen ausschliesslich `Chat.Core`. Der Composition Root in `Program.cs` verdrahtet `ChatService`, den Model-Runner-Adapter und den SQLite-Store.

## Contributor-Informationen

- Verbindliche Regeln stehen in `AGENTS.md` (Umgebung, erlaubte Befehle, Ablage, Namenskonvention) und `agent/StylingGuide.md` (Codekonventionen C#, Razor, Tests, Git).
- Jede Änderung ist eine Karte in `agent/use_cases/`, angelegt nach `agent/Use_Case_Karte_Vorlage.md` und erst bearbeitet, wenn sie READY ist (`agent/DoR_DoD.md`).
- Ablauf je Karte: Karte lesen und READY prüfen → bauen → `dotnet build`/`dotnet test`/`dotnet format` grün → Review durch eine andere Person → Commit `APxx: Aussage im Präsens`.
- Kein `git push`, kein Force-Push, keine Historienänderung; Geheimnisse, persönliche Pfade und Chatinhalte gehören nie ins Repository.

## Agentisches Arbeiten

KI-Werkzeuge (Claude Code, GitHub Copilot u. a.) setzen ausschliesslich freigegebene, vollständig beschriebene Karten aus `agent/use_cases/` um und erweitern den fachlichen Auftrag nicht selbstständig. Grundlage ist `AGENTS.md`; Claude liest zusätzlich `CLAUDE.md`, das auf dieselbe Datei verweist. Jeder KI-Einsatz wird in `agent/protokolle/KI-Einsatz.md` protokolliert (Karte, Werkzeug, wofür, wie geprüft); die Freigabe einer Karte macht immer ein Mensch, nie das Werkzeug selbst.

## Lizenzen

Dieses Repository steht unter der Lizenz in `LICENSE`. Verwendete Drittkomponenten (NuGet-Pakete, Docker Model Runner, Sprachmodelle) mit jeweiliger Version und Lizenz stehen in `agent/Drittkomponenten.md`; neue Abhängigkeiten werden dort vor der Verwendung eingetragen (`AGENTS.md` Abschnitt 5).

