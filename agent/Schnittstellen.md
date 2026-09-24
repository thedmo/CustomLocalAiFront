# Schnittstellen

Referenz für Entwickler und KI-Werkzeuge. Quelle ist das Konzept, Kapitel 7.3 und 7.4; ändert sich hier etwas, wird das Konzept im selben Schritt angepasst (AGENTS.md Abschnitt 10). Namen im Code tragen das Suffix Async (`agent/StylingGuide.md` Abschnitt 1).

## Chat-Seite zu Backend: `IChatService` (Chat.Core)

| Methode                                         | Funktion | Eingabe                                       | Ausgabe                                                                                                                     |
| ----------------------------------------------- | -------- | --------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| `NeueUnterhaltungAsync(ct)`                     | F03      | keine                                         | Reservierte Kennung eines noch ungespeicherten Entwurfs                                                                     |
| `SendeNachrichtAsync(unterhaltungId, text, ct)` | F01      | Kennung, Text, technischer Lebenszyklus-Token | `Task<AntwortLauf>` mit Antwort-ID und `IAsyncEnumerable<string>`; die ID ist vor dem Modellaufruf verfügbar                |
| `AbbrechenAsync(antwortId, ct)`                 | F02      | Kennung der aktiven Antwort                   | ChatService löst den internen Anfrage-Token aus; nach Speicherung Zustand Abgebrochen, sofern noch kein Endzustand vorliegt |
| `ListeUnterhaltungenAsync(ct)`                  | F04      | keine                                         | Kennungen, Titel, Datum                                                                                                     |
| `OeffneUnterhaltungAsync(id, ct)`               | F04      | Kennung                                       | Nachrichten und Antworten in Reihenfolge                                                                                    |
| `LoescheUnterhaltungAsync(id, ct)`              | F05      | Kennung                                       | Bestätigung                                                                                                                 |
| `StatusAsync(ct)`                               | F06, K07 | keine                                         | Modellserver erreichbar, Modellname, Konfiguration gültig                                                                   |

`AntwortLauf` enthält `Guid AntwortId` und `IAsyncEnumerable<string> Teile`. Der ChatService speichert die Antwort zunächst als Angefordert und verwaltet ihren Abbruch. Der `ct` von `SendeNachrichtAsync` beendet den technischen Lebenszyklus, beispielsweise beim endgültigen Circuit-Verlust; der Benutzerabbruch erfolgt ausschliesslich über `AbbrechenAsync`.

Fehler kommen als `StoerfallException` mit einem der vier Fälle: `ModellserverNichtErreichbar`, `ModellUnbekannt`, `KonfigurationUngueltig`, `Zeitueberschreitung`. Die Seite zeigt die Meldung mit Grund (M09), die Eingabe bleibt frei. `StatusAsync` gehört zum C#-Vertrag. Ein zusätzlicher HTTP-Endpunkt `GET /status` ist nicht Teil von AP07.

Implementierungsstand F04, freigegeben durch PS am 23.09.2026: `ListeUnterhaltungenAsync(CancellationToken ct)` liefert `Task<IReadOnlyList<UnterhaltungInfo>>`; `UnterhaltungInfo` enthält `Guid Id`, `string Titel`, `DateTimeOffset ErstelltAm`. `OeffneUnterhaltungAsync(Guid id, CancellationToken ct)` liefert `Task<Unterhaltung>`. Beide verwenden ausschliesslich den Store; unbekannte Kennungen ergeben `KeyNotFoundException`. Die Liste ist nach ErstelltAm absteigend, danach Id sortiert; Nachrichten nach Zeit aufsteigend, danach Id.

Bedienablauf F03, präzisiert durch PS am 24.09.2026: Beim Seitenstart und bei „Neue Unterhaltung“ bleibt der leere Entwurf ausschliesslich im Zustand der Chat-Seite und verwendet `Guid.Empty`; die Seite ruft noch keine schreibende Service-Methode auf. Erst beim ersten Senden reserviert `NeueUnterhaltungAsync` eine Kennung im ChatService und `SendeNachrichtAsync` speichert Unterhaltung und erste Nachricht gemeinsam in einer Store-Operation. Sobald der Service den `AntwortLauf` geliefert hat, aktualisiert die Seite die Liste. Scheitert die erste Speicherung, verwirft `LoescheUnterhaltungAsync` die Reservierung; in der Datenbank bestand zu keinem Zeitpunkt eine leere Unterhaltung. Die Methodensignaturen bleiben unverändert.

