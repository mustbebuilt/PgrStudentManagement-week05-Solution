namespace PgrStudentManagement.Web.Modules.Students.Contracts;

public interface IStudentLookup
{
    StudentSummary? GetStudentSummary(string studentNumber);
}
