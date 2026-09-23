# Drittkomponenten: Versionen, Lizenzen, Weitergabe

Pflicht aus Kursunterlagen S. 13: Liste der Drittkomponenten mit Version und Lizenz; was nicht weitergegeben werden darf, bekommt Bezugsquelle, exakte Version, Prüfsumme und ein Importscript. Jede neue Abhängigkeit kommt hier hinein, bevor sie im Code verwendet wird (`AGENTS.md` Abschnitt 5). Version = exakt wie in `.csproj`, `global.json` oder `docker-compose.yml`. Lizenz = nachgeschlagen, nicht vermutet.

| Komponente | Zweck | Version | Lizenz | Weitergabe im Paket | Bezugsquelle, Prüfsumme | Eingetragen von |
|---|---|---|---|---|---|---|
| .NET SDK und Runtime | Backend, Blazor | [AP06] | MIT | Installer oder Bezugsquelle | dotnet.microsoft.com, Version im Installer | [ ] |
| ASP.NET Core, Blazor | Web und Chat-Seite | [im SDK] | MIT | mit SDK | | [ ] |
| Microsoft.EntityFrameworkCore.Sqlite | Datenhaltung | [AP06] | MIT | NuGet-Paket im Build | nuget.org | [ ] |
| xunit, xunit.runner.visualstudio | Tests | [AP06] | Apache 2.0 | nur Entwicklung | nuget.org | [ ] |
| Docker Desktop mit Model Runner | Container, Modellserver | [Version des Referenzsystems] | Docker Subscription Service Agreement (für Ausbildung frei) | Bezugsquelle, nicht im Paket | docker.com | [ ] |
| Modell [aus Konzept 6.8] | Sprachmodell | [Tag und Digest] | [Modelllizenz prüfen: Weitergabe, Nutzung] | [Digest und Pull-Befehl, falls Weitergabe nicht erlaubt] | Docker Hub `ai/...` oder Hugging Face | [ ] |
| Bootstrap (optional) | CSS-Grundgerüst | [nur wenn verwendet] | MIT | im Repository | getbootstrap.com | [ ] |

## Regeln

- Kein Paket wird "zum Ausprobieren" hinzugefügt. Erst der Eintrag hier mit Zweck, dann `dotnet add package`.
- Vor der Abgabe stimmt diese Tabelle mit `.csproj`, `global.json` und `docker-compose.yml` überein; das ist ein Punkt der Probeinstallation (AP17).
- Modelldateien und Images, die nicht weitergegeben werden dürfen, stehen mit Digest und Importscript in der Installationsanleitung (Konzept 12).
