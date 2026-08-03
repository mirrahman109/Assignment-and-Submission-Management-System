using AssignmentSystem.Api.DTOs.Assignments;
using AssignmentSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/assignments")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;
    private readonly ICurrentUserService _currentUser;

    public AssignmentsController(IAssignmentService assignmentService, ICurrentUserService currentUser)
    {
        _assignmentService = assignmentService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<AssignmentResponse>>> List() => Ok(await _assignmentService.ListAsync(_currentUser));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssignmentResponse>> GetById(int id) =>
        Ok(await _assignmentService.GetByIdAsync(id, _currentUser));

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<AssignmentResponse>> Create(CreateAssignmentRequest request)
    {
        var result = await _assignmentService.CreateAsync(request, _currentUser);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<AssignmentResponse>> Update(int id, UpdateAssignmentRequest request) =>
        Ok(await _assignmentService.UpdateAsync(id, request, _currentUser));

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<AssignmentResponse>> UpdateStatus(int id, UpdateAssignmentStatusRequest request) =>
        Ok(await _assignmentService.UpdateStatusAsync(id, request.Status, _currentUser));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        await _assignmentService.DeleteAsync(id, _currentUser);
        return NoContent();
    }
}
