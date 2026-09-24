# KI-Einsatz: wofür KI verwendet wurde und wie das Ergebnis geprüft wurde

Pflicht aus Kursunterlagen S. 19: Prompts und Dialoge müssen nicht lückenlos abgegeben werden, aber bei wichtigen oder umfangreichen Änderungen muss erkennbar sein, wofür KI verwendet wurde und wie das Ergebnis geprüft wurde. Eine Zeile je Karte oder je grösserer Änderung. Die Nutzung von KI wird nicht negativ bewertet; fehlende Nachvollziehbarkeit schon.

Nie an ein externes KI-Werkzeug senden: Zugangsdaten, personenbezogene Informationen, vertrauliche Inhalte, nicht freigegebener fremder Code.

| Datum | Karte | Werkzeug | Wofür (Entwurf, Code, Test, Fehlersuche, Doku) | Ergebnis übernommen als | Wie geprüft | Person |
|---|---|---|---|---|---|---|
| 22.09.2026 | AP06 | Claude Code | Entwurf dieser Regeldateien | Vorlage, in AP06 gelesen und geändert | Gruppe liest jede Regel, streicht oder ändert; Zeile 1 im Reviewprotokoll | RL |
| 23.09.2026 | Vorbereitung AP07 | Codex | Ist-Analyse, Projektgerüste und Abgleich von Design, Schnittstellen, Tests, Karte und README nach Entfernung von LiteLLM | Änderungen im Arbeitsverzeichnis | Diff, Querverweissuche; Build blockiert, weil kein .NET SDK installiert ist | offen |
| 23.09.2026 | AP07 | Codex | ChatService, Lebenszyklus und Abbruch, Model-Runner-SSE-Adapter, Fehlerabbildung, automatisierte Tests und Kartenbericht | Code und Tests im Arbeitsverzeichnis | Solution-Build 0 Warnungen/0 Fehler; 16 Tests bestanden; AP07-Projekte formatkonform; Integration und menschliches Review offen | offen |
| 23.09.2026 | F01 | Codex | Chat-Seite mit Streaming und Abbruch, Backend-Verdrahtung, Arbeitsspeicher-Store, Docker-Build und bUnit-Komponententests | Code, Tests und Dokumentation im Arbeitsverzeichnis | 21 Tests bestanden; Formatprüfung grün; Container gebaut und Healthy; manueller Browsertest und Review offen | PS |
| 23.09.2026 | F03 | GitHub Copilot | neue Unterhaltung im UI, vertikale Liste, aktive Hervorhebung, Scroll- und Layoutkorrektur, bUnit-Test-Setup für JS-Interop | Code und Tests im Arbeitsverzeichnis | ChatSeite-Regression: 5/5 Tests grün; Diff geprüft | offen |
| 23.09.2026 | Vorbereitung F04/AP09a/AP09b | Codex | F03-Ist-Abgleich, F04 konkretisiert, zwei Persistenz-Teilkarten erstellt; Pakete und Lizenzen recherchiert | Arbeitsgrundlagen und ausdrücklich unfreigegebene Vorschläge | Code/Design/Schnittstellen/Testfälle abgeglichen, Dokument-Diff und git diff --check; kein Anwendungscode geändert, kein Build/Test; Entscheide und Review offen | offen |
| 24.09.2026 | AP09a/AP09b/F04 | Codex | SQLite-Projekt/Migration/Store, dauerhafte Teilspeicherung, Neustart, Unterhaltungsliste und Öffnen; Container-Startfehler behoben | Code, Tests und Dokumentation im Arbeitsverzeichnis; technische Freigabe PS vom 23.09. | Build 0 Warnungen/0 Fehler; 45 Tests bestanden, 1 vorhandener expliziter Modellserver-Test nicht ausgeführt; globale Formatprüfung grün; 5 Verläufe nach echtem isoliertem Containerneustart geprüft; localhost:80 HTTP 200; visuelle Prüfung und menschliches Review offen | PS (Auftrag), Review offen |

## Wo KI im Code beteiligt war

Zusätzlich zur Tabelle trägt jede Commit-Nachricht mit KI-Beteiligung die Zeile `KI: Werkzeug, geprueft von XX` (`AGENTS.md` Abschnitt 10). Wer in der Präsentation nach einem Teil gefragt wird, erklärt ihn selbst; das ist der Massstab für "geprüft".
