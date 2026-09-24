# Test- und Reviewprotokoll

Pflichtfelder nach Kursunterlagen S. 19: Datum, verantwortliche Person, geprüfter Entwicklungsstand, Prüfumfang, erwartetes und tatsächliches Ergebnis, festgestellte Mängel, vorgenommene Korrekturen, Resultat der Nachprüfung. Eine Zeile je manuellem Test und je Review. Ein Review darf nicht ausschliesslich durch dasselbe KI-Werkzeug erfolgen, das den Code erzeugt hat; die Freigabe macht ein Gruppenmitglied.

Stand = Commit-Kürzel oder Branch und Uhrzeit. Art = Test (manuell) oder Review (Code). Nachprüfung = Ergebnis nach der Korrektur, mit Datum.

| Datum | Person | Stand (Commit) | Art | Prüfumfang (Karte, Dateien, Testfall) | Erwartet | Tatsächlich | Mängel | Korrekturen | Nachprüfung |
|---|---|---|---|---|---|---|---|---|---|
| 22.09.2026 | [RL] | [Gerüst, Commit] | Review | AP06: AGENTS.md, StylingGuide.md, DoR_DoD.md gelesen und beschlossen | Regeln vollständig nach S. 18 f., von allen verstanden | [ ] | [ ] | [ ] | [ ] |
| 24.09.2026 | Codex, Auftrag PS | Arbeitsverzeichnis AP09/F04, Abschlusslauf 24.09. | Technischer Integrationstest per CLI/HTTP | AP09b: Hauptcontainer nach gemeldetem Startfehler | Anwendung startet, localhost:80 erreichbar | Docker-Build erfolgreich, HTTP 200, Blazor-Startskript vorhanden | SpeicherortDb wurde durch Compose nicht übergeben | Explizites Chat__SpeicherortDb=/app/data/chat.db, vorhandener Mount erhalten | Erfolgreich; keine manuelle Browserabnahme daraus abgeleitet |
| 24.09.2026 | Codex, Auftrag PS | Arbeitsverzeichnis AP09/F04, Abschlusslauf 24.09. | Technischer Integrationstest per CLI | AP09b/F04, Z05: isolierter Container mit temporärer Testdatei | 5 synthetische Unterhaltungen nach Neustart erhalten; offene Antworten Gestoert/Neustart | Alle 5 über Store geladen und verglichen; leere Unterhaltung, Text und Endzustände korrekt; HTTP 200 ohne erreichbaren Modellserver | Keine im Prüfumfang | Nicht erforderlich; Testcontainer danach entfernt | Bestanden; Hauptdienst unverändert weiterlaufend |
| 24.09.2026 | Gruppenmitglied offen | AP09/F04 nach technischem Abschluss | Ausstehender manueller Test/Review | T05/N03, beide Fensterbreiten, Tastatur, echtes Streaming; Schema/DI/Synchronisation | Gespeicherte Verläufe korrekt bedienbar, keine Regression, verständlicher Code | Nicht durchgeführt; Browserwerkzeug meldet keine verbundenen Browser/Tabs | Visuelle Abnahme und unabhängiges Review fehlen | Durch Gruppenmitglied ausführen | Offen |

## F05: ausstehende manuelle Abnahme

| Datum | Person | Stand | Art | Prüfumfang | Erwartet | Tatsächlich | Mängel | Korrekturen | Nachprüfung |
|---|---|---|---|---|---|---|---|---|---|
| 24.09.2026 | PS / unabhängige Reviewperson offen | Arbeitsverzeichnis F05 | Manueller Test und Code-Review ausstehend | T06/Z05, beide Fensterbreiten, Tastatur/Bestätigungsdialog, fünf Verläufe und echter Prozessneustart, Protokollerhalt; Store-/Service-Koordination | Nur bestätigte Unterhaltung vollständig entfernt; Anzahl sinkt um eins, keine automatische Ersatz-Unterhaltung; vier Originalverläufe erhalten; keine Löschung während aktiver Antwort; technische Protokolle unverändert | PS meldete beim ersten Stand eine gleichbleibende Unterhaltung mit leerem Inhalt; Ursache war die automatische Neuanlage nach erfolgreichem Löschen. Korrigierter Stand noch nicht manuell geprüft; automatisierte Nachweise stehen in der Karte | Automatische Ersatz-Unterhaltung entfernt; expliziter Regressionstest ergänzt | Manuelle Nachprüfung offen |

## F03: ausstehende manuelle Nachprüfung

| Datum | Person | Stand | Art | Prüfumfang | Erwartet | Tatsächlich | Mängel | Korrekturen | Nachprüfung |
|---|---|---|---|---|---|---|---|---|---|
| 24.09.2026 | PS / unabhängige Reviewperson offen | Arbeitsverzeichnis F03/F05 | Manueller Test und Code-Review ausstehend | T01: Seitenstart, mehrfach Neue Unterhaltung, erste Nachricht, Neustart; Liste und SQLite | Vor erster Nachricht kein neuer Listen-/Datenbankeintrag; mit erster Nachricht genau eine neue Unterhaltung, sofort sichtbar und nach Neustart erhalten | Automatisiert mit Fake-Service und echter temporärer SQLite-Datei geprüft; Core-Test bestätigt, dass die reservierte Kennung vor der ersten Nachricht nicht im Store liegt; Browserprüfung nicht durchgeführt | Manuelle visuelle Nachprüfung offen | Seitenstart und Neue Unterhaltung auf lokalen Entwurf umgestellt; Unterhaltung und erste Nachricht werden gemeinsam gespeichert; Dokumentation nachgeführt | Offen |

## Automatisierte Tests

Die Läufe von `dotnet test` werden nicht hier protokolliert; der Nachweis ist die Ausgabe im Bericht der Karte und der grüne Lauf vor dem Review. Die Testfälle selbst stehen im Konzept 11.3 und in `Chat.Tests`.

## Abnahmetests (Freitag, Konzept 11.2)

Je Mussziel eine Zeile, Spalte Prüfumfang nennt die Nummer (M04 bis M13) und den Testfall.
