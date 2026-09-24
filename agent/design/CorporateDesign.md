# Corporate Design der HFU für digitale Applikationen

**Stand:** 21.09.2026

**Referenz:** Homepage `www.hfu.ch`, Version 1.4.0

**Geltungsbereich:** Websites, Web-Applikationen, Portale, interne Fachanwendungen und mobile Oberflächen der HFU

## 1. Zweck und Status

Dieses Dokument beschreibt das aus der neuen Homepage abgeleitete digitale Erscheinungsbild der HFU. Es dient als verbindliche Arbeitsgrundlage für weitere Applikationen, damit Marke, Bedienung und Kommunikation über verschiedene Systeme hinweg wiedererkennbar bleiben.

Die Vorgaben verbinden:

- die freigegebenen Markenentscheide des Homepage-Projekts;
- die in der Homepage tatsächlich eingesetzten Design-Tokens und Komponenten;
- Anforderungen an Barrierefreiheit, responsive Darstellung und redaktionelle Qualität.

Es ersetzt kein allenfalls später erstelltes, rechtlich freigegebenes Markenhandbuch. Wo noch kein offizielles Asset oder keine verbindliche Detailregel vorhanden ist, wird dies ausdrücklich erwähnt.

## 2. Marke und Gestaltungscharakter

### 2.1 Markenbezeichnung

- Hauptmarke: **HFU**
- Ausgeschriebene Bezeichnung: **Höhere Fachschule Uster**
- Die frühere Bezeichnung **HBU** wird nicht als Hauptmarke weitergeführt.
- Die Partnerschaft mit der ibW darf sichtbar erklärt werden, darf den institutionellen Absender HFU aber nicht ersetzen.

### 2.2 Gewünschte Wirkung

Das digitale Erscheinungsbild soll folgende Eigenschaften vermitteln:

- persönlich und zugänglich;
- praxisnah und kompetent;
- regional verankert;
- klar, verlässlich und transparent;
- modern, ohne modisch oder verspielt zu wirken;
- technisch fortschrittlich, ohne die persönliche Begleitung zu verdrängen.

### 2.3 Gestaltungsprinzipien

1. **Klare Hierarchie:** Grosse, prägnante Überschriften, kurze Einleitungen und deutlich erkennbare Handlungen.
2. **Grosszügiger Weissraum:** Inhalte werden in klar getrennten Abschnitten präsentiert und nicht dicht gedrängt.
3. **Starke Kontraste:** HFU-Rot, dunkle Flächen und Weiss bilden das visuelle Grundgerüst.
4. **Modulare Karten:** Angebote, Personen, Vorteile und Fakten werden in wiederverwendbaren Karten dargestellt.
5. **Direkte Nutzerführung:** Jede Ansicht zeigt einen verständlichen nächsten Schritt.
6. **Zurückhaltende Bewegung:** Animationen unterstützen Orientierung, sind kurz und niemals Selbstzweck.
7. **Barrierefreiheit als Standard:** Gestaltung und Bedienung erfüllen mindestens WCAG 2.2 AA.

## 3. Logo und Absender

### 3.1 Freigegebene Ausgangsdateien

| Verwendung | Datei | Format und Abmessung | Vorgabe |
|---|---|---:|---|
| Institutioneller Hauptabsender | `Logo normal.png` | PNG mit Alphakanal, 684 × 185 px | Standard für Header, Dokumente und institutionelle Absender |
| Kurzform als Ausgangsmaterial | `Logo Kurzform.jpg` | JPG, 1772 × 709 px | Nur für geeignete kompakte oder dekorative Anwendungen |
| App-Icon/Favicon | `dev/hfu.ch/src/static/assets/img/hfu-icon.png` | PNG mit Alphakanal, 512 × 512 px | Für quadratische Icons und kleine digitale Flächen |
| Social Preview | `dev/hfu.ch/src/static/assets/img/hfu-social.jpg` | JPG, 1200 × 630 px | Für Link-Vorschauen im Format 1.91:1 |

