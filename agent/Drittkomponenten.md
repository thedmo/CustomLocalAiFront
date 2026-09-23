# Drittkomponenten: Versionen, Lizenzen, Weitergabe

Pflicht aus Kursunterlagen S. 13: Liste der Drittkomponenten mit Version und Lizenz; was nicht weitergegeben werden darf, bekommt Bezugsquelle, exakte Version, Prüfsumme und ein Importscript. Jede neue Abhängigkeit kommt hier hinein, bevor sie im Code verwendet wird (`AGENTS.md` Abschnitt 5). Version = exakt wie in `.csproj`, `global.json` oder `docker_compose.yml`. Lizenz = nachgeschlagen, nicht vermutet.

| Komponente | Zweck | Version | Lizenz | Weitergabe im Paket | Bezugsquelle, Prüfsumme | Eingetragen von |
|---|---|---|---|---|---|---|
| .NET SDK und Runtime | Backend, Blazor | 10.0, SDK ab 10.0.100 | MIT | Installer oder Bezugsquelle | dotnet.microsoft.com, Auswahl in `global.json` | Codex, Vorbereitung AP07 |
| ASP.NET Core, Blazor | Web und Chat-Seite | [im SDK] | MIT | mit SDK | | [ ] |
| Microsoft.EntityFrameworkCore.Sqlite | Datenhaltung | [AP06] | MIT | NuGet-Paket im Build | nuget.org | [ ] |
| xunit.v3.mtp-v2 | Tests mit Microsoft Testing Platform | 4.0.1 | Apache 2.0 | nur Entwicklung | nuget.org/packages/xunit.v3.mtp-v2 | Codex, Vorbereitung AP07 |
| bUnit | Komponententests der Blazor-Chat-Seite | 2.9.0 | MIT | nur Entwicklung | nuget.org/packages/bunit/2.9.0 | Codex, F01; freigegeben durch PS am 23.09.2026 |
| Docker Desktop mit Model Runner | Container, Modellserver | [Version des Referenzsystems] | Docker Subscription Service Agreement (für Ausbildung frei) | Bezugsquelle, nicht im Paket | docker.com | [ ] |
| Modell [aus Konzept 6.8] | Sprachmodell | [Tag und Digest] | [Modelllizenz prüfen: Weitergabe, Nutzung] | [Digest und Pull-Befehl, falls Weitergabe nicht erlaubt] | Docker Hub `ai/...` oder Hugging Face | [ ] |
| Bootstrap (optional) | CSS-Grundgerüst | [nur wenn verwendet] | MIT | im Repository | getbootstrap.com | [ ] |

## Regeln

- Kein Paket wird "zum Ausprobieren" hinzugefügt. Erst der Eintrag hier mit Zweck, dann `dotnet add package`.
- Vor der Abgabe stimmt diese Tabelle mit `.csproj`, `global.json` und `docker_compose.yml` überein; das ist ein Punkt der Probeinstallation (AP17).
- Modelldateien und Images, die nicht weitergegeben werden dürfen, stehen mit Digest und Importscript in der Installationsanleitung (Konzept 12).
