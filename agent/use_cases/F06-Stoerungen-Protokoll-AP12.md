# Karte AP12: Stoerungen erkennen, melden und Eingabehinweise anzeigen

**Status:** READY; fachlich freigegeben durch PS am 24.09.2026. Die Karte ist fuer die Umsetzung freigegeben.

| Feld           | Inhalt                                                                                                                                                                                                                                 |
| -------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Verantwortlich | PS                                                                                                                                                                                                                                     |
| Halbtag        | Do PM                                                                                                                                                                                                                                  |
| Referenz       | F01, F06, F08; Z06, Z07; M09, M12                                                                                                                                                                                                      |
| Quellen        | `agent/Use_Cases.md` F01, F06 und F08, Ziele Z06 und Z07; `agent/Design.md` Datenfluss, UI-Konzept und Konfigurationskonzept; `agent/Schnittstellen.md` Fehlervertrag und Eingabegrenze; `agent/Testfaelle.md` T07, T09, T10, T11, N05 |
| Testfaelle     | T07, T09, T10, T11 aus `agent/Testfaelle.md`; neue UI-Nachweise fuer Hinweis, Zaehler und Statusanzeige werden bei AP11 als T13 bis T15 erfasst                                                                                        |

## 1. Ziel (ein Satz)

Die Chat-Seite zeigt bei ungueltiger Eingabe und technischen Stoerungen eine verstaendliche, barrierearme Meldung, bleibt danach bedienbar und zeigt die verbleibende Eingabegrenze von 4000 Zeichen an, ohne ein technisches Chatprotokoll zu implementieren.

## Bereits durch AP07 und F01 erledigt

- Der ChatService weist leeren Text vor dem Modellaufruf ab und meldet: `Die Nachricht darf nicht leer sein.`
- Der ChatService weist Texte ueber 4000 Zeichen ab und nennt die Grenze in der Meldung.
- Der ChatService behandelt die vier vorgesehenen Stoerfaelle: Modellserver nicht erreichbar, Modell unbekannt, ungueltige Konfiguration und Zeitueberschreitung.
- Die Chat-Seite zeigt `StoerfallException` mit Grund an, setzt den Status auf `Stoerung` und gibt die Eingabe nach dem Fehler wieder frei.
- Backend-Nachweise bestehen durch T07, T09, T10 und T11.

## 2. Abgrenzung (gehoert nicht dazu)

- Keine technische Protokolldatei und keine Speicherung von Zeitpunkt, Art, Dauer oder Fehler einer Anfrage. F08 und Z07 werden in dieser Karte nur als offener, informativ dokumentierter Zielbestand beschrieben.
- Keine neue `IChatService`-, `IStore`- oder `IModelServerClient`-Methode.
- Keine Aenderung an Modellserver, Konfiguration, Datenbankschema, Docker Compose oder den vier fachlichen Stoerfalltypen.
- Keine Betreiberansicht und keine Rollen- oder Benutzerverwaltung.
- Keine fachliche Aenderung der Eingabegrenze: Sie bleibt bei 4000 Zeichen.

## 3. Betroffene Komponenten und Dateien

- `src/LocalAiFront/Components/Pages/Home.razor`: Hinweis am deaktivierten Senden-Knopf, Zeichenzaehler, `maxlength="4000"`, Statuspunktdarstellung und Statusmeldung bei Stoerungen.
- `src/LocalAiFront/Components/Pages/Home.razor.css`: Layout und Darstellung des Zeichenzaehlers anpassung der  Statuspunktdarstellung.
- `tests/Chat.Tests/ChatSeiteTests.cs`: Komponententests fuer Hinweis, Zaehler, Eingabegrenze und Statusanzeige.
- `agent/Testfaelle.md`: nach AP11-Vorgabe neue nummerierte UI-Testfaelle T13 bis T15 ergaenzen.
- `agent/protokolle/KI-Einsatz.md`: Umsetzung der Karte dokumentieren.
- Unberuehrt: `src/Chat.Core`, `src/Chat.Adapter.ModelRunner`, `src/Chat.Store.Sqlite`, `docker_compose.yml`, `appsettings*.json`, `global.json` und technische Daten-/Protokolldateien.