### 3.2 Verwendungsregeln

- Das Hauptlogo ist der bevorzugte institutionelle Absender.
- Das Seitenverhältnis muss immer erhalten bleiben.
- Das Logo darf nicht beschnitten, nachgezeichnet, gestaucht, gedreht oder mit Schatten und Konturen ergänzt werden.
- Um das Logo ist ausreichend freie Fläche vorzusehen; es darf nicht an Texte, Rahmen oder Fensterränder stossen.
- In digitalen Anwendungen sollte das Hauptlogo nicht kleiner als die auf der Homepage eingesetzte mobile Breite von rund 10.5 rem beziehungsweise 168 CSS-Pixel dargestellt werden. Für kleinere Flächen ist das App-Icon zu verwenden.
- Auf hellen Flächen wird das schwarze Original verwendet.
- Für dunkle Flächen soll langfristig ein offiziell freigegebenes weisses Logo bereitgestellt werden. Die Homepage invertiert das schwarze Logo im Footer derzeit technisch per CSS. Diese technische Ausnahme ist kein eigenständiges Master-Asset und soll nicht unkontrolliert in andere Anwendungen übernommen werden.
- Die Kurzform ersetzt das Hauptlogo nicht in Briefköpfen, Login-Seiten, rechtlichen Ansichten oder anderen institutionellen Absendern.
- Partnerlogos sind optisch nachgeordnet und benötigen genügend Abstand zum HFU-Logo.

## 4. Farbsystem

### 4.1 Primärfarben und neutrale Farben

| Token | Farbe | Rolle |
|---|---|---|
| `red` | `#C51F36` | Primärfarbe, Hauptaktionen, aktive Elemente |
| `red-dark` | `#98172A` | Dunkles HFU-Rot, Links, Labels, hervorgehobene Flächen |
| `red-soft` | `#F9E9EC` | Dezente Hervorhebungen und Callouts |
| `ink` | `#21171A` | Haupttext und dunkle Aktionen |
| `ink-2` | `#463A3E` | Sekundärer Fliesstext |
| `muted` | `#685D61` | Metadaten, Hilfstexte und untergeordnete Information |
| `line` | `#D9D1D3` | Rahmen und Trennlinien |
| `surface` | `#F6F3F2` | Ruhige Sekundärfläche |
| `white` | `#FFFFFF` | Grundfläche und Text auf dunklen Hintergründen |
| `panel` | `#542C38` | Dunkle Markenfläche und Footer |
| `panel-deep` | `#43252E` | Tiefste Markenfläche und Verlaufsende |
| `focus` | `#006D65` | Sichtbare Tastaturfokussierung |

### 4.2 Fachbereichsfarben

Die Farben dienen der Orientierung. Die Fachbereichsbezeichnung muss zusätzlich immer als Text vorhanden sein; Farbe allein darf keine Information tragen.

| Fachbereich | Farbe | Token |
|---|---|---|
| Technik | `#67283A` | `tech` |
| Informatik | `#183E57` | `info` |
| Wirtschaft | `#385240` | `business` |

### 4.3 Ergänzende Akzentfarben

- Helle Akzente auf dunklen Markenflächen: `#FF9BA9`, `#FFB7C2`, `#FFD0D6` bis `#FFD1D7`.
- Hero-Verlauf: `#713247` → `#542C38` → `#43252E` bei etwa 135 Grad.
- Footer-Verlauf: `#542C38` → `#43252E` bei etwa 145 Grad.
- Warnhinweis der Homepage: Rahmen/Akzent `#AD6800`, Hintergrund `#FFF3DC`.

Diese Akzentfarben sind Ergänzungen und keine alternativen Primärfarben.

### 4.4 Kontrastwerte wichtiger Kombinationen

