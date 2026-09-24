# Drittkomponenten: Versionen, Lizenzen, Weitergabe

Pflicht aus Kursunterlagen S. 13: Liste der Drittkomponenten mit Version und Lizenz; was nicht weitergegeben werden darf, bekommt Bezugsquelle, exakte Version, Prüfsumme und ein Importscript. Jede neue Abhängigkeit kommt hier hinein, bevor sie im Code verwendet wird (`AGENTS.md` Abschnitt 5). Version = exakt wie in `.csproj`, `global.json` oder `docker_compose.yml`. Lizenz = nachgeschlagen, nicht vermutet.

| Komponente | Zweck | Version | Lizenz | Weitergabe im Paket | Bezugsquelle, Prüfsumme | Eingetragen von |
|---|---|---|---|---|---|---|
| .NET SDK und Runtime | Backend, Blazor | 10.0, SDK ab 10.0.100 | MIT | Installer oder Bezugsquelle | dotnet.microsoft.com, Auswahl in `global.json` | Codex, Vorbereitung AP07 |
| ASP.NET Core, Blazor | Web und Chat-Seite | [im SDK] | MIT | mit SDK | | [ ] |
| Microsoft.EntityFrameworkCore.Sqlite | SQLite-Datenhaltung, AP09a | 10.0.12 | MIT | NuGet-Paket im Build | [Paket](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite/10.0.12), [Lizenz](https://github.com/dotnet/efcore/blob/main/LICENSE.txt) | Codex, Vorbereitung AP09a; freigegeben durch PS am 23.09.2026 |
| Microsoft.EntityFrameworkCore.Design | Migrationserzeugung, AP09a; PrivateAssets | 10.0.12 | MIT | nur Entwicklung | [Paket](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Design/10.0.12), [Lizenz](https://github.com/dotnet/efcore/blob/main/LICENSE.txt) | Codex, Vorbereitung AP09a; freigegeben durch PS am 23.09.2026 |
| dotnet-ef | Lokales Werkzeug für Migrationserzeugung, AP09a | 10.0.12 | MIT | nur Entwicklung | [Paket](https://www.nuget.org/packages/dotnet-ef/10.0.12), [Lizenz](https://github.com/dotnet/efcore/blob/main/LICENSE.txt) | Codex, Vorbereitung AP09a; freigegeben durch PS am 23.09.2026 |
| Microsoft.Data.Sqlite.Core, Microsoft.EntityFrameworkCore.Sqlite.Core | Transitive SQLite-Anbindung des freigegebenen EF-Providers | 10.0.12 | MIT | mit Anwendung | [Paket](https://www.nuget.org/packages/Microsoft.Data.Sqlite.Core/10.0.12), EF-Lizenz oben | Codex, Auflösung AP09a am 24.09.2026 |
| SQLitePCLRaw.bundle_e_sqlite3, core, provider.e_sqlite3, lib.e_sqlite3 | Transitive native SQLite-Anbindung des freigegebenen Providers | 2.1.12 | Apache-2.0; SQLite-Kern Public Domain | mit Anwendung, Lizenzhinweise beilegen | [Paket](https://www.nuget.org/packages/SQLitePCLRaw.bundle_e_sqlite3/2.1.12), [Lizenz](https://github.com/ericsink/SQLitePCL.raw/blob/main/LICENSE.TXT), [SQLite](https://www.sqlite.org/copyright.html) | Codex, Auflösung AP09a am 24.09.2026 |
| xunit.v3.mtp-v2 | Tests mit Microsoft Testing Platform | 4.0.1 | Apache 2.0 | nur Entwicklung | nuget.org/packages/xunit.v3.mtp-v2 | Codex, Vorbereitung AP07 |
| bUnit | Komponententests der Blazor-Chat-Seite | 2.9.0 | MIT | nur Entwicklung | nuget.org/packages/bunit/2.9.0 | Codex, F01; freigegeben durch PS am 23.09.2026 |
| Docker Desktop mit Model Runner | Container, Modellserver | [Version des Referenzsystems] | Docker Subscription Service Agreement (für Ausbildung frei) | Bezugsquelle, nicht im Paket | docker.com | [ ] |
| Modell [aus Konzept 6.8] | Sprachmodell | [Tag und Digest] | [Modelllizenz prüfen: Weitergabe, Nutzung] | [Digest und Pull-Befehl, falls Weitergabe nicht erlaubt] | Docker Hub `ai/...` oder Hugging Face | [ ] |
| Bootstrap (optional) | CSS-Grundgerüst | [nur wenn verwendet] | MIT | im Repository | getbootstrap.com | [ ] |

## Regeln

- AP09a wurde durch PS am 23.09.2026 technisch freigegeben. Die direkten Versionen stimmen mit Projektdatei und lokalem Werkzeugmanifest überein; die SQLite-Abhängigkeiten wurden am 24.09.2026 mit der tatsächlichen Paketauflösung abgeglichen. Lizenzhinweise für das endgültige Abgabepaket bleiben Bestandteil von AP17.

- Kein Paket wird "zum Ausprobieren" hinzugefügt. Erst der Eintrag hier mit Zweck, dann `dotnet add package`.
- Vor der Abgabe stimmt diese Tabelle mit `.csproj`, `global.json` und `docker_compose.yml` überein; das ist ein Punkt der Probeinstallation (AP17).
- Modelldateien und Images, die nicht weitergegeben werden dürfen, stehen mit Digest und Importscript in der Installationsanleitung (Konzept 12).
