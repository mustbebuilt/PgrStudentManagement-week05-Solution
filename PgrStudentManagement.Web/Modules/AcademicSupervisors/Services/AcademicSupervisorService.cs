using PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Data;

using PgrStudentManagement.Web.Modules.Students.Contracts;

namespace PgrStudentManagement.Web.Modules.AcademicSupervisors.Services;

public class AcademicSupervisorService
{
    private readonly AcademicSupervisorStorage _storage;
    private IStudentLookup _studentLookup;

    public AcademicSupervisorService(
        AcademicSupervisorStorage storage,
        IStudentLookup studentLookup)
    {
        _storage = storage;
        _studentLookup = studentLookup;
    }

    public IReadOnlyList<AcademicSupervisor> GetAcademicSupervisors()
    {
        return _storage
            .GetAcademicSupervisors()
            .OrderBy(supervisor => supervisor.LastName)
            .ThenBy(supervisor => supervisor.FirstName)
            .ToList();
    }

    public AcademicSupervisor?
        GetAcademicSupervisor(string staffNumber)
    {
        return _storage.GetAcademicSupervisor(staffNumber);
    }

    public SupervisorOutcome CreateAcademicSupervisor(
        AcademicSupervisor supervisor)
    {
        if (_storage.StaffNumberExists(supervisor.StaffNumber))
        {
            return SupervisorOutcome.DuplicateStaffNumber;
        }

        _storage.AddAcademicSupervisor(supervisor);

        return SupervisorOutcome.Created;
    }

    public AssignmentFormResult PrepareAssignment(
        string studentNumber)
    {
        var student =
            _studentLookup.GetStudentSummary(studentNumber);

        var supervisors = _storage
            .GetAcademicSupervisors()
            .OrderBy(supervisor => supervisor.LastName)
            .ThenBy(supervisor => supervisor.FirstName)
            .ThenBy(supervisor => supervisor.StaffNumber)
            .ToList();

        var availableRoles = Enum
            .GetValues<SupervisoryRole>()
            .ToList();

        return new AssignmentFormResult
        {
            Student = student,
            Supervisors = supervisors,
            AvailableRoles = availableRoles
        };
    }

    public AssignmentOutcome AssignSupervisor(
        AssignmentRequest request)
    {
        var student =
            _studentLookup.GetStudentSummary(
                request.StudentNumber);

        if (student is null)
        {
            return AssignmentOutcome.StudentNotFound;
        }

        var supervisor =
            _storage.GetAcademicSupervisor(
                request.StaffNumber);

        if (supervisor is null)
        {
            return AssignmentOutcome.SupervisorNotFound;
        }

        var currentAssignments = _storage
            .GetAssignmentsForStudent(
                request.StudentNumber)
            .Where(assignment => assignment.Current)
            .ToList();

        var duplicateAssignment =
            currentAssignments.Any(assignment =>
                assignment.SupervisorStaffNumber
                    == request.StaffNumber
                && assignment.Role == request.Role);

        if (duplicateAssignment)
        {
            return AssignmentOutcome.DuplicateAssignment;
        }

        var roleMustBeUnique =
            request.Role
                == SupervisoryRole.DirectorOfStudies
            || request.Role
                == SupervisoryRole.FirstSupervisor;

        if (roleMustBeUnique)
        {
            var roleAlreadyFilled =
                currentAssignments.Any(assignment =>
                    assignment.Role == request.Role);

            if (roleAlreadyFilled)
            {
                return AssignmentOutcome.RoleAlreadyFilled;
            }
        }

        var supervisorIsEligible =
            supervisor.EligibleRoles.Contains(
                request.Role);

        if (!supervisorIsEligible)
        {
            return AssignmentOutcome.SupervisorNotEligible;
        }

        var requiresInstitutionEmployee =
            request.Role
                == SupervisoryRole.DirectorOfStudies
            || request.Role
                == SupervisoryRole.FirstSupervisor;

        var hasPermittedEmploymentType =
            supervisor.EmploymentType
                == SupervisorEmploymentType
                    .InstitutionEmployed;

        if (requiresInstitutionEmployee
            && !hasPermittedEmploymentType)
        {
            return AssignmentOutcome
                .EmploymentTypeNotPermitted;
        }
        
        var currentSupervisorCount =
            currentAssignments.Count(assignment =>
                assignment.Role != SupervisoryRole.Adviser);

        var addingFourthSupervisor = currentSupervisorCount >= 3;

        if (addingFourthSupervisor
            && !request.ExceptionAuthorised)
        {
            return AssignmentOutcome.ExceptionRequired;
        }

        var assignment = new SupervisoryAssignment
        {
            StudentNumber = request.StudentNumber,
            SupervisorStaffNumber =
                request.StaffNumber,
            Role = request.Role,
            StartDate = request.StartDate,
            EndDate = null,
            ExceptionAuthorised =
                request.ExceptionAuthorised
        };

        _storage.AddSupervisoryAssignment(
            assignment);

        return AssignmentOutcome.Created;
    }

    public SupervisoryTeamResult GetSupervisoryTeam(
        string studentNumber)
    {
        var student =
            _studentLookup.GetStudentSummary(
                studentNumber);

        if (student is null)
        {
            return new SupervisoryTeamResult();
        }

        var assignments = _storage
            .GetAssignmentsForStudent(studentNumber);

        var resolvedAssignments = assignments
            .Select(CreateAssignmentViewModel)
            .Where(viewModel => viewModel is not null)
            .Cast<SupervisoryAssignmentViewModel>()
            .ToList();

        var currentAssignments = resolvedAssignments
            .Where(assignment => assignment.Current)
            .OrderBy(assignment => assignment.Role)
            .ThenBy(assignment =>
                assignment.SupervisorName)
            .ToList();

        var historicalAssignments =
            resolvedAssignments
                .Where(assignment =>
                    !assignment.Current)
                .OrderByDescending(assignment =>
                    assignment.EndDate)
                .ThenBy(assignment =>
                    assignment.Role)
                .ToList();

        return new SupervisoryTeamResult
        {
            Student = student,
            CurrentAssignments = currentAssignments,
            HistoricalAssignments =
                historicalAssignments
        };
    }

    private SupervisoryAssignmentViewModel?
        CreateAssignmentViewModel(
            SupervisoryAssignment assignment)
    {
        var supervisor =
            _storage.GetAcademicSupervisor(
                assignment.SupervisorStaffNumber);

        if (supervisor is null)
        {
            return null;
        }

        return new SupervisoryAssignmentViewModel
        {
            SupervisorStaffNumber =
                supervisor.StaffNumber,
            SupervisorName =
                $"{supervisor.FirstName} "
                + supervisor.LastName,
            EmploymentType =
                supervisor.EmploymentType,
            Role = assignment.Role,
            StartDate = assignment.StartDate,
            EndDate = assignment.EndDate
        };
    }
}