| Kombination | Kontrast |
|---|---:|
| `red` auf Weiss | 5.80:1 |
| `red-dark` auf Weiss | 8.43:1 |
| `ink` auf Weiss | 17.47:1 |
| `muted` auf Weiss | 6.31:1 |
| `focus` auf Weiss | 6.22:1 |
| Weiss auf `tech` | 10.82:1 |
| Weiss auf `info` | 11.26:1 |
| Weiss auf `business` | 8.57:1 |
| Weiss auf `panel` | 11.70:1 |

Neue Farbkombinationen müssen vor der Freigabe erneut gegen WCAG 2.2 AA geprüft werden.

## 5. Typografie

### 5.1 Schriftfamilie

Bevorzugte digitale Schrift ist **Inter**. Die technische Reihenfolge lautet:

```css
font-family: Inter, ui-sans-serif, system-ui, -apple-system,
  BlinkMacSystemFont, "Segoe UI", sans-serif;
```

Die Homepage bindet aktuell keine externe Schriftdatei ein. Inter wird deshalb nur verwendet, wenn sie auf dem System vorhanden ist; andernfalls greift die Systemschrift. Soll Inter in einer weiteren Applikation garantiert erscheinen, muss sie mit geklärter Lizenz lokal ausgeliefert werden. Google Fonts oder andere externe Schriftdienste dürfen nicht ohne Datenschutz- und Sicherheitsfreigabe eingebunden werden.

### 5.2 Schriftschnitte und Hierarchie

| Element | Richtwert |
|---|---|
| Fliesstext | 1 rem / 16 px, Gewicht 400, Zeilenhöhe 1.65 |
| Lead-Text | 1.17–1.45 rem, Zeilenhöhe 1.55 |
| H1 | 2.65–5.75 rem, mobil 2.35–3.4 rem |
| H2 | 2–3.6 rem |
| H3 | 1.25–1.65 rem |
| Überschriften | Gewicht 780; ohne variable Schrift Gewicht 800 verwenden |
| Navigation und Aktionen | Gewicht 700–800 |
| Eyebrow/Kategorielabel | 0.78 rem, Gewicht 800, Laufweite 0.13 em, Versalien |
| Hilfs- und Metatext | 0.82–0.875 rem |

Überschriften verwenden eine kompakte Zeilenhöhe von ungefähr 1.08 und eine leichte negative Laufweite von `-0.035em`. Lange Titel müssen umbrechen können und dürfen nicht abgeschnitten werden.

## 6. Layout und Abstände

### 6.1 Grundraster

- Maximale Inhaltsbreite: `78rem` beziehungsweise 1248 px.
- Schmale Textspalte: maximal `58rem` beziehungsweise 928 px.
- Seitlicher Abstand: standardmässig 1.25 rem je Seite, mobil 0.75 rem je Seite.
- Vertikaler Abschnittsabstand: responsiv ungefähr 4.5–8 rem.
- Standardabstand zwischen Karten: 1.25 rem.
- Grosse zweispaltige Bereiche verwenden Abstände von ungefähr 2.5–7 rem.
- Standard-Eckenradius: `1.25rem` beziehungsweise 20 px.
- Standardschatten: `0 18px 50px rgba(33, 23, 26, 0.11)`.

### 6.2 Responsive Schwellen der Referenzimplementierung

| Breite | Verhalten |
|---:|---|
| über 1120 px | vollständige Desktop-Navigation und mehrspaltige Raster |
| bis 1120 px | Menü wird kompakt; grosse Vierer-/Fünferraster werden reduziert |
| bis 820 px | Hauptlayouts wechseln überwiegend auf eine oder zwei Spalten |
| bis 600 px | einspaltige Darstellung, volle Aktionsbreite und vereinfachte Karten |

Die Schwellen sind Referenzwerte. Andere Applikationen dürfen sie an den tatsächlichen Inhalt anpassen, müssen aber mindestens bei 320, 375, 768, 1024 und 1440 CSS-Pixeln geprüft werden.

