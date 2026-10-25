using PgrStudentManagement.Tests.Fixtures;
using PgrStudentManagement.Web.Modules
    .AcademicSupervisors.Data;
using PgrStudentManagement.Web.Modules
    .AcademicSupervisors.Models;

namespace PgrStudentManagement.Tests.Storage;

public class AcademicSupervisorStorageTests
{
    [Fact]
    public void GetAssignmentsForStudent_ReturnsMatchingAssignments()
    {
        var storage = CreateStorage();

        storage.AddSupervisoryAssignment(
            AcademicSupervisorTestData.CreateAssignment(
                "S100001",
                "SUP001",
                SupervisoryRole.DirectorOfStudies));

        storage.AddSupervisoryAssignment(
            AcademicSupervisorTestData.CreateAssignment(
                "S100001",
                "SUP002",
                SupervisoryRole.FirstSupervisor));

        storage.AddSupervisoryAssignment(
            AcademicSupervisorTestData.CreateAssignment(
                "S100002",
                "SUP001",
                SupervisoryRole.FirstSupervisor));

        var result =
            storage.GetAssignmentsForStudent(
                "S100001");

        Assert.Equal(2, result.Count);

        Assert.All(
            result,
            assignment =>
                Assert.Equal(
                    "S100001",
                    assignment.StudentNumber));
    }

    [Fact]
    public void GetAssignmentsForStudent_WhenNoneExist_ReturnsEmptyList()
    {
        var storage = CreateStorage();

        var result =
            storage.GetAssignmentsForStudent(
                "S100003");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAssignmentsForSupervisor_ReturnsMatchingAssignments()
    {
        var storage = CreateStorage();

        storage.AddSupervisoryAssignment(
            AcademicSupervisorTestData.CreateAssignment(
                "S100001",
                "SUP001",
                SupervisoryRole.DirectorOfStudies));

        storage.AddSupervisoryAssignment(
            AcademicSupervisorTestData.CreateAssignment(
                "S100002",
                "SUP001",
                SupervisoryRole.FirstSupervisor));

        storage.AddSupervisoryAssignment(
            AcademicSupervisorTestData.CreateAssignment(
                "S100001",
                "SUP002",
                SupervisoryRole.FirstSupervisor));

        var result =
            storage.GetAssignmentsForSupervisor(
                "SUP001");

        Assert.Equal(2, result.Count);

        Assert.All(
            result,
            assignment =>
                Assert.Equal(
                    "SUP001",
                    assignment.SupervisorStaffNumber));
    }

    [Fact]
    public void GetAssignmentsForSupervisor_WhenNoneExist_ReturnsEmptyList()
    {
        var storage = CreateStorage();

        var result =
            storage.GetAssignmentsForSupervisor(
                "SUP004");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    private static AcademicSupervisorStorage
        CreateStorage()
    {
        return new AcademicSupervisorStorage();
    }
}