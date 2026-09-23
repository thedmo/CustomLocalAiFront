# F02: Laufende Antwort abbrechen – zurückgestellt

**Status:** Zurückgestellt am 23.09.2026, weil die Karte den bereits freigegebenen Umfang von AP07 doppelt beschrieben hat.

F02 bleibt eine fachliche Funktion in `agent/Use_Cases.md`, ist aber kein zusätzliches Arbeitspaket. Die technische Verantwortung ist eindeutig verteilt:

- AP07 implementiert `AbbrechenAsync`, beendet die Modellanfrage und bewahrt bereits gelieferte Teile mit Zustand `Abgebrochen` auf.
- F01 Chat-Seite stellt die Abbruchbedienung bereit, lässt die Teilantwort sichtbar und gibt die Eingabe danach wieder frei.
- T04 weist den Backend-Abbruch in AP07 automatisiert und den vollständigen Bedienablauf nach F01 Chat-Seite manuell nach.

Diese Datei darf nicht als READY-Karte umgesetzt oder zusätzlich ins Board aufgenommen werden. Änderungen an F02 werden in der jeweils zuständigen AP-Karte geplant.
