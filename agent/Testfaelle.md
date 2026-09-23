# Testfälle

Quelle: Konzept Kapitel 11 (Testkonzept, AP11, Rod) und die Nachweis-Spalte der Ziele in `Use_Cases.md`. Jede Karte nennt in Feld 6 die Nummern der Testfälle, die grün sein müssen. Automatisierte Fälle liegen als Tests in `Chat.Tests` mit demselben Namen im Kommentar; manuelle Fälle werden im `Test_und_Reviewprotokoll.md` mit Datum, Person, Stand, Erwartet, Tatsächlich protokolliert. Das erwartete Ergebnis steht vor dem Lauf fest (M10).

Vorschlag als Gerüste; Rod führt die Tabelle in AP11 zu Ende und überträgt sie in Konzept 11.3.

## Funktionale Testfälle (mindestens zehn, S. 12 Bedienabläufe und 4.7 Erweiterungen)

| Nr. | Funktion, Ziel | Voraussetzung | Schritte | Erwartetes Ergebnis | Art | Nachweis für |
|---|---|---|---|---|---|---|
| T01 | F03 neue Unterhaltung | Anwendung läuft | Knopf Neue Unterhaltung | leere Unterhaltung mit Kennung, erscheint in der Liste | manuell, auto (ChatService) | M05 |
| T02 | F01 Nachricht senden | Modellserver erreichbar | Text eingeben, Senden | erster Teil innerhalb des Zeitlimits, Antwort endet mit Zustand Fertig | manuell, auto (Fake-Adapter) | M05, M06 |
| T03 | F01 Antwort schrittweise | wie T02 | lange Frage senden | Teile erscheinen nacheinander, nicht als Block | manuell | M06 |
| T04 | F02 Abbrechen | Antwort läuft | Abbrechen während der Ausgabe | Ausgabe stoppt, Teilantwort bleibt markiert, Zustand Abgebrochen, Eingabe sofort frei | manuell, auto | M06 |
| T05 | F04 Unterhaltung öffnen | zwei Unterhaltungen gespeichert | Liste, zweite Unterhaltung wählen | Verlauf in richtiger Reihenfolge, auch nach Neustart der Anwendung | manuell, auto (Store) | M08 |
| T06 | F05 Unterhaltung löschen | Unterhaltung mit Antworten | Löschen, bestätigen | Unterhaltung samt Nachrichten und Antworten weg, Protokoll bleibt | manuell, auto (Store) | M08 |
| T07 | F06 Modellserver nicht erreichbar | Modellserver gestoppt | Nachricht senden | verständliche Meldung mit Grund, kein Absturz, Eingabe frei | manuell, auto (Fake wirft Störfall) | M09 |
| T08 | F07 Konfiguration ungültig | Modellname falsch oder Adresse leer | Anwendung starten, Status | Störfall Konfiguration ungültig mit fehlendem Wert, Betreiber sieht ihn | manuell, auto | M09, Z02 |
| T09 | 4.7 Erweiterung 2a leere Eingabe | Anwendung läuft | leeren Text senden | Hinweis, kein Aufruf des Modellservers | auto | M09 |
| T10 | 4.7 Erweiterung 2b zu lange Eingabe | Eingabegrenze 4000 | 4001 Zeichen senden | Hinweis mit Grenze, kein Aufruf | auto | M09 |
| T11 | Zeitüberschreitung | Zeitlimit 5 Sekunden, Modell langsam | Nachricht senden | Zustand Gestört mit Grund Zeitüberschreitung, Eingabe frei | auto (Fake mit Verzögerung) | M09 |
| T12 | Zustandsübergänge | Fake-Adapter | je Pfeil des Zustandsdiagramms ein Lauf | nur die acht erlaubten Übergänge, kein anderer | auto | M06 |

## Nichtfunktionale Testfälle (mindestens fünf)

| Nr. | Ziel | Voraussetzung | Schritte | Erwartetes Ergebnis | Art | Nachweis für |
|---|---|---|---|---|---|---|
| N01 | Offline (Z01) | Internet deaktiviert | fünf Anfragen nacheinander | alle fünf vollständig beantwortet, kein externer Aufruf | manuell, Protokoll | M04 |
| N02 | Messung Antwortzeit | Referenzsystem, gewähltes Modell | zwei Promptlängen, je dreimal | Zeit bis erster Teil, Gesamtdauer, RAM-Spitze als Tabelle | manuell, Protokolldatei | M11 |
| N03 | Corporate Design (Z04) | Gestaltungselemente aus _Logos | Seite in beiden Fensterbreiten | Logo im Kopf, Farben und Schrift wie vorgegeben, Liste klappt unter 800 Pixel ein | manuell | M07 |
| N04 | Installation durch fremde Person | Paket und Anleitung | Neuinstallation nach Anleitung ohne Hilfe | Anwendung läuft, T02 besteht | manuell, Protokoll | M13 |
| N05 | Kein Chattext im Protokoll (Z07) | mehrere Unterhaltungen | Protokolldatei öffnen | Einträge ohne Chattext und ohne Systemanweisung | manuell | S. 9 |
| N06 | Keine Geheimnisse im Repository | Repository | Suche nach Pfaden, Schlüsseln, Namen | keine Treffer; nur appsettings.example.json mit Platzhaltern | manuell, Skript | S. 13 |
