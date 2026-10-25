using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Services;

namespace PgrStudentManagement.Web.Modules.AcademicSupervisors.Controllers;

public class AcademicSupervisorsController : Controller
{
    private readonly AcademicSupervisorService _academicSupervisorService;

    public AcademicSupervisorsController(
        AcademicSupervisorService academicSupervisorService
    )
    {
        _academicSupervisorService = academicSupervisorService;
    }

    public IActionResult Index()
    {
        var supervisors =
            _academicSupervisorService
                .GetAcademicSupervisors();

        return View(supervisors);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateAcademicSupervisorViewModel());
    }

    [HttpPost]
    public IActionResult Create(CreateAcademicSupervisorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var supervisor = new AcademicSupervisor
        {
            StaffNumber = model.StaffNumber.Trim(),
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            EmailAddress = model.EmailAddress.Trim(),
            EmploymentType =
                model.EmploymentType!.Value,
            TrainingCompleted =
                model.TrainingCompleted,
            EligibleRoles =
                model.EligibleRoles.ToHashSet()
        };

        var outcome =
            _academicSupervisorService
                .CreateAcademicSupervisor(
                    supervisor);

        switch (outcome)
        {
            case SupervisorOutcome.Created:
                TempData["SuccessMessage"] =
                    "Academic Supervisor created successfully.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        staffNumber =
                            supervisor.StaffNumber
                    });

            case SupervisorOutcome.DuplicateStaffNumber:
                ModelState.AddModelError(
                    nameof(model.StaffNumber),
                    "An Academic Supervisor with this "
                    + "staff number already exists.");

                return View(model);

            case SupervisorOutcome
                .MissingRequiredInformation:
                ModelState.AddModelError(
                    string.Empty,
                    "The Academic Supervisor could not "
                    + "be created because required "
                    + "information is missing.");

                return View(model);

            default:
                ModelState.AddModelError(
                    string.Empty,
                    "The Academic Supervisor could not "
                    + "be created.");

                return View(model);
        }
    }

    [HttpGet]
    public IActionResult Details(string staffNumber)
    {
        if (string.IsNullOrWhiteSpace(staffNumber))
        {
            return View("NotFound");
        }

        var supervisor =
            _academicSupervisorService
                .GetAcademicSupervisor(
                    staffNumber);

        if (supervisor is null)
        {
            return View("NotFound");
        }

        return View(supervisor);
    }

    [HttpGet]
    public IActionResult Assign(string studentNumber)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            ModelState.AddModelError(
                string.Empty,
                "A student number is required.");
        }

        var result =
            _academicSupervisorService
                .PrepareAssignment(
                    studentNumber);

        return View(result);
    }

    [HttpPost]
    public IActionResult Assign(AssignmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            var invalidForm =
                _academicSupervisorService
                    .PrepareAssignment(
                        request.StudentNumber);

            PreserveAssignmentValues(request);

            return View(invalidForm);
        }

        var outcome =
            _academicSupervisorService
                .AssignSupervisor(request);

        if (outcome == AssignmentOutcome.Created)
        {
            TempData["SuccessMessage"] =
                "Academic Supervisor assigned successfully.";

            return RedirectToAction(
                nameof(Team),
                new
                {
                    studentNumber =
                        request.StudentNumber
                });
        }

        AddAssignmentError(outcome);

        var form =
            _academicSupervisorService
                .PrepareAssignment(
                    request.StudentNumber);

        PreserveAssignmentValues(request);

        return View(form);
    }

    [HttpGet]
    public IActionResult Team(string studentNumber)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            return View(
                new SupervisoryTeamResult());
        }

        var result =
            _academicSupervisorService
                .GetSupervisoryTeam(
                    studentNumber);

        return View(result);
    }

    private void AddAssignmentError(AssignmentOutcome outcome)
    {
        var message = outcome switch
        {
            AssignmentOutcome.StudentNotFound =>
                "Student not found.",

            AssignmentOutcome.SupervisorNotFound =>
                "Academic Supervisor not found.",

            AssignmentOutcome.DuplicateAssignment =>
                "This current supervisory assignment "
                + "already exists.",

            AssignmentOutcome.RoleAlreadyFilled =>
                "The selected supervisory role is "
                + "already filled for this student.",

            AssignmentOutcome.SupervisorNotEligible =>
                "The selected Academic Supervisor is "
                + "not eligible for this role.",

            AssignmentOutcome
                .EmploymentTypeNotPermitted =>
                "The supervisor's employment type is "
                + "not permitted for the selected role.",

            AssignmentOutcome.ExceptionRequired =>
                "An authorised exception is required "
                + "before a fourth supervisor can be "
                + "assigned.",

            _ =>
                "The supervisory assignment could "
                + "not be created."
        };

        ModelState.AddModelError(
            string.Empty,
            message);
    }

    private void PreserveAssignmentValues(AssignmentRequest request)
    {
        ViewData["SelectedStaffNumber"] =
            request.StaffNumber;

        ViewData["SelectedRole"] =
            request.Role;

        ViewData["SelectedStartDate"] =
            request.StartDate.ToString(
                "yyyy-MM-dd");

        ViewData["ExceptionAuthorised"] =
            request.ExceptionAuthorised;
    }
}