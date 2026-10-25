# PGR Student Management: Week 5 Starter

## Overview

This starter contains the completed Week 4 modular-monolith reference implementation. It is the starting point for the Week 5 laboratories on data modelling, data architecture, and persistence.

The application currently uses module-owned in-memory storage. During Week 5, you will introduce relational persistence for selected Students-module data and document persistence for the Research Activity Log.

## Technology

The starter uses:

- .NET 8
- ASP.NET Core MVC
- Razor views
- Constructor dependency injection
- A modular-monolith structure
- Module-owned in-memory collections
- Separate seed-data classes

The starter does not yet use:

- Entity Framework Core
- SQLite
- MongoDB
- Database migrations
- Data Access Objects
- Document repositories

You will introduce these during the Week 5 laboratories.

## Existing Behaviour

### Student Management

The Students module implements:

- `W2-UC01`: View Enrolment Details
- `W2-UC02`: View Thesis Details
- `W2-UC03`: Update Student Status
- `W2-UC04`: Update Expected Thesis Submission Date
- `W2-UC05`: Record Actual Thesis Submission
- `W3-UC01`: Create Student
- `W3-UC02`: List Students
- `W3-UC03`: View Student Details

### Academic Supervisor Management

The Academic Supervisors module implements:

- `W4-UC01`: Create Academic Supervisor
- `W4-UC02`: List Academic Supervisors
- `W4-UC03`: View Academic Supervisor Details
- `W4-UC04`: Assign Academic Supervisor to Student
- `W4-UC05`: View Student Supervisory Team

Do not remove or change this observable behaviour while introducing persistence in Week 5.

## Application Structure

The application is organised around business modules:

```text
PgrStudentManagement.Web/
├── Modules/
│   ├── Students/
│   │   ├── Contracts/
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   └── Data/
│   └── AcademicSupervisors/
│       ├── Controllers/
│       ├── Models/
│       ├── Services/
│       └── Data/
├── Views/
│   ├── Students/
│   ├── AcademicSupervisors/
│   └── Shared/
└── Program.cs
```

Use the source code as the definitive description if the exact filenames differ slightly.

## Module Responsibilities

### Students Module

The Students module owns:

- `Student`
- `Status`
- Student creation and retrieval
- Existing enrolment and thesis information
- Student status and thesis-submission updates
- Student Razor views
- Student in-memory data
- The public `IStudentLookup` contract

Its main dependency direction is:

```text
StudentsController
    -> StudentService
        -> StudentStorage
            -> List<Student>
```

### Academic Supervisors Module

The Academic Supervisors module owns:

- `AcademicSupervisor`
- `SupervisorEmploymentType`
- `SupervisoryRole`
- `SupervisoryAssignment`
- Academic Supervisor creation and retrieval
- Role-specific eligibility
- Supervisory-assignment rules
- Academic Supervisor and assignment views
- Supervisor and assignment in-memory data

Its main dependency direction is:

```text
AcademicSupervisorsController
    -> AcademicSupervisorService
        -> AcademicSupervisorStorage
            -> List<AcademicSupervisor>
            -> List<SupervisoryAssignment>
```

## Cross-Module Student Lookup

The Academic Supervisors module confirms that a Student exists through the public Students-module contract:

```text
AcademicSupervisorService
    -> IStudentLookup
        -> StudentService
```

The Academic Supervisors module must not access `StudentStorage` or the private Student collection directly.

Only the minimum Student information required by the supervisory use cases crosses the module boundary, normally through `StudentSummary`.

## In-Memory Storage and Seed Data

Storage and seed-data creation are separated.

```text
StudentStorage
    -> stores and retrieves Student records

StudentDataSeeder
    -> creates the initial Student records

AcademicSupervisorStorage
    -> stores and retrieves Academic Supervisors
    -> stores and retrieves Supervisory Assignments

AcademicSupervisorDataSeeder
    -> creates the initial Academic Supervisors
    -> verifies Student references through IStudentLookup
    -> creates the initial Supervisory Assignments
```

The storage classes begin with empty collections. They do not create their own sample data.

### Seeder Order

The application runs the seeders in this order:

```text
1. StudentDataSeeder
2. AcademicSupervisorDataSeeder
```

Students must exist before supervisory assignments are seeded.

Both seeders should be safe to run more than once without creating duplicate seed records.

## Reference Students

The starter contains four seeded Students:

```text
S100001  John Doe
S100002  Jane Smith
S100003  Casey Wilson
S100004  Morgan Lee
```

## Reference Academic Supervisors

The starter contains four seeded Academic Supervisors:

```text
SUP001  Robin Carter
SUP002  Jamie Ahmed
SUP003  Taylor Green
SUP004  Avery Brown
```

## Reference Supervisory Assignments

```text
S100001
    SUP001 as DirectorOfStudies, current
    SUP002 as FirstSupervisor, current
    SUP003 as SecondSupervisor, historical

S100002
    SUP001 as FirstSupervisor, current

S100003
    No supervisory assignments

S100004
    SUP002 as DirectorOfStudies, current
    SUP003 as Adviser, current
```

This dataset supports testing:

- Several assignments for one Student
- A Student with one assignment
- A known Student with no assignments
- Current and historical assignments
- An external Adviser
- One Academic Supervisor assigned to several Students
- A known Academic Supervisor with no assignments

## Build and Run

From the solution directory, restore and build the solution:

```bash
dotnet restore
dotnet build
```

Run the web application:

```bash
dotnet run --project PgrStudentManagement.Web
```

Use the address displayed in the terminal. The supplied development configuration may use:

```text
http://localhost:5277
```

The actual port may differ on your computer.

## Useful Routes

If the application runs on `http://localhost:5277`, useful routes include:

```text
http://localhost:5277/Students
http://localhost:5277/AcademicSupervisors
http://localhost:5277/AcademicSupervisors/Create
http://localhost:5277/AcademicSupervisors/Team?studentNumber=S100001
http://localhost:5277/AcademicSupervisors/Team?studentNumber=S100002
http://localhost:5277/AcademicSupervisors/Team?studentNumber=S100003
http://localhost:5277/AcademicSupervisors/Team?studentNumber=S100004
```

Expected supervisory-team results:

- `S100001` has current and historical assignments.
- `S100002` has one current assignment.
- `S100003` displays **No supervisory assignments found**.
- `S100004` has two current assignments.
- An unknown Student number displays **Student not found**.

## Run the Tests

If the solution includes `PgrStudentManagement.Tests`, run:

```bash
dotnet test
```

The test project may include:

- Student seeder tests
- Academic Supervisor seeder tests
- Academic Supervisor storage tests
- Academic Supervisor service tests
- `FakeStudentLookup` as a test double

`FakeStudentLookup` belongs only in the test project. It must not be registered in the production application.

## Data Lifetime

All starter data is held in memory.

Changes remain available while the application process is running. When the application stops or restarts:

- Newly created Students are lost.
- Newly created Academic Supervisors are lost.
- Newly created Supervisory Assignments are lost.
- The original reference data is seeded again.

This is the expected starting behaviour for Week 5.
