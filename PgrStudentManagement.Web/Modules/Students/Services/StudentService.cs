using PgrStudentManagement.Web.Modules.Students.Contracts;
using PgrStudentManagement.Web.Modules.Students.Data;
using PgrStudentManagement.Web.Modules.Students.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Models;

namespace PgrStudentManagement.Web.Modules.Students.Services;

public class StudentService : IStudentLookup
{
    private readonly StudentStorage _studentStorage;
    private readonly EnrolmentDao _enrolmentDao;

    public StudentService(
        StudentStorage studentStorage,
        EnrolmentDao enrolmentDao)
    {
        _studentStorage = studentStorage;
        _enrolmentDao = enrolmentDao;
    }

    public Task<IReadOnlyList<Enrolment>> GetEnrolmentsForStudentAsync(string studentNumber)
    {
        return _enrolmentDao.GetEnrolmentsForStudentAsync(studentNumber);
    }

    public IReadOnlyList<Student> GetStudents()
    {
        return _studentStorage
            .GetStudents()
            .OrderBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ThenBy(student => student.StudentNumber)
            .ToList();
    }

    public Student? GetStudent(string studentNumber)
    {
        return _studentStorage.GetStudent(studentNumber);
    }

    public OperationResult<Student> CreateStudent(Student student)
    {
        if (_studentStorage.StudentNumberExists(student.StudentNumber))
        {
            return OperationResult<Student>.Failure(
                "A student with this student number already exists");
        }

        _studentStorage.AddStudent(student);

        return OperationResult<Student>.Success(student);
    }

    public OperationResult UpdateStatus(
        string studentNumber,
        Status status)
    {
        var student = _studentStorage.GetStudent(studentNumber);

        if (student is null)
        {
            return OperationResult.Failure("Student not found");
        }

        student.Status = status;

        _studentStorage.UpdateStudent(student);

        return OperationResult.Success();
    }

    public OperationResult UpdateExpectedSubmissionDate(
        string studentNumber,
        DateTime? expectedSubmissionDate)
    {
        var student = _studentStorage.GetStudent(studentNumber);

        if (student is null)
        {
            return OperationResult.Failure("Student not found");
        }

        if (expectedSubmissionDate is null)
        {
            return OperationResult.Failure("Invalid expected submission date");
        }

        student.ExpectedSubmissionDate = expectedSubmissionDate;

        _studentStorage.UpdateStudent(student);

        return OperationResult.Success();
    }

    public OperationResult RecordSubmission(
        string studentNumber,
        DateTime? actualSubmissionDate)
    {
        var student = _studentStorage.GetStudent(studentNumber);

        if (student is null)
        {
            return OperationResult.Failure("Student not found");
        }

        if (actualSubmissionDate is null)
        {
            return OperationResult.Failure("Invalid submission date");
        }

        student.ActualSubmissionDate = actualSubmissionDate;

        student.Status = Status.Submitted;

        _studentStorage.UpdateStudent(student);

        return OperationResult.Success();
    }

    // Implementation of the IStudentLookup contract
    public StudentSummary? GetStudentSummary(string studentNumber)
    {
        var student = _studentStorage.GetStudent(studentNumber);

        if (student is null)
        {
            return null;
        }

        return new StudentSummary(student.StudentNumber,student.FullName);
    }

}