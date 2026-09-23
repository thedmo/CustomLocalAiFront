# Review-Checkliste

Für die Selbstprüfung des KI-Werkzeugs (AGENTS.md Abschnitt 7) und für das Review durch einen Menschen. Jeder Punkt wird mit ja oder nein beantwortet; ein nein braucht einen Grund oder eine Korrektur. Ein Review darf nicht ausschliesslich durch dasselbe KI-Werkzeug erfolgen, das den Code erzeugt hat; die Freigabe macht ein Gruppenmitglied (Kursunterlagen S. 19).

## Fachlichkeit

- [ ] Umsetzung entspricht der Karte, dem Use-Case-Text 4.7 und der Nachweis-Spalte in Konzept Kapitel 5.
- [ ] Alle Akzeptanzkriterien der Karte sind erfüllt.
- [ ] Keine fachliche Erweiterung ohne Auftrag.

## Codequalität

- [ ] Namen nach `agent/StylingGuide.md` Abschnitt 1, Fachbegriffe wie im Glossar.
- [ ] Schichtgrenzen eingehalten: `Chat.Web` kennt nur `Chat.Core`; nur der Adapter kennt den Model Runner.
- [ ] Verständlich und wartbar; jedes Gruppenmitglied könnte den Teil in der Präsentation erklären.
- [ ] `dotnet format` ohne Änderung; keine Duplikate, keine unnötige Komplexität.
- [ ] Kommentare erklären das Warum; kein auskommentierter Code.

## Fehlerbehandlung und Grenzfälle

- [ ] Die vier Störfälle (Konzept 7.3) sind behandelt: nicht erreichbar, Modell unbekannt, Konfiguration ungültig, Zeitüberschreitung.
- [ ] Abbruch führt zu Zustand Abgebrochen, Teilantwort bleibt; kein leeres catch, kein verschluckter Fehler.
- [ ] Leere und zu lange Eingaben werden vor dem Aufruf des Modellservers abgewiesen (4.7 Erweiterungen).
- [ ] Zustandsübergänge der Antwort nur wie im Zustandsdiagramm 4.7.6.

## Auswirkungen

- [ ] Keine Änderung ausserhalb des vereinbarten Umfangs (Diff gelesen, Dateiliste stimmt mit der Karte überein).
- [ ] Schnittstellen aus Konzept 7.3 unverändert oder Änderung freigegeben und in `agent/Schnittstellen.md` nachgeführt.
- [ ] Konfiguration (7.4) und Installation bleiben nachvollziehbar; `appsettings.example.json` aktuell.

## Sicherheit und Abhängigkeiten (S. 10, S. 13)

- [ ] Keine Geheimnisse, persönlichen Daten, Pfade oder Chatinhalte im Repository oder in Protokollen.
- [ ] Kein Netzwerkzugriff ausser zum Modellserver über den Adapter; keine ausführbaren Werkzeuge für das Modell.
- [ ] Neue Bibliotheken: notwendig, Version und Lizenz in `agent/Drittkomponenten.md`.
- [ ] Systemanweisung und Benutzereingabe getrennt; Eingaben an der Grenze validiert (Prompt Injection wird nicht verhindert, aber nicht verschlimmert).

## Oberfläche (nur bei Änderungen in `Chat.Web`)

- [ ] Semantisches HTML, Beschriftungen für Eingaben, Tastatur bedienbar (`agent/StylingGuide.md` Abschnitt 8).
- [ ] Farben, Schrift, Abstände nur über CSS-Variablen; Corporate Design eingehalten (M07).
- [ ] Beide Fensterbreiten geprüft.

## Tests

- [ ] `dotnet test` grün, Ausgabe im Bericht.
- [ ] Neue Logik hat neue Tests; je Störfall und je Zustandspfeil ein Test.
- [ ] Manuelle Tests durchgeführt und im `agent/protokolle/Test_und_Reviewprotokoll.md` eingetragen.
- [ ] Bekannte Einschränkungen und Restpunkte im Bericht und, wenn sie bleiben, in Konzept 14.4.

## Nachvollziehbarkeit

- [ ] `agent/protokolle/KI-Einsatz.md` hat eine Zeile zur Karte, wenn KI beteiligt war.
- [ ] Commit-Nachricht mit Kartennummer, bei KI-Beteiligung mit Werkzeug und prüfender Person.
