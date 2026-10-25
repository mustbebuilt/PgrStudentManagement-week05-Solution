using PgrStudentManagement.Web.Modules
    .AcademicSupervisors.Models;

namespace PgrStudentManagement.Tests.Fixtures;

public static class AcademicSupervisorTestData
{
    public static AcademicSupervisor
        CreateInstitutionSupervisor(
            string staffNumber = "SUP001")
    {
        return new AcademicSupervisor
        {
            StaffNumber = staffNumber,
            FirstName = "Robin",
            LastName = "Carter",
            EmailAddress =
                $"{staffNumber.ToLowerInvariant()}@example.test",
            EmploymentType =
                SupervisorEmploymentType
                    .InstitutionEmployed,
            TrainingCompleted = true,
            EligibleRoles =
            [
                SupervisoryRole.DirectorOfStudies,
                SupervisoryRole.FirstSupervisor,
                SupervisoryRole.SecondSupervisor
            ]
        };
    }

    public static AcademicSupervisor
        CreateExternalSupervisor(
            string staffNumber = "SUP003")
    {
        return new AcademicSupervisor
        {
            StaffNumber = staffNumber,
            FirstName = "Taylor",
            LastName = "Green",
            EmailAddress =
                $"{staffNumber.ToLowerInvariant()}@example.test",
            EmploymentType =
                SupervisorEmploymentType.External,
            TrainingCompleted = true,
            EligibleRoles =
            [
                SupervisoryRole.SecondSupervisor,
                SupervisoryRole.Adviser
            ]
        };
    }

    public static SupervisoryAssignment
        CreateAssignment(
            string studentNumber,
            string staffNumber,
            SupervisoryRole role,
            DateTime? endDate = null)
    {
        return new SupervisoryAssignment
        {
            StudentNumber = studentNumber,
            SupervisorStaffNumber = staffNumber,
            Role = role,
            StartDate = new DateTime(2025, 10, 1),
            EndDate = endDate,
            ExceptionAuthorised = false
        };
    }
}