## 4. Akzeptanzkriterien (pruefbar, vorher festgelegt; beim Review abhaken)

- [ ] Bei leerem Eingabefeld bleibt Senden deaktiviert; beim Hover und Fokus ueber dem deaktivierten Senden-Bereich ist ein Hinweis wie `Bitte erst Nachricht eingeben.` erreichbar. Der Hinweis ist auch fuer assistive Technologien verstaendlich zugeordnet.
- [ ] Das Eingabefeld hat `maxlength="4000"`; 4000 Zeichen ist die einzige gueltige Eingabegrenze der Oberflaeche.
- [ ] Oberhalb des Eingabefelds wird der Zaehler im Format `aktuell/4000` angezeigt und aktualisiert sich bei jeder Eingabe, zum Beispiel `0/4000` und danach `1/4000`.
- [ ] Die Anzeige des gruenen Punktes fuer `Bereit` wird so angepasst, dass dieser auch auf rot und Fehler geht wenn generell ein Fehler oder Verbindungsunterbruch oder Verbindungsaufbau vorliegt.. Der Textstatus bleibt nur erhalten, soweit er fuer Status- oder Stoerungsmeldungen benoetigt wird.
- [ ] Bei Modellserver-Nichterreichbarkeit, unbekanntem Modell, ungueltiger Konfiguration und Zeitueberschreitung erscheint jeweils der vom Backend gelieferte verstaendliche Grund; danach sind Eingabefeld und Senden wieder bedienbar.
- [ ] Eine leere Eingabe und eine Eingabe ueber 4000 Zeichen loesen keinen Modellaufruf aus; der Benutzer erhaelt einen passenden Hinweis.
- [ ] Es wird keine technische Protokolldatei angelegt oder erweitert. Die Karte dokumentiert ausdruecklich, dass F08/Z07 aktuell informativ bleiben und spaeter separat beauftragt werden muessten.
- [ ] Bestehendes Streaming, Abbrechen, Unterhaltungsverwaltung und die vier Backend-Stoerfaelle bleiben unveraendert.

## 5. Schnittstellen und Konfiguration

Die bestehende Schnittstelle `IChatService` wird unveraendert verwendet. Die UI nutzt weiterhin `SendeNachrichtAsync`, `AbbrechenAsync` sowie die vorhandenen Unterhaltungsoperationen. Die Eingabegrenze wird aus dem bestehenden Vertrag mit 4000 Zeichen abgeleitet; es wird keine neue Konfigurationsoption eingefuehrt. `IStore.ProtokollAsync` bleibt ausdruecklich ausserhalb dieser Karte und ist aktuell noch nicht implementiert.

## 6. Tests

- Automatisiert bestehend: T07, T09, T10 und T11 aus `agent/Testfaelle.md` sowie die vorhandenen Streaming-, Abbruch- und Bedienbarkeitstests.
- Automatisiert neu, in AP11 als T13 bis T15 zu nummerieren:
  - leeres Eingabefeld zeigt den Hinweis am deaktivierten Senden-Bereich und ruft den Service nicht auf;
  - Zeichenzaehler startet mit `0/4000`, zaehlt bei Eingabe korrekt hoch und begrenzt die Eingabe bei 4000 Zeichen;
  - Stoerfallmeldung gibt die Eingabe frei und zeigt den roten Statuspunkt mit dem Status `Stoerung`.
- Manuell: im Browser leeres Feld per Maus und Tastatur fokussieren, Zaehler bis zur Grenze pruefen, Modellserver stoppen und nach einer Stoerungsmeldung eine weitere Eingabe senden. Erwartetes und tatsaechliches Ergebnis in `agent/protokolle/Test_und_Reviewprotokoll.md` eintragen.
- N05 wird nicht als umzusetzender Logtest ausgefuehrt, weil F08/Z07 gemaess Auftrag nur informativ bleiben.

