using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Web.Modules.Students.Models;
using PgrStudentManagement.Web.Modules.Students.Services;

namespace PgrStudentManagement.Web.Modules.Students.Controllers;

public class StudentsController : Controller
{
    private readonly StudentService _studentService;

    public StudentsController(StudentService studentService)
    {
        _studentService = studentService;
    }

    public IActionResult Index()
    {
        var students = _studentService.GetStudents();

        return View(students);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Student());
    }

    [HttpPost]
    public IActionResult Create(Student student)
    {
        if (!ModelState.IsValid)
        {
            return View(student);
        }

        var result = _studentService.CreateStudent(student);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                nameof(Student.StudentNumber),
                result.ErrorMessage!);

            return View(student);
        }

        return RedirectToAction(
            nameof(Details),
            new { studentNumber = student.StudentNumber });
    }

    public IActionResult Details(string studentNumber)
    {
        var student = _studentService.GetStudent(studentNumber);

        if (student is null)
        {
            return View("StudentNotFound");
        }

        return View(student);
    }

    public async Task<IActionResult> Enrolments(
        string studentNumber)
    {
        var enrolments = await _studentService
            .GetEnrolmentsForStudentAsync(studentNumber);

        ViewData["StudentNumber"] = studentNumber;

        if (enrolments.Count == 0)
        {
            ViewData["Message"] = "No enrolments found";
        }

        return View(enrolments);
    }

    public IActionResult EnrolmentDetails(string studentNumber)
    {
        var student = _studentService.GetStudent(studentNumber);

        if (student is null)
        {
            return View("StudentNotFound");
        }

        return View(student);
    }

    public IActionResult ThesisDetails(string studentNumber)
    {
        var student = _studentService.GetStudent(studentNumber);

        if (student is null)
        {
            return View("StudentNotFound");
        }

        if (string.IsNullOrWhiteSpace(student.ThesisTitle))
        {
            ViewData["Message"] =
                "Thesis information not available";
        }

        return View(student);
    }

    [HttpGet]
    public IActionResult EditStatus(string studentNumber)
    {
        var student = _studentService.GetStudent(studentNumber);

        if (student is null)
        {
            return View("StudentNotFound");
        }

        return View(student);
    }

    [HttpPost]
    public IActionResult EditStatus(
        string studentNumber,
        Status status)
    {
        var result = _studentService.UpdateStatus(
            studentNumber,
            status);

        if (!result.Succeeded)
        {
            return View("StudentNotFound");
        }

        return RedirectToAction(
            nameof(Details),
            new { studentNumber });
    }

    [HttpGet]
    public IActionResult EditExpectedSubmissionDate(
        string studentNumber)
    {
        var student = _studentService.GetStudent(studentNumber);

        if (student is null)
        {
            return View("StudentNotFound");
        }

        return View(student);
    }

    [HttpPost]
    public IActionResult EditExpectedSubmissionDate(
        string studentNumber,
        DateTime? expectedSubmissionDate)
    {
        var result =
            _studentService.UpdateExpectedSubmissionDate(
                studentNumber,
                expectedSubmissionDate);

        if (!result.Succeeded)
        {
            var student =
                _studentService.GetStudent(studentNumber);

            if (student is null)
            {
                return View("StudentNotFound");
            }

            ModelState.AddModelError(
                nameof(Student.ExpectedSubmissionDate),
                result.ErrorMessage!);

            return View(student);
        }

        return RedirectToAction(
            nameof(ThesisDetails),
            new { studentNumber });
    }

    [HttpGet]
    public IActionResult RecordSubmission(
        string studentNumber)
    {
        var student = _studentService.GetStudent(studentNumber);

        if (student is null)
        {
            return View("StudentNotFound");
        }

        return View(student);
    }

    [HttpPost]
    public IActionResult RecordSubmission(
        string studentNumber,
        DateTime? actualSubmissionDate)
    {
        var result = _studentService.RecordSubmission(
            studentNumber,
            actualSubmissionDate);

        if (!result.Succeeded)
        {
            var student =
                _studentService.GetStudent(studentNumber);

            if (student is null)
            {
                return View("StudentNotFound");
            }

            ModelState.AddModelError(
                nameof(Student.ActualSubmissionDate),
                result.ErrorMessage!);

            return View(student);
        }

        return RedirectToAction(
            nameof(ThesisDetails),
            new { studentNumber });
    }
}