Implementierungsstand F05, beauftragt durch PS am 24.09.2026: `LoescheUnterhaltungAsync(Guid id, CancellationToken ct)` liefert `Task`; erfolgreicher Abschluss bestätigt die Löschung, auch bei bereits fehlender Kennung. Eine aktive Antwort verhindert die Löschung mit `InvalidOperationException`. Alle ChatService-Instanzen am selben Singleton-Store teilen die aktiven Anfragen und eine asynchrone Sperre für Antwortstart und Löschen. Die Registrierung bleibt bis zum gespeicherten Endzustand bestehen. Dadurch kann auch eine zweite Sitzung während einer Antwort nicht löschen; beginnt Löschen zuerst, scheitert späteres Senden beim Laden mit `KeyNotFoundException`. Die Seite behandelt beide Fälle verständlich. Nach dem Löschen öffnet sie eine verbleibende Unterhaltung oder zeigt eine leere Liste; sie erzeugt keine Ersatz-Unterhaltung. Diese Koordination gilt innerhalb des vorgesehenen einzelnen Backend-Prozesses.

## Backend zu Modellserver: `IModelServerClient` (Chat.Core), Umsetzung `ModelRunnerClient`

| Methode                                            | Zweck                   | Eingabe                                                   | Ausgabe                    |
| -------------------------------------------------- | ----------------------- | --------------------------------------------------------- | -------------------------- |
| `StreamAntwortAsync(verlauf, systemanweisung, ct)` | Antwort in Teilen holen | Verlauf als Liste von Nachrichten, Systemanweisung, Token | `IAsyncEnumerable<string>` |

Der Adapter sendet `POST chat/completions` mit Modellname, Systemanweisung und Verlauf, `stream=true`, und liest die Teile als Server-Sent Events. Adresse und Modellname kommen aus der Konfiguration. Docker Compose übergibt die von Docker Model Runner bereitgestellte Adresse als `MODEL_RUNNER_URL`; ausserhalb von Compose gilt `Chat:AdresseModellserver`. Nur der Adapter kennt das HTTP-Protokoll des Modellservers.

## Backend zu Datenhaltung: `IStore` (Chat.Core), Umsetzung `SqliteStore`

| Methode                            | Zweck                                                                                             |
| ---------------------------------- | ------------------------------------------------------------------------------------------------- |
| `SpeichernAsync(unterhaltung, ct)` | Unterhaltung mit Nachrichten und Antworten anlegen oder aktualisieren; eine Transaktion je Aufruf |
| `LadenAsync(id, ct)`               | Unterhaltung mit Verlauf laden                                                                    |
| `ListeAsync(ct)`                   | Kennung, Titel, Datum aller Unterhaltungen                                                        |
| `LoeschenAsync(id, ct)`            | Unterhaltung mit Nachrichten und Antworten entfernen (Kaskade), Protokoll bleibt                  |
| `ProtokollAsync(eintrag, ct)`      | technischer Eintrag ohne Chattext (Konzept 9.3)                                                   |

Implementierungsstand AP09/F05: `IStore.ListeAsync(CancellationToken ct)` liefert `Task<IReadOnlyList<UnterhaltungInfo>>`. `SpeichernAsync` ist transaktional, `LadenAsync` rekonstruiert gespeicherte Zustände. `Task LoeschenAsync(Guid id, CancellationToken ct)` entfernt gezielt die Unterhaltung mit den vorhandenen SQLite-Kaskaden; fehlende Kennungen sind erfolgreich, technische Protokolle werden nicht angesprochen. Der Service koordiniert die Operation mit aktiven Antworten. Nur `ProtokollAsync` bleibt Zielvertrag für AP12 und ist noch nicht im C#-Interface enthalten. Startup-Migration und Neustart-Wiederherstellung gehören zur technischen Store-Einbindung und erweitern den fachlichen Vertrag nicht.

## Konfiguration (Abschnitt `Chat` in `appsettings.json`, Konzept 7.4)

`AdresseModellserver`, `Modellname`, `Systemanweisung`, `Temperatur`, `MaximaleAntwortlaenge`, `Eingabegrenze` (4000 Zeichen), `ZeitlimitSekunden` (60), `SpeicherortDb`, `Protokolldatei`. Überschreibbar per Umgebungsvariable `Chat__Modellname`. Beispiel in `src/LocalAiFront/appsettings.example.json`.

## Zustände der Antwort (Konzept 4.7.6)

`Angefordert` nach dem Speichern der Nachricht, `Laeuft` ab dem ersten Teil, `Fertig` nach dem Ende, `Abgebrochen` nach Abbruch, `Gestoert` bei einem Störfall. Erlaubt sind `Angefordert → Laeuft`, `Angefordert → Abgebrochen`, `Angefordert → Gestoert`, `Laeuft → Fertig`, `Laeuft → Abgebrochen` und `Laeuft → Gestoert`. Aus `Fertig`, `Abgebrochen` und `Gestoert` gibt es keinen Übergang mehr.