## 7. Groesse

Ein Halbtag. Teilschritte: 1. Hinweis, Zeichenzaehler und Statusdarstellung in der Razor-Seite; 2. CSS und Komponententests; 3. `dotnet build`, `dotnet test`, `dotnet format` und manuelle Browserpruefung.

## 8. Board

Projektplan: AP12, Do PM, Verantwortlich PS, Status READY/freigegeben; Umsetzung nach dem bestehenden Kartenablauf.

---

## Ergebnis (das Werkzeug schreibt seinen Bericht nach AGENTS.md Abschnitt 9 hierher; der Mensch prueft und hakt ab)

Karte: AP12 Stoerungen erkennen, melden und Eingabehinweise anzeigen

Umgesetzt:

- [x] Deaktivierter Senden-Bereich zeigt bei leerer oder zu langer Eingabe einen per `aria-describedby` zugeordneten Hinweis und ist per Tastatur fokussierbar.
- [x] Eingabefeld verwendet ausschliesslich die Grenze 4000; Zaehler zeigt `aktuell/4000` und wird bei jeder Eingabe aktualisiert.
- [x] Statuspunkt ist nur bei `Bereit` gruen und waehrend Anfrage, Abbruchaufbau oder Stoerung rot; der Textstatus bleibt erhalten.
- [x] Alle vier bestehenden `StoerfallException`-Gruende werden angezeigt; Eingabefeld und Senden sind danach wieder bedienbar.
- [x] T13 bis T15 sind in `agent/Testfaelle.md` dokumentiert und als Komponententests umgesetzt.

Geaenderte Dateien:

- [x] `src/LocalAiFront/Components/Pages/Home.razor`
- [x] `src/LocalAiFront/Components/Pages/Home.razor.css`
- [x] `tests/Chat.Tests/ChatSeiteTests.cs`
- [x] `agent/Testfaelle.md`
- [x] `agent/use_cases/F06-Stoerungen-Protokoll-AP12.md`
- [x] `agent/protokolle/KI-Einsatz.md`

Nicht angefasst (bewusst):

- [x] Technische Protokollierung, Modellserver, Store-Vertrag und Konfiguration ausserhalb der bestehenden Eingabegrenze.
- [x] `src/Chat.Core`, `src/Chat.Adapter.ModelRunner`, `src/Chat.Store.Sqlite`, `docker_compose.yml`, `appsettings*.json` und `global.json`.

Pruefungen:

- `dotnet build`: OK, mit `--no-restore --disable-build-servers -m:1`, 0 Warnungen und 0 Fehler. Der Standardaufruf brach zuvor beim parallelen Restore beziehungsweise Auswerten der Projektreferenzen ohne gemeldeten Codefehler ab.
- `dotnet test`: 71 bestanden, 0 fehlgeschlagen, 1 vorhandener expliziter Modellserver-Integrationstest nicht ausgefuehrt.
- `dotnet format`: geaenderte C#-Testdatei formatiert und gezielt ohne weitere Aenderung verifiziert. Die globale Pruefung bleibt wegen vorbestehender CRLF-Zeilenenden in nicht betroffenen Dateien rot; diese wurden gemaess Kartenabgrenzung nicht geaendert.
- Manuelle Browserpruefung: offen; in der Computersteuerung war kein Browser verfuegbar und der direkte Anwendungsstart benoetigt eine nicht vorhandene lokale Datenbankkonfiguration. Erwartete Pruefung durch PS: Hinweis per Maus und Tastatur, Zaehler bis 4000, beide Fensterbreiten und erneutes Senden nach gestopptem Modellserver.

Selbstpruefung (`agent/Review_Checkliste.md`):

