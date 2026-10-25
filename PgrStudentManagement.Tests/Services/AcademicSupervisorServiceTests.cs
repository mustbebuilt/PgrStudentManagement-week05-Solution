using PgrStudentManagement.Tests.Fixtures;
using PgrStudentManagement.Tests.TestDoubles;
using PgrStudentManagement.Web.Modules
    .AcademicSupervisors.Data;
using PgrStudentManagement.Web.Modules
    .AcademicSupervisors.Models;
using PgrStudentManagement.Web.Modules
    .AcademicSupervisors.Services;

namespace PgrStudentManagement.Tests.Services;

public class AcademicSupervisorServiceTests
{
    [Fact]
    public void AssignSupervisor_WhenStudentDoesNotExist_ReturnsStudentNotFound()
    {
        var storage = CreateStorage();

        storage.AddAcademicSupervisor(
            AcademicSupervisorTestData
                .CreateInstitutionSupervisor());

        var service =
            new AcademicSupervisorService(
                storage,
                CreateStudentLookup());

        var request = CreateRequest(
            "S999999",
            "SUP001",
            SupervisoryRole.DirectorOfStudies);

        var outcome =
            service.AssignSupervisor(request);

        Assert.Equal(
            AssignmentOutcome.StudentNotFound,
            outcome);

        Assert.Empty(
            storage.GetAssignmentsForStudent(
                "S999999"));
    }

    [Fact]
    public void AssignSupervisor_WhenSupervisorDoesNotExist_ReturnsSupervisorNotFound()
    {
        var storage = CreateStorage();

        var service =
            new AcademicSupervisorService(
                storage,
                CreateStudentLookup());

        var request = CreateRequest(
            "S100001",
            "SUP999",
            SupervisoryRole.DirectorOfStudies);

        var outcome =
            service.AssignSupervisor(request);

        Assert.Equal(
            AssignmentOutcome.SupervisorNotFound,
            outcome);

        Assert.Empty(
            storage.GetAssignmentsForStudent(
                "S100001"));
    }

    [Fact]
    public void AssignSupervisor_WhenRequestIsValid_CreatesAssignment()
    {
        var storage = CreateStorage();

        storage.AddAcademicSupervisor(
            AcademicSupervisorTestData
                .CreateInstitutionSupervisor());

        var service =
            new AcademicSupervisorService(
                storage,
                CreateStudentLookup());

        var request = CreateRequest(
            "S100001",
            "SUP001",
            SupervisoryRole.DirectorOfStudies);

        var outcome =
            service.AssignSupervisor(request);

        Assert.Equal(
            AssignmentOutcome.Created,
            outcome);

        var assignment = Assert.Single(
            storage.GetAssignmentsForStudent(
                "S100001"));

        Assert.Equal(
            "SUP001",
            assignment.SupervisorStaffNumber);

        Assert.Equal(
            SupervisoryRole.DirectorOfStudies,
            assignment.Role);

        Assert.True(assignment.Current);
    }

    [Fact]
    public void AssignSupervisor_WhenCurrentAssignmentExists_ReturnsDuplicateAssignment()
    {
        var storage = CreateStorage();

        storage.AddAcademicSupervisor(
            AcademicSupervisorTestData
                .CreateInstitutionSupervisor());

        storage.AddSupervisoryAssignment(
            AcademicSupervisorTestData
                .CreateAssignment(
                    "S100001",
                    "SUP001",
                    SupervisoryRole
                        .DirectorOfStudies));

        var service =
            new AcademicSupervisorService(
                storage,
                CreateStudentLookup());

        var request = CreateRequest(
            "S100001",
            "SUP001",
            SupervisoryRole.DirectorOfStudies);

        var outcome =
            service.AssignSupervisor(request);

        Assert.Equal(
            AssignmentOutcome.DuplicateAssignment,
            outcome);

        Assert.Single(
            storage.GetAssignmentsForStudent(
                "S100001"));
    }

    private static AcademicSupervisorStorage
        CreateStorage()
    {
        return new AcademicSupervisorStorage();
    }

    private static FakeStudentLookup
        CreateStudentLookup()
    {
        var lookup = new FakeStudentLookup();

        lookup.AddStudent(
            "S100001",
            "John Doe");

        lookup.AddStudent(
            "S100002",
            "Jane Smith");

        lookup.AddStudent(
            "S100003",
            "Casey Wilson");

        lookup.AddStudent(
            "S100004",
            "Morgan Lee");

        return lookup;
    }

    private static AssignmentRequest CreateRequest(
        string studentNumber,
        string staffNumber,
        SupervisoryRole role,
        bool exceptionAuthorised = false)
    {
        return new AssignmentRequest
        {
            StudentNumber = studentNumber,
            StaffNumber = staffNumber,
            Role = role,
            StartDate =
                new DateTime(2026, 1, 15),
            ExceptionAuthorised =
                exceptionAuthorised
        };
    }
}