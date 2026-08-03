using AssignmentSystem.Api.DTOs.TeacherAssignments;
using AssignmentSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/teacher-assignments")]
[Authorize]
public class TeacherAssignmentsController : ControllerBase
{
    private readonly ITeacherAssignmentService _teacherAssignmentService;
    private readonly ICurrentUserService _currentUser;

    public TeacherAssignmentsController(ITeacherAssignmentService teacherAssignmentService, ICurrentUserService currentUser)
    {
        _teacherAssignmentService = teacherAssignmentService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<List<TeacherAssignmentResponse>>> List() =>
        Ok(await _teacherAssignmentService.ListAsync(_currentUser));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TeacherAssignmentResponse>> Create(CreateTeacherAssignmentRequest request) =>
        Ok(await _teacherAssignmentService.CreateAsync(request));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _teacherAssignmentService.DeleteAsync(id);
        return NoContent();
    }
}