- [x] Fachlichkeit: entspricht Karte, F06 und M09; Akzeptanzkriterien automatisiert abgedeckt, keine fachliche Erweiterung.
- [x] Namen und Fachbegriffe: StylingGuide eingehalten; deutsche UI-Texte und Bezeichner ohne Umlaute verwendet.
- [x] Schichtgrenzen: Seite verwendet weiterhin ausschliesslich `IChatService`; keine Backend-, Adapter- oder Store-Aenderung.
- [x] Verstaendlichkeit: Eingabegrenze und Hinweisermittlung sind lokal und ohne verschachtelte ternaere Ausdruecke umgesetzt.
- [x] Format und Kommentare: geaenderte C#-Datei gezielt formatkonform; keine Duplikate, keine unnoetigen Kommentare oder auskommentierter Code.
- [x] Vier Stoerfaelle: alle vier Gruende werden im Komponententest geprueft und die Eingabe danach freigegeben.
- [x] Abbruch: bestehendes Verhalten und Teilantwort bleiben unveraendert; Regressionstest gruen.
- [x] Eingabegrenzen: leer und ueber 4000 werden vor dem Service-Aufruf abgewiesen; `maxlength` und Zaehler verwenden 4000.
- [x] Zustandsuebergaenge: nicht geaendert; bestehende Tests gruen.
- [x] Umfang: Diff auf die in der Karte genannten UI-, Test- und Dokumentationsdateien begrenzt.
- [x] Schnittstellen: `IChatService`, `IStore` und `IModelServerClient` unveraendert.
- [x] Konfiguration und Installation: keine Aenderung; `appsettings.example.json` und README unberuehrt.
- [x] Geheimnisse und Datenschutz: keine Geheimnisse, persoenlichen Daten, Pfade oder Chatinhalte ergaenzt.
- [x] Netzwerk und Modellwerkzeuge: kein neuer Netzwerkzugriff und keine ausfuehrbaren Werkzeuge.
- [x] Abhaengigkeiten: keine neue Bibliothek oder Drittkomponente.
- [x] Systemanweisung und Eingabe: Trennung unveraendert; UI- und Service-Grenzpruefung bleiben aktiv.
- [x] Semantik und Barrierearmut: sichtbares `label`, `aria-describedby`, `role="tooltip"`, Tastaturfokus und bestehende Live-Regionen vorhanden.
- [x] Corporate Design: ausschliesslich vorhandene CSS-Variablen verwendet; Status hat Farbe und Text.
- [ ] Beide Fensterbreiten: CSS-Regeln geprueft, visuelle Browserpruefung durch PS noch offen.
- [x] Automatisierte Tests: 71 bestanden, 0 fehlgeschlagen; neue Logik durch T13 bis T15 abgedeckt.
- [ ] Manuelle Tests und Protokoll: durch PS nach dieser Uebergabe auszufuehren und in `Test_und_Reviewprotokoll.md` einzutragen.
- [x] Einschraenkungen: Browserpruefung, globale CRLF-Formatabweichung und F08/Z07 sind hier dokumentiert.
- [x] KI-Einsatz: Eintrag fuer AP12 in `agent/protokolle/KI-Einsatz.md` ergaenzt.
- [ ] Commit und Freigabe: stehen aus; Review muss durch ein Gruppenmitglied erfolgen.

Offen, Risiken, Befunde ausserhalb der Karte:

- F08/Z07 bleiben informativ; ein technisches Protokoll benoetigt eine spaetere eigene Freigabe.
- Der normale parallele Solution-Build brach in dieser Umgebung ohne Diagnose ab; der sequenzielle Solution-Build war vollstaendig gruen.
- Repositoryweite Formatpruefung meldet vorbestehende CRLF-Zeilenenden ausserhalb des Kartenumfangs.
- Review und Freigabe nach Umsetzung durch ein Gruppenmitglied stehen aus.

Vorschlag Commit-Nachricht: `AP12: Stoerungen und Eingabehinweise zeigen` / `KI: Codex, geprueft von PS`
