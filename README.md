# PO-Performance
This is the Progressive Overload Performance WebApp - Designed to enhance your performance in training.

## 1. Overview

CyberLift, a futuristic gym franchise that aims to eliminate the need for pen and paper, has hired you to build their new member portal PO-Performance.

In their facilities, every member carries a "SmartPass" card. When a member approaches a strength machine (like the Chest Press or Preacher Curls), they tap their SmartPass. The machine automatically tracks the weight used and the repetitions performed. Additionally, the machine features a "Quick-Note" system where users can  quickly tag a set with a description of how it felt (e.g., "ach und krach", "last rep was sloppy", or "no rotation").

At the end of a training session, the SmartPass contains a raw log of all exercises performed. CyberLift needs a "Sync Station" web application where these logs can be uploaded, processed, and visualized to help members track their performance over time.

## 2. Data Model

The data model is part of the starter code (`DatabaseModel.cs`). Use it as it is. You do not need to modify it.

### 2.1 Exercise
* **ExerciseID** (system-assigned, numeric identifier, PK in DB)
* **Name** (unique string identifier, max. 100 chars, e.g., "Chest Press")
* **MuscleGroup** (string, max. 50 chars, e.g., "Chest") — *Note: The log file format does not contain muscle group data. Set this to an empty string during import.*

### 2.2 TrainingSession
* **SessionID** (system-assigned, numeric identifier, PK in DB)
* **Date** (DateTime, represents the day the training took place)

### 2.3 SetRecord
* **SetRecordID** (system-assigned, numeric identifier, PK in DB)
* **SessionID** (FK to TrainingSession)
* **ExerciseID** (FK to Exercise)
* **Weight** (double, e.g., `32.5`)
* **Reps** (double, precision for partial reps like `8.5`)
* **Commentary** (optional string, max. 250 chars, contains transcribed notes)
* **Calculated1RM** (double, calculated value for progress tracking)
* **IsPlateau** (boolean, flag to indicate if strength progression has stalled)

## 3. Import File Format

You can find the detailed, technical specification of the exercise import file format in the [file-format-spec.md](./file-format-spec.md) document. The `data/` folder contains sample import files for testing purposes (valid and invalid ones). Make yourself familiar with the format before you start implementing the importer tool.

## 4. Non-Trivial Business Logic

Your import service must calculate and persist the following data **during the sync process** (not on-the-fly when querying).

### 4.1 One-Rep Max (1RM) Calculation
**Goal:** Normalize performance across different repetition ranges to allow for accurate strength comparison.

**The Rule:**
For **every single set** imported, calculate the **Estimated 1RM** using the **Epley Formula**:

$$1RM = Weight \times \left(1 + \frac{Reps}{30}\right)$$

**Implementation Details:**
* **Precision:** Calculate using `double`.
* **Storage:** Store the result in the `Calculated1RM` property of the `SetRecord`.

### 4.2 Plateau Detection
**Goal:** Identify if the user's strength progress has stalled based on their recent history.

**The Algorithm:**
After calculating the 1RM for the current session's sets, perform the following check:

1.  **Determine Session Max:** Identify the highest `Calculated1RM` achieved in the *current* session for the specific exercise.
2.  **Fetch History:** Query the database for the **Session Max** of the **3 most recent previous sessions** for that exercise (ordered by date descending).
3.  **Calculate Baseline:** Calculate the arithmetic average of those 3 previous Session Max values.
4.  **Compare:**
    * If the history contains fewer than 3 sessions, `IsPlateau` is `false`.
    * If the **Current Session Max** is **$\le$ (Baseline Average + 1%)**, then the user has plateaued.
    * Set the `IsPlateau` flag to `true` for all sets in the current session.
    * Otherwise, set `IsPlateau` to `false`.

**Example:**
* *Previous 3 Maxes:* 100kg, 102kg, 98kg $\rightarrow$ Average: 100kg.
* *Threshold:* 100kg + 1% = 101kg.
* *Current Max:* 100.5kg.
* *Result:* $100.5 < 101$, therefore **Plateau Detected**.


## 5. Technical Requirements - Web API & Frontend

### 5.1 Web API (Minimal API)

The Web API acts as the **Data Access Layer** for the dashboard. It does **not** handle the import process (which is simulated internally via a CLI tool/Test). Its sole responsibility is to serve the processed data to the frontend.

You must implement the following **GET** endpoints.

