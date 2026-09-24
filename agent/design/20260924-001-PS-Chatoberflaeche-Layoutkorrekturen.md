# Umsetzungsauftrag Design: Chatoberfläche ohne Seiten-Scrollen

**Status:** Umgesetzt; manuelle Sichtprüfung und Review offen

**Art:** Design- und Bedienkorrektur, kein neuer Use Case

**Verantwortlich:** PS

**Datum:** 24.09.2026

| Feld | Inhalt |
|---|---|
| Ziel | Die Chatoberfläche nutzt den verfügbaren Viewport ohne Scrollen des gesamten Fensters und stellt Verlauf, Unterhaltungsliste, Hinweise sowie Auswahl- und Löschzustände eindeutig dar. |
| Referenzen | `agent/design/CorporateDesign.md`; `agent/bilder/20260923-001-KI-Chat-Mockup.png`; `agent/Design.md` Abschnitt UI-Konzept; `agent/StylingGuide.md` Abschnitt 8 |
| Nachweis | N03 aus `agent/Testfaelle.md` sowie die manuellen Prüfungen in Abschnitt 7 dieses Dokuments |
| Umfang | Präsentationsschicht in `src/LocalAiFront`, zugehörige Styles und UI-Tests |
| Unberührt | `Chat.Core`, `Chat.Adapter.ModelRunner`, `Chat.Store.Sqlite`, Datenbank, Schnittstellen, Konfiguration und Docker-Setup |

## 1. Ausgangslage

Die Seite als Ganzes ist aktuell scrollbar. Der Nachrichtenbereich scrollt bereits innerhalb der Oberfläche, die Liste der Unterhaltungen jedoch noch nicht. Dadurch können Kopf, Eingabe und andere zentrale Bedienelemente aus dem sichtbaren Bereich geschoben werden.

Hinweise und Fehlermeldungen erscheinen nicht an der gewünschten Stelle. Sie sollen als klar erkennbare Message-/Hinweisbox oben und horizontal zentriert angezeigt werden.

Beim Klicken auf die Löschbedienung einer Unterhaltung wandert die visuelle Hervorhebung von der ausgewählten Unterhaltung zur Löschbedienung. Der Tastaturfokus auf der Löschbedienung muss sichtbar bleiben, darf aber die dauerhafte Kennzeichnung der aktuell ausgewählten Unterhaltung nicht ersetzen.

## 2. Gestalterische Grundlage

Die visuelle und responsive Grundlage ist das Mockup:

![Mockup der Chatoberfläche mit Desktop- und Mobilansicht.](../bilder/20260923-001-KI-Chat-Mockup.png)

Die verbindlichen Farben, Typografie, Abstände, Zustände, Fokusdarstellung und Regeln zur Barrierefreiheit stehen in [`CorporateDesign.md`](CorporateDesign.md). Bei Abweichungen zwischen Mockup und Corporate Design hat das Corporate Design Vorrang. Das Mockup bestimmt Struktur und Bedienidee, nicht pixelgenaue Masse oder neutrale Platzhalterfarben.

## 3. Anforderungen

### 3.1 Viewport und Scrollbereiche

- Das Browserfenster beziehungsweise die Seite selbst darf bei den festgelegten Prüfbreiten nicht horizontal oder vertikal scrollen.
- Kopfbereich, Eingabebereich und zentrale Aktionen bleiben sichtbar.
- Nur inhaltlich begrenzte Bereiche dürfen bei Überlauf selbst scrollen:
  - der Nachrichtenverlauf;
  - das mehrzeilige Eingabefeld, sobald sein eigener Inhalt die sichtbare Höhe überschreitet;
  - die Liste der vorhandenen Unterhaltungen.
- Die Unterhaltungsliste erhält einen eigenen vertikalen Scrollbereich. Die Bedienung für eine neue Unterhaltung bleibt auch bei vielen Einträgen erreichbar.
- Flex- und Grid-Container müssen mit geeigneten Mindestgrössen aufgebaut sein, damit Überlauf in den vorgesehenen Bereichen statt auf `body` oder dem Seitencontainer entsteht.
- Es darf bei 320 CSS-Pixeln Breite kein horizontales Scrollen geben.

### 3.2 Message-/Hinweisbox

