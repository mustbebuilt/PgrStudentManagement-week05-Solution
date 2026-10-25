using PgrStudentManagement.Web.Modules.Students.Data;

namespace PgrStudentManagement.Tests.Data;

public class StudentDataSeederTests
{
    [Fact]
    public void Seed_AddsFourStudents()
    {
        var storage = new StudentStorage();
        var seeder = new StudentDataSeeder(storage);

        seeder.Seed();

        var students = storage.GetStudents();

        Assert.Equal(4, students.Count);

        Assert.NotNull(
            storage.GetStudent("S100001"));

        Assert.NotNull(
            storage.GetStudent("S100002"));

        Assert.NotNull(
            storage.GetStudent("S100003"));

        Assert.NotNull(
            storage.GetStudent("S100004"));
    }

    [Fact]
    public void Seed_WhenCalledTwice_DoesNotAddDuplicates()
    {
        var storage = new StudentStorage();
        var seeder = new StudentDataSeeder(storage);

        seeder.Seed();
        seeder.Seed();

        var students = storage.GetStudents();

        Assert.Equal(4, students.Count);
    }

    [Fact]
    public void Seed_CreatesExpectedStudentNames()
    {
        var storage = new StudentStorage();
        var seeder = new StudentDataSeeder(storage);

        seeder.Seed();

        var john = storage.GetStudent("S100001");
        var jane = storage.GetStudent("S100002");
        var casey = storage.GetStudent("S100003");
        var morgan = storage.GetStudent("S100004");

        Assert.NotNull(john);
        Assert.NotNull(jane);
        Assert.NotNull(casey);
        Assert.NotNull(morgan);

        Assert.Equal("John", john.FirstName);
        Assert.Equal("Doe", john.LastName);

        Assert.Equal("Jane", jane.FirstName);
        Assert.Equal("Smith", jane.LastName);

        Assert.Equal("Casey", casey.FirstName);
        Assert.Equal("Wilson", casey.LastName);

        Assert.Equal("Morgan", morgan.FirstName);
        Assert.Equal("Lee", morgan.LastName);
    }
}