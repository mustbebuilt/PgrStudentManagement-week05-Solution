using PgrStudentManagement.Web.Modules.Students.Contracts;

namespace PgrStudentManagement.Tests.TestDoubles;

public class FakeStudentLookup : IStudentLookup
{
    private readonly Dictionary<string, StudentSummary>
        _students =
            new(StringComparer.OrdinalIgnoreCase);

    public void AddStudent(
        string studentNumber,
        string displayName)
    {
        _students[studentNumber] =
            new StudentSummary(
                studentNumber,
                displayName);
    }

    public StudentSummary? GetStudentSummary(
        string studentNumber)
    {
        return _students.TryGetValue(
            studentNumber,
            out var student)
                ? student
                : null;
    }
}