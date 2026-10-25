using PgrStudentManagement.Web.Modules.Students.Models;

namespace PgrStudentManagement.Web.Modules.Students.Data;

public class StudentStorage
{
    private readonly List<Student> _students = [];

    public IReadOnlyList<Student> GetStudents()
    {
        return _students.AsReadOnly();
    }

    public Student? GetStudent(string studentNumber)
    {
        return _students.FirstOrDefault(
            student => student.StudentNumber == studentNumber);
    }

    public bool StudentNumberExists(string studentNumber)
    {
        return _students.Any(
            student => student.StudentNumber == studentNumber);
    }

    public void AddStudent(Student student)
    {
        _students.Add(student);
    }

    public void UpdateStudent(Student student)
    {
        var index = _students.FindIndex(
            existing =>
                existing.StudentNumber == student.StudentNumber);

        if (index >= 0)
        {
            _students[index] = student;
        }
    }
}