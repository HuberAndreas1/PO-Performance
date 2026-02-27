- **Validierung:** Der Parser MUSS jede Zeile validieren und bei Fehlern die passende [CyberLiftParseException](cci:2://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/AppServices/Importer/CyberLiftLogParser.cs:24:0-48:1) mit dem richtigen `CyberLiftParseError`-Enum werfen. Es gibt 16 definierte Fehlercodes:
- [EmptyFile](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:91:4-96:5) — Datei hat keinen Inhalt
- [MissingSessionHeader](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:105:4-115:5) — Erste nicht-leere Zeile beginnt nicht mit `###`
- [InvalidSessionDateFormat](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:117:4-128:5) — Datum nach `###` ist ungültig (erwartet: YYYY-MM-DD)
- `FutureDateError` — Datum liegt in der Zukunft
- `MultipleSessionsInFile` — Mehr als ein `###`-Header gefunden
- [ExerciseNameTooShort](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:159:4-170:5) — Übungsname < 3 Zeichen
- [ExerciseNameTooLong](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:172:4-184:5) — Übungsname > 100 Zeichen
- `DuplicateExerciseInSession` — Gleicher Übungsname kommt doppelt vor
- [MissingExerciseName](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:201:4-211:5) — Set-Zeile (`1. ...`) vor erstem Übungsnamen
- [EmptyExercise](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:227:4-239:5) — Übungsname ohne folgende Sets
- [InvalidSetFormat](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:241:4-252:5) — Set-Zeile fehlt `.` oder `x` Separator
- [InvalidWeightFormat](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:267:4-278:5) — Gewicht fehlt [kg](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/AppServicesTests/BusinessLogicTests.cs:12:4-18:5) oder ist keine gültige Zahl
- [InvalidRepsFormat](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:293:4-304:5) — Wiederholungen sind keine gültige Zahl
- [InvalidSetSequence](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:306:4-318:5) — Set-Index ist nicht genau 1 höher als der vorherige
- `NegativeValueDetected` — Gewicht oder Reps sind ≤ 0
- [CommentaryTooLong](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:359:4-371:5) — Text nach `|` überschreitet 250 Zeichen
- **Wichtig:** Leere Zeilen zwischen Übungen/Sets werden ignoriert. Kommentar ist optional und wird durch `|` getrennt.
- **DTOs vorgegeben:** [ParsedExercise(string Name, List<ParsedSet> Sets)](cci:2://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/AppServices/Importer/CyberLiftLogParser.cs:57:0-57:64) und [ParsedSet(int Index, double Weight, double Reps, string? Commentary)](cci:2://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/AppServices/Importer/CyberLiftLogParser.cs:62:0-62:83)

**Review-Schwerpunkte:**
- Werden alle 16 Fehlerfälle korrekt behandelt und die richtige Exception geworfen?
- Ist die Parsing-Logik robust (Whitespace-Handling, Trimming, leere Zeilen)?
- Wird der State korrekt verwaltet (aktueller Übungsname, Set-Index-Counter)?
- Ist der Code lesbar und gut strukturiert, oder ein monolithischer Block?
- Werden alle 24 vorgegebenen Unit Tests bestanden?

---

#### 2. BusinessLogic ([AppServices/BusinessLogic.cs](cci:7://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/AppServices/BusinessLogic.cs:0:0-0:0))
- **Methode 1:** [CalculateOneRepMax(double weight, double reps)](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/AppServices/BusinessLogic.cs:28:4-31:5) → `double`
- Epley-Formel: `1RM = Weight × (1 + Reps / 30)`
- **Methode 2:** [DetectPlateau(double currentSessionMax, List<double> previousSessionMaxes)](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/AppServices/BusinessLogic.cs:33:4-36:5) → `bool`
- Weniger als 3 vorherige Sessions → `false`
- Sonst: Durchschnitt der 3 letzten Session-Maxima berechnen, Threshold = Durchschnitt + 1%
- Wenn currentSessionMax ≤ Threshold → `true` (Plateau), sonst `false`

**Review-Schwerpunkte:**
- Ist die Epley-Formel korrekt implementiert?
- Ist die Plateau-Erkennung korrekt? Besonders die Grenzfälle (≤ vs <, genau am Threshold)?
- Wird der Fall "weniger als 3 Sessions" korrekt behandelt?
- Werden alle 18 vorgegebenen Unit Tests bestanden?

---

#### 3. ImportDatabaseWriter ([AppServices/Importer/ImportDatabaseWriter.cs](cci:7://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/AppServices/Importer/ImportDatabaseWriter.cs:0:0-0:0))
3 Stub-Methoden mussten implementiert werden (Transaktions-Methoden waren bereits implementiert):
- [GetOrCreateExerciseAsync(string name)](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/AppServices/Importer/ImportDatabaseWriter.cs:48:4-51:5) — Exercise per Name in der DB suchen, wenn nicht vorhanden erstellen (MuscleGroup leer lassen)
- [CreateSessionAsync(DateTime date)](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/AppServices/Importer/ImportDatabaseWriter.cs:53:4-56:5) — Neue TrainingSession erstellen und speichern
- [WriteSetRecordsAsync(IEnumerable<SetRecord> setRecords)](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/AppServices/Importer/ImportDatabaseWriter.cs:58:4-61:5) — SetRecords in die DB schreiben

**Review-Schwerpunkte:**
- Wird [GetOrCreateExerciseAsync](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/AppServices/Importer/ImportDatabaseWriter.cs:48:4-51:5) korrekt implementiert (erst prüfen ob Exercise existiert, dann ggf. erstellen)?
- Werden `SaveChangesAsync()` Aufrufe korrekt platziert?
- Wird der [ApplicationDataContext](cci:2://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/AppServices/DataContext.cs:6:0-38:1) korrekt über den Konstruktor-Parameter verwendet?

---

#### 4. ExerciseEndpoints ([WebApi/ExerciseEndpoints.cs](cci:7://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/WebApi/ExerciseEndpoints.cs:0:0-0:0))
2 Minimal API Endpoints + ihre DTOs mussten implementiert werden:

- **GET `/api/exercises`** → Liste von `ExerciseOverviewDto`:
- [Id](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/WebApiTests/ExerciseIntegrationTests.cs:20:4-30:5) (int), [Name](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:172:4-184:5) (string), `LastSessionDate` (DateTime), `Current1RM` (double), `CurrentMaxWeight` (double), `IsPlateau` (bool)
- Current1RM/CurrentMaxWeight beziehen sich auf die NEUESTE Session

- **GET `/api/exercises/{id}/history`** → Liste von `ExerciseHistoryDto` (chronologisch aufsteigend):
- [Date](cci:1://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/ImporterTests/CyberLiftLogParserTests.cs:130:4-141:5) (DateTime), `SessionMax1RM` (double), `SessionMaxWeight` (double), `IsPlateau` (bool)
- Aggregation: EIN Datenpunkt pro Session, Max-Werte über alle Sets

**Review-Schwerpunkte:**
- Sind die DTOs korrekt definiert mit allen Feldern?
- Sind die LINQ-Queries korrekt? Besonders die Aggregation (GroupBy Session, Max über Sets)?
- Wird `LastSessionDate` tatsächlich das Datum der neuesten Session sein?
- Ist die History chronologisch aufsteigend sortiert?
- Werden die 2 vorgegebenen Integration-Tests bestanden?

---

#### 5. Dashboard Frontend ([Frontend/src/app/dashboard/dashboard.ts](cci:7://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/Frontend/src/app/dashboard/dashboard.ts:0:0-0:0) und [dashboard.html](cci:7://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/Frontend/src/app/dashboard/dashboard.html:0:0-0:0))
- HTML-Struktur und CSS waren vorgegeben (Header, Tabelle, Chart-Sektion)
- TypeScript: Leere Klasse, der Schüler musste implementieren:
- DTO-Interfaces definieren
- Signals für State-Management erstellen
- `HttpClient`-Aufrufe an die API (`GET /api/exercises`, `GET /api/exercises/{id}/history`)
- Daten an [CyberLiftChartComponent](cci:2://file:///Users/andi/Documents/htl/pose/PO-Performance/starter/Frontend/src/app/cyberlift-chart/cyberlift-chart.ts:39:0-173:1) binden (Inputs: `oneRepMaxData`, `maxWeightData`, `highlightPlateaus`)
- `DatePipe` und `DecimalPipe` importieren falls in der Tabelle verwendet
- HTML: Data-Bindings mit `@for`-Loops, Signal-Reads, Pipe-Nutzung, Plateau-Highlighting (CSS-Klasse "plateau"), Click-Handler

**Review-Schwerpunkte:**
- Werden Angular Signals korrekt verwendet?
- Ist HttpClient korrekt injiziert und werden die Aufrufe korrekt gemacht?
- Werden die ChartDataPoints korrekt aus der API-Response gemappt?
- Ist die Tabelle funktional (Plateau-Highlighting, Klick auf Zeile lädt History)?
- Wird die CyberLiftChartComponent korrekt mit Inputs versorgt?

---

#### 6. Zusätzliche Tests (vom Schüler zu schreiben)
- Mindestens **2 weitere Parser-Tests** (z.B. Partial Reps, Boundary-Übungsnamen)
- Mindestens **2 weitere BusinessLogic-Tests** (z.B. Partial Reps 1RM, Plateau-Grenzfall)
- Mindestens **1 weiterer Integration-Test** (z.B. DB seeden → API-Response prüfen)

**Review-Schwerpunkte:**
- Sind die Tests sinnvoll und testen sie tatsächlich neue Edge-Cases?
- Sind die Tests korrekt aufgebaut (Arrange/Act/Assert)?
- Prüfen die Assertions das Richtige?

---

### Bewertungsformat

Strukturiere dein Review in folgende Abschnitte:

1. **Gesamteindruck** — Kurze Zusammenfassung: Funktioniert die Lösung? Wie ist die Gesamtqualität?

2. **Pro Bereich** (Parser, BusinessLogic, DatabaseWriter, Endpoints, Frontend, Tests):
 - ✅ Was gut gemacht wurde
 - ⚠️ Verbesserungsvorschläge
 - ❌ Fehler oder kritische Probleme

3. **Code-Qualität** — Lesbarkeit, Struktur, Namensgebung, Konsistenz

4. **Gesamtbewertung** — Abschließende Einschätzung mit Note/Punkten falls gewünscht

Sei konkret: Zeige Codestellen, erkläre WARUM etwas problematisch ist, und gib einen konkreten Verbesserungsvorschlag. Vermeide generische Aussagen wie "der Code könnte besser sein" — sage stattdessen genau was und wie.
