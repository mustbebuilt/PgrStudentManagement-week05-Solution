# PGR Student Management - Week 5 Persistence Architecture

This document summarizes the changes introduced to the application across **Lab 1 (Relational Data Modelling & Persistence with SQLite)** and **Lab 2 (Document Data Modelling & NoSQL Persistence with MongoDB)**.

---

## Architecture & Technology Stack

The application has evolved from an entirely in-memory modular monolith into a multi-model architecture combining relational and document persistence while preserving clean module boundaries:

| Capability | Persistence Technology | Access Mechanism | Owning Module |
|---|---|---|---|
| **Students & Enrolments** | Relational (**SQLite**) via **Entity Framework Core 10** | `EnrolmentDao` & `StudentManagementDbContext` | `Students` |
| **Research Activity Log** | Document (**MongoDB**) via **MongoDB .NET Driver 3** | `ResearchActivityRepository` & `IMongoClient` | `ResearchActivities` |
| **Academic Supervisors** | In-Memory Collections | `AcademicSupervisorStorage` | `AcademicSupervisors` |

---

## Lab 1: Relational Persistence with SQLite & EF Core

### 1. Purpose & Learning Outcomes
- Transition selected student and enrolment data from volatile in-memory collections to a persistent relational database.
- Introduce `Enrolment` as a first-class domain concept separate from `Student` (one student has zero, one, or many enrolments over time).
- Implement Data Access Object (DAO) pattern to encapsulate database queries behind the service layer.
- Preserve module encapsulation and existing Week 4 behaviour.

### 2. Dependencies & Configuration
- **Installed NuGet Packages** in `PgrStudentManagement.Web.csproj`:
  - `Microsoft.EntityFrameworkCore` (v10.0.13)
  - `Microsoft.EntityFrameworkCore.Sqlite` (v10.0.13)
  - `Microsoft.EntityFrameworkCore.Design` (v10.0.13)
- **Updated EF Core CLI Tool**:
  - Updated `dotnet-ef` tool to match the runtime version (`10.0.13`).
- **Connection String in `appsettings.json`**:
  ```json
  "ConnectionStrings": {
    "StudentManagementDatabase": "Data Source=student-management.db"
  }
  ```

### 3. Domain Model & Relational Mappings
- **`Modules/Students/Models/EnrolmentStatus.cs`**:
  - Created enum with values: `Active`, `Suspended`, `Completed`, `Withdrawn`.
- **`Modules/Students/Models/Enrolment.cs`**:
  - Created entity with `EnrolmentId` (PK, auto-increment), `StudentNumber` (FK), `ProgrammeCode`, `StartDate`, `ExpectedEndDate` (nullable), `ModeOfStudy`, and `Status`.
  - Added navigation property `public Student Student { get; set; } = null!;`.
- **`Modules/Students/Models/Student.cs`**:
  - Added primary key annotation `[Key]` and `[MaxLength(20)]` to `StudentNumber`.
  - Added required length constraints and unique index `[Index(nameof(EmailAddress), IsUnique = true)]`.
  - Added navigation collection `public ICollection<Enrolment> Enrolments { get; set; }`.
  - Decorated legacy in-memory properties with `[NotMapped]` to prevent schema mismatch while ensuring backwards compatibility with Week 4 use cases.
- **`Modules/Students/Data/StudentManagementDbContext.cs`**:
  - Implemented `DbContext` with `DbSet<Student> Students` and `DbSet<Enrolment> Enrolments`.
  - Configured 1-to-many relationship in `OnModelCreating` with `DeleteBehavior.Restrict` (preventing accidental cascade deletion of enrolment histories).
  - Configured `Status` to be stored as readable text (`HasConversion<string>()`) with `MaxLength(30)`.