#### **1. Dashboard Overview**

| | |
|---|---|
| **Route** | `GET /api/exercises` |
| **Response** | A JSON list of `ExerciseOverviewDto` |

**`ExerciseOverviewDto` Structure:**

| Field | Type | Description |
|-------|------|-------------|
| `Id` | int | Exercise identifier |
| `Name` | string | Exercise name |
| `LastSessionDate` | DateTime | Date of most recent session |
| `Current1RM` | double | Highest *calculated 1RM* from the most recent session |
| `CurrentMaxWeight` | double | Heaviest *raw weight* lifted in the most recent session |
| `IsPlateau` | bool | Plateau status from the most recent session |

#### **2. Exercise History**

| | |
|---|---|
| **Route** | `GET /api/exercises/{id}/history` |
| **Response** | A JSON list of `ExerciseHistoryDto` ordered by date (ascending) |
| **Aggregation** | Return **one data point per session**. Aggregate sets to find daily maximums. |

**`ExerciseHistoryDto` Structure:**

| Field | Type | Description |
|-------|------|-------------|
| `Date` | DateTime | Session date |
| `SessionMax1RM` | double | Highest *calculated 1RM* of that session |
| `SessionMaxWeight` | double | Heaviest *raw weight* lifted in that session |
| `IsPlateau` | bool | Whether that session was flagged as a plateau |

### 5.2 Angular Frontend

> **Note:** The charting library **Chart.js** is already installed as a dependency.

#### **Feature 1: The Dashboard Table**
* Implement a table listing all exercises fetched from `GET /api/exercises`.
* **Columns:** Name, Last Date, Current 1RM, Current Weight.
* **Visual Feedback:** If `IsPlateau` is `true`, the row or a specific cell must be highlighted (e.g., Red background or Warning Icon).
* *User Interaction:* Clicking a row selects the exercise and updates the details view.
* The HTML structure and CSS are provided in the starter code. You need to implement the TypeScript logic only (API calls, signals, data binding).

#### **Feature 2: The Progression Charts**
* **Requirement:** You do **not** need to implement the charts yourself. A `CyberLiftChartComponent` is provided in the starter code.
* **Task:**
    1.  Call `GET /api/exercises/{id}/history` when an exercise is selected.
    2.  Map the API response to the format expected by the component.
    3.  Bind the data to the component's inputs:
        * `[oneRepMaxData]`: Takes the list of `SessionMax1RM` and `Date`.
        * `[maxWeightData]`: Takes the list of `SessionMaxWeight` and `Date`.
        * `[highlightPlateaus]`: Takes the list of dates where `IsPlateau` is true.

### 5.3 Automated Tests

The starter code already provides test classes with pre-written test cases that serve as the **specification**. Your implementation must make all existing tests pass. Additionally, you must add the following tests yourself.

#### Importer Unit Tests (`CyberLiftLogParserTests.cs`)
The following test cases are already provided and must pass:
* Valid file parsing (session date, exercises, sets, commentary)
* All 16 error codes from the file-format-spec

**You must add at least 2 more tests**, for example:
* Parsing a file with partial reps (e.g., `8.5`)
* Parsing a file where the exercise name has exactly 3 characters (boundary)
* Parsing a file with many exercises and sets to verify correct indexing

#### Business Logic Unit Tests (`BusinessLogicTests.cs`)
The following test cases are already provided and must pass:
* 1RM calculation with various weight/rep combinations
* Plateau detection: below threshold, above threshold, at threshold, insufficient history

**You must add at least 2 more tests**, for example:
* 1RM calculation with partial reps (e.g., `8.5` reps)
* Plateau detection with exactly 3 sessions where improvement is just above the 1% threshold (e.g., average 100, current 101.01 → no plateau)
* Plateau detection with identical previous maxes (e.g., all 3 sessions at 50kg)

#### Web API Integration Tests (`ExerciseIntegrationTests.cs`)
The following test cases are already provided and must pass:
* `GET /api/exercises` returns `200 OK` with JSON content
* `GET /api/exercises/{id}/history` for a non-existent ID returns `200` or `404`

**You must add at least 1 more test**, for example:
* Seed the database with test data (create an `Exercise`, a `TrainingSession`, and a `SetRecord`), then verify that `GET /api/exercises` returns the correct `ExerciseOverviewDto` with the expected `Name`, `Current1RM`, and `IsPlateau` values
