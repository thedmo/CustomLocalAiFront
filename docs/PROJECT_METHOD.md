# Projektmethode für agent-basierte Codegenerierung

## Grundprinzip
Der Agent erhält **keine offene Idee**, sondern einen **fertig ausgearbeiteten und freigegebenen Use Case**.
Der Use Case beschreibt fachliches Ziel, Umfang und prüfbare Akzeptanzkriterien. Die fachliche Verantwortung bleibt beim Projektteam.

## Ablauf

### 1. Idee und Ausarbeitung durch das Team
Das Team klärt Benutzererlebnis, Beschreibung, Ziele, Abgrenzung, Schnittstellen und Testidee.

### 2. Definition of Ready
Vor Übergabe wird geprüft, ob der Use Case umsetzungsbereit ist. Ohne erfüllte DoR keine Implementierung.

### 3. Umsetzung durch den Agenten
Der Agent:
- analysiert den bestehenden Projektstand
- erstellt einen kurzen Umsetzungsplan
- implementiert nur den freigegebenen Umfang
- ergänzt bzw. aktualisiert Tests und notwendige Dokumentation

### 4. Maschinelle Kontrolle
Mindestens ausführen, sofern im Projekt vorhanden:
- Build / Compile
- Linter / Formatter
- automatisierte Tests
- statische Analyse

### 5. Agent-Selbstkontrolle
Vor Übergabe prüft der Agent:
- Akzeptanzkriterien erfüllt?
- unbeabsichtigte Änderungen entstanden?
- Fehlerfälle berücksichtigt?
- neue Abhängigkeiten eingeführt?
- Konfiguration / Secrets korrekt behandelt?

### 6. Menschliche Kontrolle
Ein Teammitglied prüft Code, Verhalten und manuelle Testfälle. Ein Agent darf seine eigene Änderung nicht endgültig freigeben.

### 7. Definition of Done
Erst nach bestandenen Tests, Review und aktualisierter Dokumentation gilt der Use Case als abgeschlossen.

## Empfohlenes Übergabeformat für einen Use Case
```md
# UC-XXX: Titel

## Beschreibung
...

## Ziele
- ...

## Nicht-Ziele / Abgrenzung
- ...

## Betroffene Komponenten / Schnittstellen
- ...

## Akzeptanzkriterien
- [ ] ...
- [ ] ...

## Vorgesehene Tests
- ...

## Hinweise / Randbedingungen
- ...
```
