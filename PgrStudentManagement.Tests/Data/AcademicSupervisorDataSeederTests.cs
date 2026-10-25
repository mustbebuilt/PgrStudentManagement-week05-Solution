using PgrStudentManagement.Tests
    .TestDoubles;
using PgrStudentManagement.Web.Modules
    .AcademicSupervisors.Data;

namespace PgrStudentManagement.Tests.Data;

public class AcademicSupervisorDataSeederTests
{
    [Fact]
    public void Seed_AddsExpectedSupervisorsAndAssignments()
    {
        var storage =
            new AcademicSupervisorStorage();

        var studentLookup =
            CreateStudentLookup();

        var seeder =
            new AcademicSupervisorDataSeeder(
                storage,
                studentLookup);

        seeder.Seed();

        Assert.Equal(
            4,
            storage.GetAcademicSupervisors()
                .Count);

        Assert.Equal(
            3,
            storage.GetAssignmentsForStudent(
                "S100001").Count);

        Assert.Single(
            storage.GetAssignmentsForStudent(
                "S100002"));

        Assert.Empty(
            storage.GetAssignmentsForStudent(
                "S100003"));

        Assert.Equal(
            2,
            storage.GetAssignmentsForStudent(
                "S100004").Count);

        Assert.Empty(
            storage.GetAssignmentsForSupervisor(
                "SUP004"));
    }

    [Fact]
    public void Seed_WhenCalledTwice_DoesNotAddDuplicates()
    {
        var storage =
            new AcademicSupervisorStorage();

        var seeder =
            new AcademicSupervisorDataSeeder(
                storage,
                CreateStudentLookup());

        seeder.Seed();
        seeder.Seed();

        Assert.Equal(
            4,
            storage.GetAcademicSupervisors()
                .Count);

        Assert.Equal(
            6,
            storage.GetAssignmentsForStudent(
                "S100001").Count
            + storage.GetAssignmentsForStudent(
                "S100002").Count
            + storage.GetAssignmentsForStudent(
                "S100003").Count
            + storage.GetAssignmentsForStudent(
                "S100004").Count);
    }

    [Fact]
    public void Seed_WhenReferencedStudentIsMissing_ThrowsException()
    {
        var storage = new AcademicSupervisorStorage();

        var emptyStudentLookup = new FakeStudentLookup();

        var seeder = new AcademicSupervisorDataSeeder(
                storage,
                emptyStudentLookup);

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => seeder.Seed());

        Assert.Contains(
            "student S100001 does not exist",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    private static FakeStudentLookup
        CreateStudentLookup()
    {
        var lookup =
            new FakeStudentLookup();

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
}