# agent/: alles für eine Karte an einem Ort

Einstieg ist `../AGENTS.md` (Regeln für das Werkzeug). Dieser Ordner enthält, was das Werkzeug und die Menschen darüber hinaus brauchen. Diagramme liegen hier nur als Mermaid: Text, den ein Werkzeug lesen, ändern und im Diff vergleichen kann. Die Bilder für das Konzept liegen im Projektordner `bilder\`.

## Leseordnung für eine neue Karte

| Nr. | Datei | Wozu | Ändert sich |
|---|---|---|---|
| 0 | `Uebersicht.md` | wie das Setup zusammenhängt: Dokumentfluss, Phasen mit ihren Dateien, wer was ändern darf | selten (Regel) |
| 1 | `Ablauf.md` | wer macht was wann, zehn Schritte, vier Rollen | selten (Regel) |
| 2 | `DoR_DoD.md` | wann begonnen werden darf, wann fertig ist | selten (Regel) |
| 3 | `Use_Case_Karte_Vorlage.md` | die Karte selbst, acht Felder plus Ergebnis | selten (Vorlage) |
| 4 | `Use_Cases.md` | das Was: Funktionen, Anforderungen, Haupt-Use-Case F01, Ziele mit Nachweis | bei Konzeptänderung, neu exportiert |
| 5 | `Design.md` | das Wie: Schichten, Komponenten, Klassen, Datenfluss, Konfiguration, Oberfläche, Daten | bei Konzeptänderung, neu exportiert |
| 6 | `Schnittstellen.md` | IChatService, IModelServerClient, IStore, Konfiguration, Zustände | bei Designänderung, zusammen mit dem Konzept |
| 7 | `Testfaelle.md` | nummerierte Testfälle, auf die Karten zeigen | wächst mit AP11 und neuen Karten |
| 8 | `StylingGuide.md` | Codekonventionen C#, Razor, HTML, CSS, Tests, Git, Muster-Code | selten (Regel) |
| 9 | `Review_Checkliste.md` | Selbstprüfung des Werkzeugs, Review durch Menschen | selten (Regel) |
| 10 | `Drittkomponenten.md` | Bibliotheken, Images, Modell mit Version und Lizenz | bei jeder neuen Abhängigkeit |
| | `use_cases/` | eine Datei je Karte: Auftrag, READY, Ergebnis des Werkzeugs, DONE | lebend, je Karte |
| | `protokolle/` | `KI-Einsatz.md`, `Test_und_Reviewprotokoll.md` | lebend, je Karte |

## Drei Regeln für diesen Ordner

- Das Konzept (Word) ist der Master für `Use_Cases.md` und `Design.md`. Nicht hier editieren, sondern das Konzept ändern und neu exportieren; sonst laufen zwei Wahrheiten auseinander (Konzept 14.3).
- Regeln (`Ablauf`, `DoR_DoD`, `StylingGuide`, `Review_Checkliste`) ändert nur die Gruppe, mit Zeile im Reviewprotokoll.
- Lebende Dateien (`use_cases/`, `protokolle/`) werden nur ergänzt, nie geleert.