### 4. Database Migrations & Seeding
- **Created Migration**: `InitialStudentEnrolmentModel` containing `Students` and `Enrolments` tables, foreign keys, indexes, and constraints matching the initial ERD.
- **Database Schema**: Applied to `student-management.db` via `dotnet ef database update`.
- **`Modules/Students/Data/StudentDataSeeder.cs`**:
  - Added `SeedAsync(StudentManagementDbContext context)` to insert initial students (`S100001` - `S100004`) and 4 enrolments across active, completed, suspended, full-time, and part-time states.
  - Added duplicate check (`context.Students.AnyAsync()`) to guarantee idempotency.
  - Retained in-memory `Seed()` method for backwards compatibility with legacy services and unit tests.

### 5. Data Access Layer & Presentation
- **`Modules/Students/Data/Daos/EnrolmentDao.cs`**:
  - Created DAO encapsulating the EF Core query using `.AsNoTracking()`, filtering by `StudentNumber`, and ordering by `StartDate`.
- **`Modules/Students/Services/StudentService.cs`**:
  - Injected `EnrolmentDao` and exposed `GetEnrolmentsForStudentAsync(string studentNumber)`.
- **`Modules/Students/Controllers/StudentsController.cs`**:
  - Added `Enrolments(string studentNumber)` action returning the list of enrolments or setting an empty-result message.
- **`Views/Students/Enrolments.cshtml`**:
  - Created Razor view to render student enrolments in chronological order with formatted dates and status values, or display `"No enrolments found"`.
- **Navigation**:
  - Added "View enrolments" links in both `Views/Students/Index.cshtml` and `Views/Students/Details.cshtml`.

### 6. Relational Verification & Tests
- Added `PgrStudentManagement.Tests/Data/EnrolmentDaoTests.cs` using SQLite in-memory provider to test:
  - Seeding records and duplicate prevention.
  - Chronological ordering of enrolments (`S100002`).
  - Correct handling of nullable `ExpectedEndDate` (`S100003`).
  - Empty enrolment response for students without enrolments (`S100004`).

---

## Lab 2: Document Persistence with MongoDB

### 1. Purpose & Learning Outcomes
- Introduce the **Research Activity Log** as a new business module using document (NoSQL) persistence.
- Store polymorphically structured activity data (reflections, supervision meetings, conference attendance) in a single document collection without requiring sparse relational tables or complex joins.
- Use reference identifier (`studentNumber`) across module boundaries instead of embedding complete student entity records.
- Implement repository pattern for document persistence and display data through MVC.

### 2. Dependencies & Configuration
- **Installed NuGet Package** in `PgrStudentManagement.Web.csproj`:
  - `MongoDB.Driver` (v3.13.0)
- **Connection Configuration in `appsettings.json`**:
  ```json
  "ResearchActivityDatabase": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "StudentManagement",
    "CollectionName": "ResearchActivities"
  }
  ```

### 3. Document Model
- **`Modules/ResearchActivities/Models/ResearchActivityType.cs`**:
  - Created enum: `ResearchReflection`, `SupervisionMeeting`, `ConferenceAttendance`, `Publication`, `Experiment`, `TrainingEvent`.
- **`Modules/ResearchActivities/Models/ResearchActivity.cs`**:
  - `[BsonId] public string Id { get; set; }`
  - `public string StudentNumber { get; set; }`
  - `[BsonRepresentation(BsonType.String)] public ResearchActivityType ActivityType { get; set; }`
  - `public string Title { get; set; }`
  - `public DateTime ActivityDate { get; set; }`
  - `public DateTime CreatedOn { get; set; }`
  - `public List<string> Tags { get; set; } = []`
  - `public Dictionary<string, object> Details { get; set; } = new()`: Flexible polymorphic payload storing activity-specific details.

### 4. Repository & Seeder
- **`Modules/ResearchActivities/Data/ResearchActivityRepository.cs`**:
  - Injected `IMongoClient` and `IConfiguration` to resolve `IMongoCollection<ResearchActivity>`.
  - Implemented `GetForStudentAsync(string studentNumber)` querying by student number and sorting by `ActivityDate` ascending.
  - Implemented `AddAsync(ResearchActivity activity)`.
