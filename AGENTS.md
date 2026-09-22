# AGENTS.md

## Zweck

Diese Datei definiert die verbindlichen Regeln für KI-Coding-Agenten in diesem Repository.
Der Agent setzt freigegebene, vollständig beschriebene Use Cases um. Er erweitert oder verändert den fachlichen Auftrag nicht selbstständig.

## Arbeitsgrundlage

Eine Umsetzung startet nur, wenn ein Use Case die Definition of Ready erfüllt und mindestens enthält:

- eindeutige Beschreibung / fachlicher Kontext
- Ziele und erwarteter Nutzen
- Abgrenzung / Nicht-Ziele
- betroffene Komponenten bzw. Schnittstellen
- Akzeptanzkriterien
- vorgesehene Tests

Fehlen wesentliche Angaben oder widersprechen sich Anforderungen, stoppt der Agent und fordert Klärung an.

## Projektmethode

1. **Use Case übernehmen** – Auftrag vollständig lesen, Umfang und Akzeptanzkriterien prüfen.
2. **Vorgehen planen** – betroffene Dateien/Komponenten und notwendige Änderungen kurz festhalten.
3. **Umsetzen** – nur Änderungen innerhalb des freigegebenen Use Cases durchführen.
4. **Maschinell prüfen** – Build, Linter und automatisierte Tests ausführen.
5. **Selbstkontrolle** – Änderung gegen Akzeptanzkriterien, Guidelines und Seiteneffekte prüfen.
6. **Übergabe an Mensch** – Ergebnis, geänderte Dateien, Tests und bekannte Einschränkungen knapp dokumentieren.
7. **Manuelle Abnahme** – Abschluss/Freigabe erfolgt durch ein Teammitglied, nicht durch den Agenten.

## Kontrollpunkte und Abbruchkriterien

Der Agent stoppt und meldet den Grund, wenn:

- Anforderungen unklar, unvollständig oder widersprüchlich sind
- eine Architektur- oder Schnittstellenänderung ausserhalb des Use Cases nötig wäre
- neue externe Abhängigkeiten erforderlich sind, die nicht freigegeben wurden
- sicherheitsrelevante Auswirkungen nicht zuverlässig beurteilt werden können
- Tests fehlschlagen und die Ursache nicht innerhalb des Auftrags behoben werden kann
- bestehende Funktionen ausserhalb des Auftrags erheblich verändert würden

## Erlaubte Aktionen

- vorhandenen Code analysieren und innerhalb des Use Cases ändern
- Tests ergänzen oder anpassen
- projektspezifische Dokumentation aktualisieren
- bestehende Werkzeuge für Build, Test, Formatierung und statische Analyse verwenden

## Nicht erlaubt ohne Freigabe

- eigenständige Änderung von Fachanforderungen oder Akzeptanzkriterien
- grosse Refactorings ausserhalb des Use Cases
- Austausch zentraler Frameworks oder Architekturkomponenten
- neue Bibliotheken/Services ohne Begründung und Freigabe
- Secrets, Zugangsdaten oder persönliche Pfade im Sourcecode hinterlegen
- bestehende Sicherheitsprüfungen oder Tests deaktivieren, um einen Build "grün" zu machen

## Generelle Richtlinien

- Projektsprache für Dokumentation: Deutsch
- Code, Bezeichner und technische API-Namen gemäss sprach-/technologiespezifischer Guideline.
- Kleine, nachvollziehbare Änderungen bevorzugen.
- Bestehende Struktur und Konventionen respektieren.
- Fehler explizit behandeln; keine stillen Fehler oder leeren Catch-Blöcke.
- Konfiguration gehört ausserhalb des Sourcecodes; Secrets niemals einchecken.
- KI-generierter Code muss verständlich, prüfbar und wartbar sein. Code-Versändlichkeit geht vor Implementierungseffizenz!

## Verbindliche Dokumente

- `docs/PROJECT_METHOD.md` – Projektmethode und Übergabe eines Use Cases
- `docs/GUIDELINES.md` – allgemeine Coding- und Strukturregeln
- `docs/DEFINITION_OF_READY.md` – Startkriterien
- `docs/DEFINITION_OF_DONE.md` – Abschlusskriterien
- `docs/REVIEW_CHECKLIST.md` – menschliche Review- und Abnahmekontrolle
