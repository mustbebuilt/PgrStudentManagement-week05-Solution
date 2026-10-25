using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PgrStudentManagement.Web.Modules.Students.Data;
using PgrStudentManagement.Web.Modules.Students.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Models;

namespace PgrStudentManagement.Tests.Data;

public class EnrolmentDaoTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly StudentManagementDbContext _context;
    private readonly EnrolmentDao _dao;

    public EnrolmentDaoTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<StudentManagementDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new StudentManagementDbContext(options);
        _context.Database.EnsureCreated();

        _dao = new EnrolmentDao(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task SeedAsync_InsertsExpectedStudentsAndEnrolments()
    {
        await StudentDataSeeder.SeedAsync(_context);

        var students = await _context.Students.ToListAsync();
        Assert.Equal(4, students.Count);

        var enrolments = await _context.Enrolments.ToListAsync();
        Assert.Equal(4, enrolments.Count);
    }

    [Fact]
    public async Task SeedAsync_WhenCalledTwice_DoesNotAddDuplicates()
    {
        await StudentDataSeeder.SeedAsync(_context);
        await StudentDataSeeder.SeedAsync(_context);

        var students = await _context.Students.ToListAsync();
        Assert.Equal(4, students.Count);

        var enrolments = await _context.Enrolments.ToListAsync();
        Assert.Equal(4, enrolments.Count);
    }

    [Fact]
    public async Task GetEnrolmentsForStudentAsync_ReturnsChronologicalEnrolmentsForStudent()
    {
        await StudentDataSeeder.SeedAsync(_context);

        var studentTwoEnrolments = await _dao.GetEnrolmentsForStudentAsync("S100002");
        Assert.Equal(2, studentTwoEnrolments.Count);
        Assert.Equal("MPHIL-COMP", studentTwoEnrolments[0].ProgrammeCode);
        Assert.Equal(EnrolmentStatus.Completed, studentTwoEnrolments[0].Status);
        Assert.Equal("PHD-COMP", studentTwoEnrolments[1].ProgrammeCode);
        Assert.Equal(EnrolmentStatus.Active, studentTwoEnrolments[1].Status);
        Assert.True(studentTwoEnrolments[0].StartDate < studentTwoEnrolments[1].StartDate);
    }

    [Fact]
    public async Task GetEnrolmentsForStudentAsync_ReturnsEmptyWhenStudentHasNoEnrolments()
    {
        await StudentDataSeeder.SeedAsync(_context);

        var studentFourEnrolments = await _dao.GetEnrolmentsForStudentAsync("S100004");
        Assert.Empty(studentFourEnrolments);
    }

    [Fact]
    public async Task GetEnrolmentsForStudentAsync_EnrolmentWithNullExpectedEndDateHandledCorrectly()
    {
        await StudentDataSeeder.SeedAsync(_context);

        var studentThreeEnrolments = await _dao.GetEnrolmentsForStudentAsync("S100003");
        var enrolment = Assert.Single(studentThreeEnrolments);
        Assert.Null(enrolment.ExpectedEndDate);
        Assert.Equal(EnrolmentStatus.Suspended, enrolment.Status);
    }
}