- **`Modules/ResearchActivities/Data/ResearchActivityDataSeeder.cs`**:
  - Implemented `SeedAsync(ResearchActivityRepository repository)` with idempotency check.
  - Seeded `activity-001` (`ResearchReflection` with `summary` and `nextSteps`) for `S100001`.
  - Seeded `activity-002` (`SupervisionMeeting` with `discussion` and `actions`) for `S100001`.
  - Seeded `activity-003` (`ConferenceAttendance` with `conferenceName`, `location`, `presentationTitle`) for `S100002`.
  - Left `S100003` and `S100004` empty to test the alternative flow.

### 5. Service, Controller & Views
- **`Modules/ResearchActivities/Services/ResearchActivityService.cs`**:
  - Coordinates use case and calls repository: `GetActivitiesForStudentAsync(string studentNumber)`.
- **`Modules/ResearchActivities/Controllers/ResearchActivityController.cs`**:
  - Receives `studentNumber`, fetches activities via service, and passes model to view.
  - Handles empty results with `"No research activities found"` message.
  - Supported routing for both `/ResearchActivity` and `/ResearchActivities`.
- **`Views/ResearchActivities/Index.cshtml` & `Views/ResearchActivity/Index.cshtml`**:
  - Renders student number, activity date, type, title, and tags.
  - Displays appropriate type-specific detail sections for each activity variant.
  - Displays alert banner when no activities exist.
- **Navigation**:
  - Added "Research activities" link to `Views/Students/Index.cshtml` and `Views/Students/Details.cshtml`.

### 6. Dependency Injection & Startup Execution
- **`Program.cs` Updates**:
  - Registered `IMongoClient` singleton with configured connection string.
  - Registered `ResearchActivityRepository` and `ResearchActivityService` as scoped services.
  - Executed `await ResearchActivityDataSeeder.SeedAsync(researchActivityRepository)` during application startup.
  - Added custom controller route pattern for `ResearchActivities`.

### 7. Document Verification & Tests
- Added `PgrStudentManagement.Tests/Data/ResearchActivityRepositoryTests.cs` testing:
  - Seeding of multiple activity types.
  - Idempotency when running seeder multiple times.
  - Chronological ordering by `ActivityDate`.
  - Insertion and retrieval of polymorphic dictionary details.
  - Verification of seeded documents in the default MongoDB database.

---

## Verification & Test Results

All 25 automated tests across the solution pass successfully:

```bash
dotnet test
```

```text
Passed!  - Failed: 0, Passed: 25, Skipped: 0, Total: 25, Duration: 1 s - PgrStudentManagement.Tests.dll (net10.0)
```

### Test Breakdown:
- **Relational Persistence**: `EnrolmentDaoTests` (5 tests)
- **Document Persistence**: `ResearchActivityRepositoryTests` (5 tests)
- **Academic Supervisors**: Storage, Seeder, Service, and Seed Order Tests (11 tests)
- **Students Baseline**: Student Data Seeder Tests (4 tests)

---

## Running the Application

### 1. Prerequisites
- **.NET SDK 10.0**
- **MongoDB** daemon running locally on `localhost:27017`

### 2. Start Application
```bash
dotnet run --project PgrStudentManagement.Web
```

### 3. Useful URLs for Testing
- **Students List**: `http://localhost:5277/Students`
- **View Enrolments (Relational / SQLite)**:
  - `http://localhost:5277/Students/Enrolments?studentNumber=S100001` (1 enrolment)
  - `http://localhost:5277/Students/Enrolments?studentNumber=S100002` (2 enrolments in chronological order)
  - `http://localhost:5277/Students/Enrolments?studentNumber=S100003` (1 enrolment with unrecorded expected end date)
  - `http://localhost:5277/Students/Enrolments?studentNumber=S100004` (displays *No enrolments found*)
- **View Research Activities (Document / MongoDB)**:
  - `http://localhost:5277/ResearchActivities?studentNumber=S100001` (2 activities: reflection & meeting)
  - `http://localhost:5277/ResearchActivities?studentNumber=S100002` (1 activity: conference attendance)
  - `http://localhost:5277/ResearchActivities?studentNumber=S100004` (displays *No research activities found*)
- **Academic Supervisors**: `http://localhost:5277/AcademicSupervisors`
