# Allgemeine Guidelines

Diese Regeln gelten für manuell und durch KI erzeugten Code gleichermassen. Sprach- oder frameworkspezifische Ergänzungen können als weitere Dateien unter `docs/guidelines/` ergänzt werden.

## Struktur und Benennung

- Bestehende Projektstruktur beibehalten.
- Namen müssen Zweck und Verantwortung ausdrücken.
- Dateien und Klassen klein und klar abgegrenzt halten.
- Keine Parallelstrukturen für bereits vorhandene Funktionen erzeugen.

## Formatierung und Kommentare

- Projektformatierung bzw. Formatter verwenden.
- Kommentare erklären die einzelnen Code-Abschnitte
- Öffentliche Schnittstellen bei Bedarf kurz dokumentieren in `docs/Interface_Documentation.md`

## Fehlerbehandlung

- Erwartbare Fehler gezielt behandeln und verständlich weitergeben.
- Technische Details nur dort loggen, wo sie für Diagnose erforderlich sind.
- Keine vollständigen sensiblen Nutzdaten in technischen Logs.

## Daten und Transaktionen

- Mehrschrittige persistente Änderungen konsistent/atomar ausführen, wenn die verwendete Technologie dies unterstützt.
- Teilzustände bei Fehlern vermeiden oder gezielt zurückrollen.
- Eingaben an Systemgrenzen validieren.

## Konfiguration und Abhängigkeiten

- Konfigurationswerte nicht hart codieren, wenn sie umgebungsabhängig sind.
- Keine Secrets einchecken.
- Neue Abhängigkeiten nur, wenn sie notwendig, passend lizenziert und freigegeben sind.
- Vorhandene Bibliotheken bevorzugen, wenn sie den Zweck bereits erfüllen.

## Tests

- Neue oder geänderte Fachlogik benötigt passende Tests.
- Neben Erfolgsfällen mindestens relevante Fehler- und Randfälle prüfen.
- Tests müssen reproduzierbar sein und dürfen nicht von zufälligen lokalen Zuständen abhängen.

## Muster für Agent-Antwort nach Umsetzung

```text
Umgesetzt:
- ...

Geänderte Dateien:
- ...

Prüfungen:
- Build: OK / Fehler
- Tests: OK / Fehler
- Lint/Analyse: OK / Fehler

Offen / Risiken:
- ...

Commit-Messsage: ...
```
