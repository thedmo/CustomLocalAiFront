# Definition of Ready und Definition of Done

Für jeden Entwicklungsschritt (Karte). Pflicht aus Kursunterlagen S. 18. Eine Karte ist ein Arbeitspaket aus dem Projektplan oder ein Teilschritt davon, höchstens ein Halbtag einer Person.

## Definition of Ready: bevor jemand oder eine KI baut

Eine Karte ist READY, wenn alle acht Punkte beantwortet in der Karte stehen (`Use_Case_Karte_Vorlage.md`):

1. **Ziel** in einem Satz, mit Referenz: Funktion (F01), Ziel (Z03, M06) oder Projektziel.
2. **Abgrenzung**: was ausdrücklich nicht dazugehört.
3. **Betroffene Komponenten**: Projekt und Dateien nach `Design.md` (Konzept 7.1); was unberührt bleibt.
4. **Akzeptanzkriterien**: prüfbar, aus `Use_Cases.md`: Nachweis-Spalte der Ziele oder Use-Case-Text (Standardablauf, Erweiterungen).
5. **Schnittstellen**: welche Methoden aus `Schnittstellen.md` (Konzept 7.3) genutzt oder geändert werden; Konfigurationswerte aus 7.4.
6. **Tests**: welche Nummern aus `Testfaelle.md` grün sein müssen (automatisiert in `Chat.Tests`, manuell im Protokoll); neue Fälle werden dort ergänzt.
7. **Grösse**: höchstens ein Halbtag; sonst in mehrere Karten teilen.
8. **Board**: Karte steht im Projektplan mit Verantwortlichem und Status In Arbeit.

Fehlt ein Punkt: Karte auf Blockiert, der Punkt wird im Daily oder im nächsten Block geklärt. Eine KI bekommt keine Karte, die nicht READY ist.

## Definition of Done: wann eine Karte fertig ist

1. **Läuft**: `dotnet build` grün, Anwendung startet mit `docker compose up` auf dem Referenzsystem.
2. **Styling**: `agent/StylingGuide.md` eingehalten, `dotnet format` ändert nichts mehr.
3. **Sauber**: keine Geheimnisse, keine persönlichen Pfade oder Daten, keine Chatinhalte im Repository.
4. **Getestet**: die in der Karte genannten Tests bestanden, `dotnet test` grün, neue Logik hat neue Tests.
5. **Manuell geprüft**: Eintrag im `Test_und_Reviewprotokoll.md` mit erwartetem und tatsächlichem Ergebnis.
6. **Review**: durch eine andere Person bei Sicherheit, Architektur (Schichten, Schnittstellen, Konfiguration) und schwer verständlichem Code, sonst Stichprobe. Freigabe durch ein Gruppenmitglied, nie nur durch ein KI-Werkzeug.
7. **Dokumentiert**: Konzeptkapitel angepasst, wenn die Umsetzung abweicht (Kapitel 14.3); `README.md`, wenn Start oder Installation sich ändern; `KI-Einsatz.md`, wenn KI beteiligt war; `Drittkomponenten.md`, wenn eine Abhängigkeit dazukam.
8. **Abgehakt**: Karte im Board auf Erledigt, Dauer effektiv eingetragen; Commit mit Kartennummer.

Erledigt gibt es nur nach Punkt 6. Wer eine Karte ohne Review auf Erledigt setzt, setzt sie zurück auf Review.
