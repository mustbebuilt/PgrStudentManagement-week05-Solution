using PgrStudentManagement.Tests.TestDoubles;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Data;
using PgrStudentManagement.Web.Modules.Students.Data;

namespace PgrStudentManagement.Tests.Data;

public class ApplicationSeedOrderTests
{
    [Fact]
    public void StudentsMustExistBeforeSupervisorAssignmentsAreSeeded()
    {
        var studentStorage = new StudentStorage();

        var studentSeeder =
            new StudentDataSeeder(
                studentStorage);

        studentSeeder.Seed();

        var studentLookup = new FakeStudentLookup();

        foreach (var student in studentStorage.GetStudents())
        {
            studentLookup.AddStudent(
                student.StudentNumber,
                $"{student.FirstName} "
                + student.LastName);
        }

        var supervisorStorage = new AcademicSupervisorStorage();

        var supervisorSeeder =
            new AcademicSupervisorDataSeeder(
                supervisorStorage,
                studentLookup);

        supervisorSeeder.Seed();

        Assert.Equal(
            4,
            studentStorage.GetStudents().Count);

        Assert.Equal(
            4,
            supervisorStorage
                .GetAcademicSupervisors()
                .Count);

        Assert.Equal(
            3,
            supervisorStorage
                .GetAssignmentsForStudent("S100001")
                .Count);

        Assert.Empty(
            supervisorStorage
                .GetAssignmentsForStudent("S100003"));
    }
}