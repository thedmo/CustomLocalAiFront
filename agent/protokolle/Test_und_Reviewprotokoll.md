# Test- und Reviewprotokoll

Pflichtfelder nach Kursunterlagen S. 19: Datum, verantwortliche Person, geprüfter Entwicklungsstand, Prüfumfang, erwartetes und tatsächliches Ergebnis, festgestellte Mängel, vorgenommene Korrekturen, Resultat der Nachprüfung. Eine Zeile je manuellem Test und je Review. Ein Review darf nicht ausschliesslich durch dasselbe KI-Werkzeug erfolgen, das den Code erzeugt hat; die Freigabe macht ein Gruppenmitglied.

Stand = Commit-Kürzel oder Branch und Uhrzeit. Art = Test (manuell) oder Review (Code). Nachprüfung = Ergebnis nach der Korrektur, mit Datum.

| Datum | Person | Stand (Commit) | Art | Prüfumfang (Karte, Dateien, Testfall) | Erwartet | Tatsächlich | Mängel | Korrekturen | Nachprüfung |
|---|---|---|---|---|---|---|---|---|---|
| 22.09.2026 | [RL] | [Gerüst, Commit] | Review | AP06: AGENTS.md, StylingGuide.md, DoR_DoD.md gelesen und beschlossen | Regeln vollständig nach S. 18 f., von allen verstanden | [ ] | [ ] | [ ] | [ ] |
| | | | | | | | | | |

## Automatisierte Tests

Die Läufe von `dotnet test` werden nicht hier protokolliert; der Nachweis ist die Ausgabe im Bericht der Karte und der grüne Lauf vor dem Review. Die Testfälle selbst stehen im Konzept 11.3 und in `Chat.Tests`.

## Abnahmetests (Freitag, Konzept 11.2)

Je Mussziel eine Zeile, Spalte Prüfumfang nennt die Nummer (M04 bis M13) und den Testfall.
