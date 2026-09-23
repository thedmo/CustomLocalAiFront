# AGENTS.md: Regeln für KI-Coding-Werkzeuge in diesem Repository

Gilt für jedes KI-Werkzeug (Claude Code, GitHub Copilot, ChatGPT, andere) und für jede Person, die es bedient. Verbindlich ab Freigabe in AP06. Pflichten aus den Kursunterlagen S. 18 f. und den Einführungsfolien 10 und 11. Die Verantwortung für jede Zeile bleibt bei der Gruppe: KI-Code ist ein Vorschlag, bis ein Mensch ihn gelesen, verstanden und geprüft hat. Unverstandener Code wird vereinfacht oder ersetzt, nicht behalten.

Das Werkzeug setzt freigegebene, vollständig beschriebene Karten um. Es erweitert oder verändert den fachlichen Auftrag nicht selbstständig.

## 1. Das Projekt in fünf Sätzen

Wir bauen einen lokal betriebenen KI-Chat für die HFU Uster: Chat-Seite im Browser, eigenes Backend, lokaler Modellserver, alles auf einem Referenzgerät ohne Internet (Konzept Kapitel 3). Drei logisch getrennte Komponenten sind Pflicht (M03): Chat-Seite als Blazor-Komponente, Backend mit ChatService, Adapter und Store, Modellserver Docker Model Runner im zweiten Container (Konzept 7.1). Der Haupt-Use-Case F01 Nachricht senden mit Streaming und Abbruch ist der Kern; F02 bis F08 hängen daran (Konzept 4.7, 5.1). Das Design steht in Kapitel 7: Schichtenmodell, Design-Sequenz, Schnittstelle IChatService, Konfigurationswerte. Für das Werkzeug liegt beides als Markdown im Repository: das Was in `agent/Use_Cases.md` (Funktionsliste, nichtfunktionale Anforderungen, Haupt-Use-Case F01 mit Standardablauf, Erweiterungen, SSD und Zuständen, Ziele mit Nachweis), das Wie in `agent/Design.md` (Architektur, Komponenten, Klassen, Datenfluss, Schnittstellen, Konfiguration, Oberfläche, Daten) und `agent/Schnittstellen.md`; die Testfälle in `agent/Testfaelle.md`. Was dort nicht steht, steht im Konzept; was in beiden nicht steht, wird gefragt.

## 2. Umgebung

