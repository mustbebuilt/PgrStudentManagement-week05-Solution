using PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;

namespace PgrStudentManagement.Web.Modules.AcademicSupervisors.Data;

public class AcademicSupervisorStorage 
{
    private readonly List<AcademicSupervisor> _academicSupervisors = [];

    private readonly List<SupervisoryAssignment> _supervisoryAssignments = [];

    public IReadOnlyList<AcademicSupervisor> GetAcademicSupervisors()
    {
        return _academicSupervisors.ToList();
    }

    public AcademicSupervisor?
        GetAcademicSupervisor(string staffNumber)
    {
        return _academicSupervisors
            .FirstOrDefault(supervisor =>
                string.Equals(
                    supervisor.StaffNumber,
                    staffNumber,
                    StringComparison.OrdinalIgnoreCase));
    }

    public void AddAcademicSupervisor(AcademicSupervisor supervisor)
    {
        _academicSupervisors.Add(supervisor);
    }

    public bool StaffNumberExists(string staffNumber)
    {
        return _academicSupervisors.Any(
            academic => academic.StaffNumber == staffNumber
        );
    }

    public IReadOnlyList<SupervisoryAssignment>
        GetAssignmentsForStudent(string studentNumber)
    {
        return _supervisoryAssignments
            .Where(assignment =>
                string.Equals(
                    assignment.StudentNumber,
                    studentNumber,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public IReadOnlyList<SupervisoryAssignment>
        GetAssignmentsForSupervisor(string staffNumber)
    {
        return _supervisoryAssignments
            .Where(assignment =>
                string.Equals(
                    assignment.SupervisorStaffNumber,
                    staffNumber,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public void AddSupervisoryAssignment(SupervisoryAssignment assignment)
    {
        _supervisoryAssignments.Add(assignment);
    }

    public void UpdateSupervisoryAssignment(
        SupervisoryAssignment assignment)
    {
        var index = _supervisoryAssignments.FindIndex(
            existing =>
                existing.StudentNumber
                    == assignment.StudentNumber
                && existing.SupervisorStaffNumber
                    == assignment.SupervisorStaffNumber
                && existing.Role == assignment.Role
                && existing.StartDate
                    == assignment.StartDate);

        if (index >= 0)
        {
            _supervisoryAssignments[index] =
                assignment;
        }
    }
}