- Status-, Hinweis- und Fehlermeldungen erscheinen oben in der Mitte des sichtbaren Chatbereichs.
- Die Box besitzt eine begrenzte Breite, bleibt auf kleinen Bildschirmen innerhalb der seitlichen Abstände und überdeckt keine unzugänglichen Bedienelemente.
- Die Platzierung verursacht beim Ein- und Ausblenden keinen störenden Layoutsprung.
- Meldungsart und Bedeutung sind durch Text und, falls verwendet, Symbol erkennbar; Farbe allein reicht nicht.
- Statusmeldungen verwenden `aria-live="polite"`. Fehler verwenden weiterhin eine für Screenreader geeignete Alarmsemantik.
- Tastaturfokus wird durch das Erscheinen einer Meldung nicht ungefragt verschoben.

### 3.3 Auswahl und Löschen einer Unterhaltung

- Die aktuell geöffnete Unterhaltung bleibt vor, während und nach einem Klick auf ihre Löschbedienung eindeutig hervorgehoben, bis die Auswahl tatsächlich geändert oder die Unterhaltung erfolgreich gelöscht wurde.
- Die Löschbedienung erhält unabhängig davon einen sichtbaren `:focus-visible`-Rahmen. Fokus und Auswahl müssen gleichzeitig erkennbar sein.
- Ein Klick auf die Löschbedienung ändert die aktuelle Unterhaltung nicht und löst nicht zusätzlich die Auswahlaktion des Listeneintrags aus.
- Auswahlzustand, Tastaturfokus und Löschbestätigung werden mit getrennten CSS-Klassen beziehungsweise semantischen Attributen dargestellt.
- Wird die ausgewählte Unterhaltung erfolgreich gelöscht, zeigt die Oberfläche anschliessend die vom bestehenden Fachablauf bestimmte neue Auswahl. Wird das Löschen abgebrochen oder schlägt es fehl, bleibt die ursprüngliche Auswahl hervorgehoben.
- Die Lösung verwendet gültiges semantisches HTML; interaktive Elemente werden nicht ineinander verschachtelt.

### 3.4 Responsive Verhalten

- Desktop orientiert sich an der linken Unterhaltungsliste und dem zentralen Chatbereich des Mockups.
- Auf kleinen Bildschirmen ist die Unterhaltungsliste ein- und ausklappbar und standardmässig geschlossen. Der Chatbereich nutzt die verfügbare Breite.
- Eine geöffnete mobile Unterhaltungsliste besitzt einen eigenen Scrollbereich und verdeckt weder ihre Schliessbedienung noch dauerhaft die Eingabe.
- Touch-Ziele sind ungefähr 44 mal 44 CSS-Pixel oder grösser.
- Die Oberfläche wird mindestens bei 320, 375, 768, 1024 und 1440 CSS-Pixeln sowie bei 200 Prozent Zoom geprüft.

## 4. Betroffene Dateien

Vor Beginn prüft der umsetzende Agent den aktuellen Stand und passt die Liste an, ohne den fachlichen Umfang zu erweitern.

- `src/LocalAiFront/Components/Pages/Home.razor`
- `src/LocalAiFront/Components/Pages/Home.razor.css`
- vorhandene partielle Dateien `src/LocalAiFront/Components/Pages/Home.*.cs`, soweit Auswahl, Löschen oder die mobile Seitenleiste dort bereits umgesetzt sind
- vorhandene Chat-Seiten-Tests unter `tests/Chat.Tests`, soweit die geänderten Zustände automatisiert prüfbar sind
- `agent/protokolle/KI-Einsatz.md`
- diese Datei unter Abschnitt 9 Ergebnis

Neue NuGet-Pakete, JavaScript-Bibliotheken oder externe Assets sind nicht vorgesehen. Bestehende Logo- und Bilddateien werden lokal verwendet.

## 5. Abgrenzung

- Keine Änderung an fachlichen Löschregeln oder der Auswahl einer Ersatzunterhaltung nach erfolgreichem Löschen.
- Keine Änderung an `IChatService`, Store, Adapter, Datenbankmodell oder Migrationen.
- Keine neuen Funktionen für Bearbeiten, Suchen, Sortieren oder Gruppieren von Unterhaltungen.
- Keine extern geladenen Schriften, Icons, Bilder oder Dienste.
- Kein pixelgenauer Nachbau von ChatGPT, Claude oder Gemini; übernommen werden nur vertraute Bedienmuster innerhalb des HFU Corporate Designs.

## 6. Akzeptanzkriterien