## 7. Komponenten

### 7.1 Schaltflächen

- Primäraktion: HFU-Rot mit weisser Schrift.
- Mindesthöhe: etwa 3.15 rem beziehungsweise 50 px.
- Form: vollständig gerundete Kapsel (`border-radius: 999rem`).
- Rand: 2 px in der jeweiligen Aktionsfarbe.
- Beschriftung: konkret und handlungsorientiert, zum Beispiel «Beratung buchen» statt «Mehr».
- Hover: dunkleres Rot und höchstens 1 px Anhebung.
- Sekundäraktion auf heller Fläche: transparenter Hintergrund, dunkler Rand und dunkler Text.
- Sekundäraktion auf dunkler Fläche: transparenter Hintergrund, heller Rand und heller Text.
- In mobilen Ansichten dürfen zentrale Aktionen die volle verfügbare Breite einnehmen.
- Pro Bereich soll eine Handlung klar als Primäraktion erkennbar sein.

### 7.2 Karten und Panels

- Weisser Hintergrund, feine Linie `line`, Radius 20 px.
- Schatten zurückhaltend einsetzen; kein starkes Schweben ohne funktionalen Grund.
- Hover-Effekt bei klickbaren Karten: leichte Anhebung und klarerer Rand.
- Inhaltliche Karten brauchen eine erkennbare Überschrift und dürfen nicht nur über die gesamte Fläche ohne sichtbares Linkziel funktionieren.
- Callouts verwenden bevorzugt `red-soft` und einen roten Akzent.
- Dunkle Markenpanels verwenden Weiss als Textfarbe und sparsame rosa Akzente.

### 7.3 Tags und Statusangaben

- Tags sind klein, kompakt und kapselförmig.
- Standard: `red-dark` mit weisser Schrift, etwa 0.72 rem und Gewicht 800.
- Status darf nie ausschliesslich durch Farbe vermittelt werden; immer Text oder Symbol ergänzen.
- Für einen allgemeinen Erfolgsstatus existiert noch kein verbindlicher HFU-Farbtoken. Ein neuer Token muss vor der breiten Verwendung auf Kontrast und Bedeutung geprüft werden.

### 7.4 Formulare

- Jedes Feld besitzt ein dauerhaft sichtbares Label.
- Eingaben haben mindestens 3 rem Höhe, einen klaren Rand und einen Radius von ungefähr 0.55 rem.
- Pflichtfelder werden textlich oder mit einem erklärten Stern gekennzeichnet.
- Fehler erscheinen direkt beim Feld, verständlich formuliert und programmatisch zugeordnet.
- Fehlerfelder erhalten einen mindestens 2 px starken Rand in `red-dark`.
- Status- und Fehlermeldungen müssen von Screenreadern angekündigt werden.
- Formulare werden auf kleinen Bildschirmen einspaltig dargestellt.
- Datenschutzbestätigungen dürfen nicht vorausgewählt werden.

### 7.5 Navigation und Dialoge

- Die Hauptnavigation ist auf Desktop horizontal und ab 1120 px abwärts als Tastatur-bedienbares Menü ausgeführt.
- Der aktive Navigationspunkt wird durch Text und sichtbare Markierung kenntlich gemacht.
- Dialoge besitzen einen benannten Titel, klaren Schliessen-Mechanismus, Escape-Unterstützung und korrektes Fokusmanagement.
- Modale Hintergründe werden deutlich abgedunkelt, ohne den Dialogkontrast zu beeinträchtigen.

### 7.6 Icons

- Bevorzugt werden einfache Outline-SVG-Icons in einem 24 × 24-Raster.
- Referenz: Strichstärke 1.8, runde Linienenden und Linienverbindungen.
- Dekorative Icons werden für Screenreader ausgeblendet.
- Informative Icons benötigen eine zugängliche Textalternative.
- Icons unterstützen Texte, ersetzen aber keine verständliche Beschriftung.
- Unterschiedliche Icon-Stile oder Emoji als reguläre Bedienicons sind zu vermeiden.

