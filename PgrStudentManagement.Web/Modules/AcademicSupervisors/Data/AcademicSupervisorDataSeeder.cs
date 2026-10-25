using PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;
using PgrStudentManagement.Web.Modules.Students.Contracts;

namespace PgrStudentManagement.Web.Modules.AcademicSupervisors.Data;

public class AcademicSupervisorDataSeeder
{
    private readonly AcademicSupervisorStorage _storage;

    private readonly IStudentLookup _studentLookup;

    public AcademicSupervisorDataSeeder(
        AcademicSupervisorStorage storage,
        IStudentLookup studentLookup)
    {
        _storage = storage;
        _studentLookup = studentLookup;
    }

    public void Seed()
    {
        SeedAcademicSupervisors();
        SeedSupervisoryAssignments();
    }

    private void SeedAcademicSupervisors()
    {
        AddSupervisorIfMissing(
            new AcademicSupervisor
            {
                StaffNumber = "SUP001",
                FirstName = "Robin",
                LastName = "Carter",
                EmailAddress =
                    "robin.carter@example.test",
                EmploymentType =
                    SupervisorEmploymentType
                        .InstitutionEmployed,
                TrainingCompleted = true,
                EligibleRoles =
                [
                    SupervisoryRole
                        .DirectorOfStudies,
                    SupervisoryRole
                        .FirstSupervisor,
                    SupervisoryRole
                        .SecondSupervisor
                ]
            });

        AddSupervisorIfMissing(
            new AcademicSupervisor
            {
                StaffNumber = "SUP002",
                FirstName = "Jamie",
                LastName = "Ahmed",
                EmailAddress =
                    "jamie.ahmed@example.test",
                EmploymentType =
                    SupervisorEmploymentType
                        .InstitutionEmployed,
                TrainingCompleted = true,
                EligibleRoles =
                [
                    SupervisoryRole
                        .DirectorOfStudies,
                    SupervisoryRole
                        .FirstSupervisor,
                    SupervisoryRole
                        .SecondSupervisor
                ]
            });

        AddSupervisorIfMissing(
            new AcademicSupervisor
            {
                StaffNumber = "SUP003",
                FirstName = "Taylor",
                LastName = "Green",
                EmailAddress =
                    "taylor.green@example.test",
                EmploymentType =
                    SupervisorEmploymentType.External,
                TrainingCompleted = true,
                EligibleRoles =
                [
                    SupervisoryRole
                        .SecondSupervisor,
                    SupervisoryRole.Adviser
                ]
            });

        AddSupervisorIfMissing(
            new AcademicSupervisor
            {
                StaffNumber = "SUP004",
                FirstName = "Avery",
                LastName = "Brown",
                EmailAddress =
                    "avery.brown@example.test",
                EmploymentType =
                    SupervisorEmploymentType
                        .InstitutionEmployed,
                TrainingCompleted = true,
                EligibleRoles =
                [
                    SupervisoryRole
                        .FirstSupervisor,
                    SupervisoryRole
                        .SecondSupervisor,
                    SupervisoryRole.Adviser
                ]
            });
    }

    private void AddSupervisorIfMissing(
        AcademicSupervisor supervisor)
    {
        var existing =
            _storage.GetAcademicSupervisor(
                supervisor.StaffNumber);

        if (existing is null)
        {
            _storage.AddAcademicSupervisor(
                supervisor);
        }
    }

    private void SeedSupervisoryAssignments()
    {
        AddAssignmentIfMissing(
            studentNumber: "S100001",
            staffNumber: "SUP001",
            role:
                SupervisoryRole.DirectorOfStudies,
            startDate:
                new DateTime(2025, 10, 1));

        AddAssignmentIfMissing(
            studentNumber: "S100001",
            staffNumber: "SUP002",
            role:
                SupervisoryRole.FirstSupervisor,
            startDate:
                new DateTime(2025, 10, 1));

        AddAssignmentIfMissing(
            studentNumber: "S100001",
            staffNumber: "SUP003",
            role:
                SupervisoryRole.SecondSupervisor,
            startDate:
                new DateTime(2024, 10, 1),
            endDate:
                new DateTime(2025, 9, 30));

        AddAssignmentIfMissing(
            studentNumber: "S100002",
            staffNumber: "SUP001",
            role:
                SupervisoryRole.FirstSupervisor,
            startDate:
                new DateTime(2026, 1, 15));

        AddAssignmentIfMissing(
            studentNumber: "S100004",
            staffNumber: "SUP002",
            role:
                SupervisoryRole.DirectorOfStudies,
            startDate:
                new DateTime(2025, 10, 1));

        AddAssignmentIfMissing(
            studentNumber: "S100004",
            staffNumber: "SUP003",
            role:
                SupervisoryRole.Adviser,
            startDate:
                new DateTime(2025, 10, 1));
    }

    private void AddAssignmentIfMissing(
        string studentNumber,
        string staffNumber,
        SupervisoryRole role,
        DateTime startDate,
        DateTime? endDate = null)
    {
        var student =
            _studentLookup.GetStudentSummary(
                studentNumber)
            ?? throw new InvalidOperationException(
                $"Cannot seed supervisory assignment: "
                + $"student {studentNumber} "
                + "does not exist.");

        var supervisor =
            _storage.GetAcademicSupervisor(
                staffNumber)
            ?? throw new InvalidOperationException(
                $"Cannot seed supervisory assignment: "
                + $"Academic Supervisor {staffNumber} "
                + "does not exist.");

        var assignmentExists =
            _storage
                .GetAssignmentsForStudent(
                    student.StudentNumber)
                .Any(assignment =>
                    string.Equals(
                        assignment
                            .SupervisorStaffNumber,
                        supervisor.StaffNumber,
                        StringComparison
                            .OrdinalIgnoreCase)
                    && assignment.Role == role
                    && assignment.StartDate
                        == startDate);

        if (assignmentExists)
        {
            return;
        }

        _storage.AddSupervisoryAssignment(
            new SupervisoryAssignment
            {
                StudentNumber =
                    student.StudentNumber,

                SupervisorStaffNumber =
                    supervisor.StaffNumber,

                Role = role,
                StartDate = startDate,
                EndDate = endDate,
                ExceptionAuthorised = false
            });
    }
}