- [ ] Das Browserfenster bleibt bei allen Prüfbreiten ohne eigene Scrollbar. Implementiert, Sichtprüfung offen.
- [ ] Nachrichtenverlauf, mehrzeiliges Eingabefeld und Unterhaltungsliste scrollen bei Überlauf jeweils innerhalb ihres Bereichs. Implementiert, Sichtprüfung offen.
- [ ] Bei vielen Unterhaltungen bleiben „Neue Unterhaltung“ und die übrigen zentralen Bedienelemente erreichbar. Implementiert, Sichtprüfung offen.
- [ ] Die Message-/Hinweisbox erscheint oben und horizontal zentriert, ohne Fokusverlust oder störenden Layoutsprung. Implementiert, Sichtprüfung offen.
- [x] Die ausgewählte Unterhaltung bleibt beim Fokussieren und Anklicken ihrer Löschbedienung semantisch ausgewählt; Abbruch, laufendes Löschen und Fehler sind automatisiert geprüft.
- [ ] Fokus, Auswahl, Löschbestätigung, Fehler und deaktivierte Zustände sind voneinander unterscheidbar und nicht nur über Farbe vermittelt. Implementiert, Sichtprüfung offen.
- [x] Löschen, Abbrechen des Löschens und ein Löschfehler behalten das bestehende fachliche Verhalten.
- [x] Die mobile Unterhaltungsliste lässt sich öffnen und schliessen; Zustand und Bedienelemente sind automatisiert geprüft. Manuelle Tastatur- und Touchprüfung offen.
- [ ] Farben, Typografie und Abstände entsprechen `agent/design/CorporateDesign.md`. Token umgesetzt, Sichtprüfung offen.
- [x] Die Lösung benötigt keine neue Abhängigkeit und ändert keine Schnittstelle.

## 7. Prüfungen

### Automatisiert

- Vorhandene Komponenten- und Service-Tests bleiben unverändert grün.
- Falls die Zustandslogik noch nicht abgedeckt ist: Komponententest ergänzen, der eine Unterhaltung auswählt, deren Löschbedienung betätigt und prüft, dass der ausgewählte Eintrag bis zum tatsächlichen Löschergebnis semantisch ausgewählt bleibt.
- Komponententest für Abbruch und Fehler beim Löschen: ursprüngliche Auswahl bleibt erhalten.
- Semantische Zustände der mobilen Seitenleiste und der Hinweisbox prüfen, soweit dies ohne Layoutsimulation möglich ist.

### Manuell

- N03 bei 320, 375, 768, 1024 und 1440 CSS-Pixeln durchführen.
- In jeder Breite prüfen: keine Scrollbar am gesamten Fenster; Nachrichtenverlauf und Unterhaltungsliste mit genügend Testeinträgen separat scrollbar.
- Lange Eingabe prüfen: nur das Eingabefeld scrollt innerhalb seiner Begrenzung.
- Hinweis, Fehler und Löschbestätigung auslösen: Box jeweils oben mittig, vollständig lesbar und ohne Layoutsprung.
- Ausgewählte Unterhaltung löschen anklicken, Bestätigung abbrechen und einen Löschfehler auslösen: Auswahl bleibt sichtbar; Fokusrahmen der Löschbedienung bleibt ebenfalls sichtbar.
- Tastaturprüfung mit Tab, Shift+Tab, Enter, Leertaste und Escape; anschliessend Prüfung bei 200 Prozent Zoom.
- Erwartetes und tatsächliches Ergebnis in `agent/protokolle/Test_und_Reviewprotokoll.md` eintragen.

## 8. Vorgehen für den umsetzenden Agenten

1. Diese Datei, `agent/design/CorporateDesign.md`, das Mockup, `agent/StylingGuide.md`, `agent/Design.md`, `agent/Testfaelle.md` und `agent/Review_Checkliste.md` vollständig lesen.
2. Aktuelle Dateien und vorhandene uncommittete Änderungen prüfen. Fremde Änderungen nicht überschreiben.
3. In drei bis fünf Sätzen nennen, welche Dateien geändert werden, welche Tests folgen und was unberührt bleibt; bei mehr als einer Datei Zustimmung abwarten.
4. Zuerst Layout und begrenzte Scrollbereiche, danach Hinweisbox, danach getrennte Auswahl-/Fokusdarstellung in kleinen Schritten umsetzen.
5. Nach jedem Schritt `dotnet build` und `dotnet test` ausführen. Bei einem roten Lauf stoppen und mit Ausgabe berichten.
6. `dotnet format`, Diff und Selbstprüfung nach `agent/Review_Checkliste.md` durchführen.
7. Ergebnis in Abschnitt 9 und KI-Einsatz in `agent/protokolle/KI-Einsatz.md` dokumentieren. Die Freigabe erfolgt durch ein Gruppenmitglied.

## 9. Ergebnis

Karte: Design Chatoberfläche ohne Seiten-Scrollen

Umgesetzt:

- Viewportgebundenes Flex-Layout verhindert vorgesehenes Scrollen von Seite und Browserfenster; Verlauf, Texteingabe und Unterhaltungsliste besitzen getrennte Überlaufbereiche.
- Aktionen für neue und zu löschende Unterhaltungen bleiben ausserhalb der scrollenden Liste erreichbar.
- Mobile Seitenleiste ist unter 800 Pixeln standardmässig geschlossen, lässt sich semantisch öffnen und schliessen und respektiert reduzierte Bewegung.
- Status und Fehler stehen in einer stabilen, oben zentrierten Hinweisbox mit getrennten `status`- und `alert`-Semantiken.
- Aktive Unterhaltung verwendet unabhängig vom Fokus `aria-current`, `aria-pressed` und eine eigene HFU-Auswahlklasse; Fokus bleibt über den grünen Corporate-Design-Rahmen separat sichtbar.
- HFU-Farb- und Typografievariablen aus `agent/design/CorporateDesign.md` sind zentral in `app.css` hinterlegt und für die geänderten Elemente verwendet.

Geänderte Dateien:

- `src/LocalAiFront/Components/Layout/MainLayout.razor.css`
- `src/LocalAiFront/Components/Pages/Home.razor`, `Home.razor.css`, `Home.Entwurf.cs`
- neu `src/LocalAiFront/Components/Pages/Home.Navigation.cs`, `Home.Unterhaltungen.cs`
- `src/LocalAiFront/wwwroot/app.css`
- `tests/Chat.Tests/ChatSeiteUnterhaltungenTests.cs`
- `agent/protokolle/KI-Einsatz.md` und diese Datei

Nicht angefasst (bewusst):

- Fachliche Löschregeln, `IChatService`, Store, Adapter, Datenbank, Migrationen, Konfiguration und Docker-Setup.
- Vorhandene Logos und Bilder; das Mockup diente als Strukturreferenz, ohne ein zusätzliches Asset zu kopieren.

Prüfungen:

- `dotnet build CustomLocalAiFront.slnx --no-restore --disable-build-servers -m:1`: OK, 0 Warnungen, 0 Fehler.
- `dotnet test --project tests/Chat.Tests/Chat.Tests.csproj --no-build`: 66 bestanden, 0 fehlgeschlagen, 1 expliziter Integrationstest übersprungen.
- `dotnet format` auf den betroffenen C#-Dateien: keine verbleibende Änderung.
- `git diff --check`: OK.
- Manuelle N03-Sichtprüfung: offen; der Computer-Use-Dienst stellte keinen Browser bereit.

Selbstprüfung (`agent/Review_Checkliste.md`):

- Fachlichkeit und Umfang: ja; ausschliesslich freigegebene Layout-, Darstellungs- und Zustandskorrekturen.
- Namen und Verständlichkeit: ja; deutsche Fachbegriffe, kleine Partial-Dateien und keine neue Abhängigkeit.
- Schichtgrenzen und Schnittstellen: ja; nur Präsentationsschicht und bestehende UI-Tests geändert.
- Fehlerbehandlung und Fachzustände: ja; bestehender Löschablauf unverändert, Regressionstests grün.
- Konfiguration, Sicherheit und Datenschutz: ja; keine Konfigurationsänderung, externen Aufrufe, Geheimnisse oder Chatinhalte im Protokoll.
- Semantisches HTML und Barrierefreiheit: ja im Code; echte Buttons, `aria-current`, `aria-expanded`, `status`, `alert`, Fokusdarstellung und reduzierte Bewegung. Manuelle Tastatur-, Touch- und Zoomprüfung offen.
- Corporate Design und Fensterbreiten: Token und Breakpoint umgesetzt; visuelle N03-Prüfung bei den fünf Breiten offen.
- Tests und Nachvollziehbarkeit: ja automatisiert; Build und 66 Tests grün, KI-Einsatz dokumentiert. Menschliches Review offen.

Offen, Risiken, Befunde ausserhalb des Auftrags:

- N03 bei 320, 375, 768, 1024 und 1440 CSS-Pixeln sowie bei 200 Prozent Zoom manuell prüfen und im Test- und Reviewprotokoll eintragen.
- Review und Freigabe durch ein Gruppenmitglied stehen aus; das Werkzeug gibt die Änderung nicht selbst frei.
- Der lokale Start für die Sichtprüfung war mit temporären Testwerten erfolgreich. Da kein Browser im Computer-Use-Dienst verfügbar war, wurde der Prozess wieder beendet und keine visuelle Aussage erfunden.

Vorschlag Commit-Nachricht: `Design: Chatoberfläche begrenzt Scrollbereiche` / `KI: Werkzeug, geprueft von XX`