## 8. Bildsprache und grafische Elemente

- Bilder sollen nach Möglichkeit echte HFU-Situationen, Unterricht, Menschen und regionale Bezüge zeigen.
- Generische Stockbilder und nicht belegte Testimonials sind zu vermeiden.
- Personenbilder dürfen nur mit geklärten Nutzungsrechten veröffentlicht werden.
- Portraitkarten verwenden auf der Homepage das Seitenverhältnis 4:5.
- Logos von Mitgliedern oder Partnern werden in neutralen Flächen mit `object-fit: contain` gezeigt und nicht beschnitten.
- Fotos werden für die Zielgrösse optimiert; für Webinhalte ist WebP zu bevorzugen, sofern kein Transparenz- oder Kompatibilitätsgrund dagegenspricht.
- Informative Bilder benötigen einen passenden Alternativtext. Dekorative Bilder erhalten einen leeren Alternativtext beziehungsweise werden semantisch ausgeblendet.
- Dekorative Kreise, Umlaufbahnen und abstrahierte Kompassformen dürfen als wiederkehrendes HFU-Motiv eingesetzt werden. Sie bleiben geometrisch, ruhig und den Inhalten untergeordnet.

## 9. Sprache und Tonalität

### 9.1 Ansprache

- Konsequente **Du-Ansprache**.
- Freundliches Schweizer Hochdeutsch.
- Schweizer Schreibweise mit «ss» statt «ß».
- Direkt, konkret und respektvoll; weder bürokratisch noch übertrieben werblich.

### 9.2 Textprinzipien

- Nutzen und nächster Schritt stehen früh im Text.
- Kurze Absätze, aktive Verben und verständliche Fachbegriffe.
- Anerkennungen, Preise, Termine, Erfolgszahlen und Zulassungsangaben müssen belegt und aktuell sein.
- Keine absoluten Erfolgsversprechen und keine verbindlichen Zulassungszusagen ohne fachliche Prüfung.
- Button- und Linktexte benennen das Ziel: «Angebot ansehen», «Beratung buchen», «Jetzt anmelden».
- Die Abkürzung HFU darf verwendet werden, nachdem der Zusammenhang klar ist.
- Offizielle Abschlussbezeichnungen werden vollständig und konsistent geschrieben.

## 10. Barrierefreiheit und Interaktion

Für alle neuen digitalen Anwendungen gelten mindestens folgende Anforderungen:

- WCAG 2.2 AA;
- Bedienbarkeit sämtlicher Funktionen per Tastatur;
- gut sichtbarer Fokus mit `focus` (`#006D65`) und ausreichendem Abstand;
- logische Fokus- und Lesereihenfolge;
- semantische Überschriftenstruktur mit genau einer Hauptüberschrift pro Ansicht;
- korrekte Landmarken, Labels, Namen und Statusmeldungen;
- Kontrast mindestens 4.5:1 für normalen Text und 3:1 für grossen Text sowie relevante UI-Komponenten;
- keine Information ausschliesslich über Farbe, Position oder Bewegung;
- Nutzbarkeit bei 200 % Zoom und angepassten Textabständen;
- kein horizontales Scrollen bei 320 CSS-Pixeln, ausser bei fachlich unvermeidbaren Datentabellen;
- ausreichende Touch-Ziele von ungefähr 44 × 44 CSS-Pixeln oder grösser;
- Animationen und weiches Scrollen müssen `prefers-reduced-motion` respektieren;
- Inhalte bleiben ohne JavaScript lesbar, soweit es sich nicht um ausdrücklich interaktive Zusatzfunktionen handelt.

## 11. Bewegung und Rückmeldung