- Referenzsystem: Pascals Notebook (Konzept 3.5), Windows 11, Docker Desktop mit aktiviertem Model Runner. Alles andere gilt als ungetestet.
- Stack: .NET [Version aus AP06, LTS], ASP.NET Core mit Blazor Interactive Server (C#, Razor, HTML, CSS), Entity Framework Core mit SQLite, xUnit. Massgeblich sind `global.json` und die `.csproj`-Dateien, nicht das Gedächtnis des Werkzeugs. Aktuelle APIs werden nachgeschlagen, nicht erinnert.
- Anwendungen: Docker Desktop mit Model Runner, .NET SDK, Git, Visual Studio 2022 oder Visual Studio Code mit C# Dev Kit [AP06: eine Umgebung für alle drei festlegen], Browser Edge oder Chrome für die Chat-Seite. Das KI-Werkzeug läuft im Repository-Ordner mit denselben Rechten wie die Person, die es bedient.
- Erlaubte Befehle: `dotnet build`, `dotnet test`, `dotnet format`, `dotnet run`, `docker compose up` und `down`, `git status`, `git diff`, `git add <Pfad>`, `git commit`. Andere Werkzeuge nur nach Eintrag hier.
- Sprache: Deutsch mit ss statt Eszett. Fachbegriffe aus dem Glossar des Konzepts (Unterhaltung, Nachricht, Antwort, Störfall), technische Begriffe englisch (Service, Client, Store). Einzelheiten im `agent/StylingGuide.md`.

## 3. Ablage und Namen

- Repository-Struktur und Schichten: `README.md` und Konzept 7.1. Abhängigkeiten zeigen nur nach unten; `Chat.Web` kennt weder Adapter noch Store.
- Dateien im Code nach .NET-Konvention: Dateiname gleich Klassenname, Namensraum gleich Ordner.
- Dokumente ausserhalb des Codes nach Kursunterlagen S. 21: `YYYYMMDD-999-AA-Titel.xxxx`, ohne Umlaute, Sonderzeichen und Leerzeichen. Die Regel- und Protokolldateien in `agent/` tragen feste Namen, damit Verweise stabil bleiben.
- Konfiguration: `appsettings.json` plus Umgebungsvariablen. `appsettings.example.json` ist die einzige Datei mit Beispielwerten im Repository. Echte Werte, Hostnamen anderer Geräte, persönliche Pfade: nie.
- Karten liegen in `agent/use_cases/`, eine Datei je Karte nach `agent/Use_Case_Karte_Vorlage.md`, Name `AP07-backend-senden.md`. Eine Karte ist ein Arbeitspaket aus dem Projektplan oder ein Teilschritt davon, höchstens ein Halbtag; fachlich entspricht sie einem freigegebenen Use Case oder einem Teil davon.

## 4. Erlaubt

- Code, Tests, Kommentare und Dokumentation innerhalb des Umfangs der Karte erstellen und ändern.
- Bestehenden Code lesen, erklären, vereinfachen, wenn die Karte es verlangt.
- `dotnet build`, `dotnet test`, `dotnet format` ausführen, so oft wie nötig.
- Fragen stellen, bevor etwas gebaut wird, das nicht auf der Karte steht.
- Befunde ausserhalb der Karte notieren und im Bericht nennen.

## 5. Nicht erlaubt

- Ändern ausserhalb des Umfangs der Karte: andere Komponenten, andere Schichten, `docker-compose.yml`, `appsettings*.json`, `.csproj`, `global.json`, ausser die Karte nennt die Datei.
- Fachanforderungen, Akzeptanzkriterien oder Schnittstellen aus Konzept 7.3 eigenständig ändern; grosse Refactorings; Austausch zentraler Frameworks oder Architekturkomponenten.
- Neue Abhängigkeiten (NuGet-Pakete, Container-Images, Modelle) ohne Eintrag in `agent/Drittkomponenten.md` mit Zweck, Version, Lizenz und Freigabe durch ein Gruppenmitglied.
- Geheimnisse, Zugangsdaten, persönliche Pfade, Namen, Chatinhalte in Code, Konfiguration, Tests, Protokollen oder Commits.
- Tests löschen, abschwächen, überspringen oder so ändern, dass sie grün werden, ohne dass die Karte den Test nennt. Sicherheitsprüfungen deaktivieren.
- `git push`, Branch löschen, `git reset --hard`, Force-Push, `git add -A`, Änderungen an der Historie. Commits nur, wenn die Karte oder die Person es sagt, und nur eigene Pfade stagen.
- Aufrufe externer Dienste aus der Anwendung (Z01 offline). Der Modellserver wird nur über den Adapter angesprochen (Konzept 7.1); die Seite ruft nie HTTP.
- Dateien in `data/`, Datenbanken oder Protokolle anfassen; Protokolleinträge mit Chattext schreiben (Konzept 9.3).
- Zugangsdaten, personenbezogene Informationen, vertrauliche Inhalte oder nicht freigegebenen fremden Code an externe KI-Dienste senden (S. 19).

## 6. Bei Unsicherheit

- Ist die Karte nicht READY (`agent/DoR_DoD.md`), wird nichts gebaut: die fehlende Angabe benennen und auf Antwort warten. Eine KI erzeugt auf unklare Aufträge keine umfangreichen Änderungen (S. 18).
- Zwei Lösungswege möglich: den einfacheren vorschlagen, nicht beide bauen. Verständlichkeit geht vor Raffinesse.
- Etwas gefunden, das nicht zur Karte gehört: notieren, melden, nicht miterledigen.
- Build oder Test rot: erst reproduzieren, dann eine Hypothese nennen, dann ein Fix. Keine Workarounds, keine unterdrückten Fehler, keine drei Änderungen auf einmal.
- Widerspruch zwischen Karte und Konzept: anhalten und melden; das Konzept ändert nur die Gruppe (Kapitel 14.3).

## 7. Ablauf je Karte, mit Kontrollpunkten

Wer wann was tut, zeigt `agent/Ablauf.md`. Für das Werkzeug gilt:

1. Karte lesen, Definition of Ready prüfen. Nicht READY: Stopp, Bericht.
2. Vorgehen in drei bis fünf Sätzen nennen: welche Dateien, welcher Test, was unberührt bleibt. Erst nach Zustimmung bauen, wenn die Karte mehr als eine Datei betrifft.
3. Kleinster Schritt zuerst. Nach jedem Schritt `dotnet build` und `dotnet test`. Rot heisst Stopp und Bericht mit Ausgabe.
4. Ergebnis als Diff zeigen und den Bericht nach Abschnitt 9 liefern.
5. Selbstprüfung gegen `agent/Review_Checkliste.md`, schriftlich, Punkt für Punkt.
6. Eintrag in `agent/protokolle/KI-Einsatz.md`: Karte, Werkzeug, wofür, wie geprüft. Der Mensch liest den Diff, testet manuell, entscheidet über Review und Commit. Das Werkzeug gibt nie selbst frei.

## 8. Abbruchkriterien

In jedem dieser Fälle anhalten und den Grund berichten:

- die Karte ist unklar, unvollständig oder widersprüchlich, oder sie ändert sich während der Arbeit
- ein Test müsste geändert werden, den die Karte nicht nennt
- eine neue Abhängigkeit wäre nötig
- eine Schnittstelle aus Konzept 7.3 oder eine Architekturkomponente müsste sich ändern
- die Änderung würde eine andere Schicht oder bestehende Funktionen ausserhalb der Karte berühren
- sicherheitsrelevante Auswirkungen lassen sich nicht zuverlässig beurteilen (S. 10)
- ein Befehl ausserhalb Abschnitt 2 wäre nötig

## 9. Bericht an den Menschen (nach jeder Karte, in dieser Form)

Der Bericht wird in die Kartendatei `agent/use_cases/APxx-titel.md` unter "Ergebnis" geschrieben und zusätzlich im Chat gezeigt. So liegt zu jeder Karte an einem Ort: Auftrag, Definition of Ready, Ergebnis, Prüfungen, Review, Definition of Done.

```text
Karte: APxx Titel
Umgesetzt:
- ...
Geänderte Dateien:
- ...
Nicht angefasst (bewusst):
- ...
Prüfungen:
- dotnet build: OK oder Fehler (Ausgabe)
- dotnet test: n bestanden, m fehlgeschlagen (Namen)
- dotnet format: keine Änderung oder Liste
Selbstprüfung (agent/Review_Checkliste.md): Punkt für Punkt ja oder nein mit Grund
Offen, Risiken, Befunde ausserhalb der Karte:
- ...
Vorschlag Commit-Nachricht: APxx: ... / KI: Werkzeug, geprüft von XX
```

## 10. Tests, Review, Dokumentation

- Jede Karte nennt ihre Tests. `dotnet test` muss grün sein, bevor eine Karte auf Review geht. Manuelle Tests kommen ins `agent/protokolle/Test_und_Reviewprotokoll.md` mit erwartetem und tatsächlichem Ergebnis.
- Review nie ausschliesslich durch dasselbe KI-Werkzeug, das den Code erzeugt hat. Die Freigabe macht ein Gruppenmitglied. Pflicht-Review durch eine andere Person bei Sicherheit, Architektur (Schichten, Schnittstellen 7.3, Konfiguration 7.4) und schwer verständlichem Code; sonst Stichprobe.
- Dokumentation: weicht die Umsetzung vom Konzept ab, wird das Kapitel angepasst und die Abweichung in 14.3 genannt. Ändert sich eine Schnittstelle, wird `agent/Schnittstellen.md` angepasst. Ändert sich Start oder Installation, wird `README.md` angepasst.
- Commit-Nachricht: Kartennummer und Aussage im Präsens, `AP07: Chat-Endpunkt streamt Teile`. Bei KI-Beteiligung eine zweite Zeile `KI: Claude Code, geprueft von BH`. Keine Emojis, keine Nummer ohne Text.

## 11. Verweise

`agent/StylingGuide.md`, `agent/Use_Cases.md` (Was), `agent/Design.md` (Wie), `agent/Schnittstellen.md`, `agent/Testfaelle.md`, `agent/DoR_DoD.md`, `agent/Use_Case_Karte_Vorlage.md`, `agent/Review_Checkliste.md`, `agent/protokolle/Test_und_Reviewprotokoll.md`, `agent/protokolle/KI-Einsatz.md`, `agent/Drittkomponenten.md`, `agent/Ablauf.md`. Konzept: Kapitel 4.7 (Use Case), 5 (Ziele mit Nachweis), 7 (Design), 9 (Daten), 11 (Tests), 15 (KI-Einsatz). Kursunterlagen S. 10 (Sicherheit), S. 12 (Bedienabläufe), S. 18 f. (diese Regeln), S. 21 (Namenskonvention).
