# Karte F01: Chat-Seite zeigt die Antwort schrittweise und bricht sie ab (F01, F02)

**Status:** Entwurf, noch nicht READY. Verantwortlichkeit, Halbtag und Board-Eintrag müssen vor der Umsetzung freigegeben werden.

| Feld | Inhalt |
|---|---|
| Verantwortlich | [festzulegen] |
| Halbtag | [festzulegen] |
| Referenz | F01 Nachricht senden, F02 Abbrechen; Z03 Antwort in Teilen; M05, M06 |
| Quellen | `agent/Use_Cases.md` Haupt-Use-Case Schritte 1, 2, 4 und 6 sowie Erweiterungen 2a, 2b, 3a, 3b, 4a und 4b; `agent/Design.md` Oberfläche und Datenfluss; `agent/Schnittstellen.md` `IChatService` |
| Testfälle | T02, T03, T04, T07, T09, T10, T11 aus `agent/Testfaelle.md` |

## 1. Ziel (ein Satz)

Die Chat-Seite sendet eine gültige Nachricht über `IChatService`, zeigt jeden gelieferten Antwortteil sofort und in derselben Antwort an, sperrt währenddessen weitere Eingaben und ermöglicht den Abbruch der laufenden Antwort.

## 2. Abgrenzung (gehört nicht dazu)

Kein direkter HTTP-Aufruf der Seite zum Model Runner. Keine Änderung an ChatService oder ModelRunnerClient (AP07), keine dauerhafte Speicherung (AP09) und kein technisches Dateiprotokoll (AP12).

## 3. Betroffene Komponenten und Dateien

- `web_app/LocalAiFront`: Chat-Seite, zugehörige Darstellung und Registrierung von `IChatService`
- `tests/Chat.Tests`: Komponententests für schrittweise Anzeige, Eingabesperre, Abschluss, Störfall und Abbruch
- Unberührt: `src/Chat.Adapter.ModelRunner`, `src/Chat.Store.Sqlite`, `docker_compose.yml`, `global.json`

Die konkreten Dateinamen und nötigen Änderungen an Projekt- oder Konfigurationsdateien sind vor READY anhand des bestehenden Web-Projekts festzulegen.

## 4. Akzeptanzkriterien

- [ ] Die eigene Nachricht erscheint nach dem Senden sofort im Verlauf.
- [ ] Mindestens zwei zeitlich getrennt gelieferte Teile werden einzeln und in Reihenfolge in derselben Antwort angezeigt; der erste Teil ist sichtbar, bevor der Stream abgeschlossen ist (T03).
- [ ] Während der Stream läuft, sind Nachrichteneingabe und erneutes Senden gesperrt; nach Abschluss oder Störfall sind sie wieder verfügbar.
- [ ] Die Abbruchbedienung beendet die laufende Antwort über ihre Antwort-ID. Bereits angezeigte Teile bleiben sichtbar und die Eingabe wird wieder frei (T04).
- [ ] Leere oder zu lange Eingaben und die dokumentierten Störfälle werden verständlich angezeigt, ohne die Seite unbedienbar zu machen.
- [ ] Die Seite spricht ausschliesslich `IChatService` an und kennt weder Adapter noch Store.

## 5. Schnittstellen und Konfiguration

Die Seite verwendet nur `IChatService.SendeNachrichtAsync` und `IChatService.AbbrechenAsync`. Sie liest `AntwortLauf.AntwortId` und konsumiert `AntwortLauf.Teile` fortlaufend. Änderungen an den Schnittstellen aus `agent/Schnittstellen.md` sind nicht Teil dieser Karte.

## 6. Tests

- Automatisiert: Komponententest mit steuerbarem Fake-Service für zwei getrennte Teile; Eingabesperre; Abschluss; Abbruch mit sichtbarer Teilantwort; Störfallanzeige.
- Manuell: T03 und T04 im Browser gegen den lokalen Model Runner; erwartetes und tatsächliches Ergebnis in `agent/protokolle/Test_und_Reviewprotokoll.md` eintragen.

## 7. Grösse

Vor READY schätzen. Wenn Oberfläche, Dependency Injection und Komponententests nicht in einen Halbtag passen, wird die Karte entlang eines nachweisbaren Bedienablaufs geteilt.

## 8. Board

[Nach Freigabe mit Verantwortlichem und Halbtag eintragen.]

---

## Ergebnis

Noch nicht umgesetzt.