- Standardübergänge dauern ungefähr 180–300 ms.
- Bewegung besteht hauptsächlich aus kleinen Farbwechseln, einer Anhebung um 1–4 px oder einer kurzen Icon-Verschiebung.
- Keine automatisch laufenden Karussells, blinkenden Elemente oder grossflächigen Animationen.
- Ladezustände, Erfolge und Fehler werden zusätzlich als Text angezeigt.
- Bei reduzierter Bewegung werden Animationen und weiches Scrollen praktisch deaktiviert.

## 12. Technische Design-Tokens

Neue Web-Applikationen sollen dieselben semantischen Token-Namen verwenden oder sauber darauf abbilden:

```css
:root {
  --hfu-red: #c51f36;
  --hfu-red-dark: #98172a;
  --hfu-red-soft: #f9e9ec;
  --hfu-ink: #21171a;
  --hfu-ink-secondary: #463a3e;
  --hfu-muted: #685d61;
  --hfu-line: #d9d1d3;
  --hfu-surface: #f6f3f2;
  --hfu-white: #ffffff;
  --hfu-panel: #542c38;
  --hfu-panel-deep: #43252e;
  --hfu-focus: #006d65;
  --hfu-area-tech: #67283a;
  --hfu-area-info: #183e57;
  --hfu-area-business: #385240;
  --hfu-radius: 1.25rem;
  --hfu-container: 78rem;
  --hfu-shadow: 0 18px 50px rgba(33, 23, 26, 0.11);
}
```

Die Präfixe verhindern Konflikte mit anwendungseigenen Variablen. Anwendungen dürfen zusätzliche semantische Token für Erfolg, Warnung oder Informationszustände einführen, müssen diese aber dokumentieren und auf Barrierefreiheit prüfen.

## 13. Mindestanforderungen für neue Applikationen

Vor der Freigabe ist zu prüfen:

- HFU als eindeutiger institutioneller Absender;
- korrektes, unverzerrtes Logo und geeignete Logo-Variante;
- Verwendung der freigegebenen Farben und Typografie;
- konsistente Du-Ansprache in Schweizer Hochdeutsch;
- eindeutige Hauptaktion und verständliche Navigation;
- responsive Darstellung bei 320, 375, 768, 1024 und 1440 CSS-Pixeln;
- Tastaturbedienung, sichtbarer Fokus und Screenreader-taugliche Beschriftungen;
- WCAG-konforme Farbkontraste;
- Fehlermeldungen und Statuszustände nicht nur über Farbe;
- keine extern geladenen Schriften, Icons oder Medien ohne Datenschutz- und Sicherheitsprüfung;
- geklärte Nutzungsrechte für Bilder, Logos und Personendarstellungen;
- fachliche Prüfung aller sichtbaren Termine, Preise, Anerkennungen und Zusagen;
- Sichtprüfung in aktuellen Versionen von Chrome, Edge, Firefox und Safari sowie unter iOS und Android.

## 14. Referenzdateien und Pflege

Die aktuelle Referenzimplementierung befindet sich im Homepage-Projekt:

- Projekt-Mockup der Chatoberfläche: `agent/bilder/20260923-001-KI-Chat-Mockup.png`
- Hauptlogo: `Logo normal.png`
- Kurzlogo: `Logo Kurzform.jpg`
- Design-Tokens und Komponenten: `dev/hfu.ch/src/static/assets/css/site.css`
- Seiten- und Komponentenstruktur: `dev/hfu.ch/build.mjs`
- App-Icon: `dev/hfu.ch/src/static/assets/img/hfu-icon.png`
- Social Preview: `dev/hfu.ch/src/static/assets/img/hfu-social.jpg`
- Projektvorgaben: `260809-Homepage DoD v1.md`

Änderungen an Primärfarben, Logo, Schrift, Ansprache oder grundlegenden Komponenten sind zentral zu entscheiden und in diesem Dokument mit Datum nachzuführen. Anwendungsspezifische Abweichungen müssen begründet, dokumentiert und vor der Freigabe geprüft werden.
