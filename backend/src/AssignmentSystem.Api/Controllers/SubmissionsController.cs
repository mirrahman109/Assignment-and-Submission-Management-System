using AssignmentSystem.Api.DTOs.Submissions;
using AssignmentSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionService _submissionService;
    private readonly ICurrentUserService _currentUser;

    public SubmissionsController(ISubmissionService submissionService, ICurrentUserService currentUser)
    {
        _submissionService = submissionService;
        _currentUser = currentUser;
    }

    [HttpPost("api/assignments/{assignmentId:int}/submissions")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<SubmissionResponse>> Create(int assignmentId, CreateSubmissionRequest request)
    {
        var result = await _submissionService.CreateAsync(assignmentId, request, _currentUser);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("api/assignments/{assignmentId:int}/submissions")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<List<SubmissionResponse>>> ListForAssignment(int assignmentId) =>
        Ok(await _submissionService.ListForAssignmentAsync(assignmentId, _currentUser));

    [HttpGet("api/submissions/mine")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<List<SubmissionResponse>>> ListMine() =>
        Ok(await _submissionService.ListMineAsync(_currentUser));

    [HttpGet("api/submissions/{id:int}")]
    public async Task<ActionResult<SubmissionResponse>> GetById(int id) =>
        Ok(await _submissionService.GetByIdAsync(id, _currentUser));

    [HttpPut("api/submissions/{id:int}")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<SubmissionResponse>> Update(int id, UpdateSubmissionRequest request) =>
        Ok(await _submissionService.UpdateAsync(id, request, _currentUser));

    [HttpPut("api/submissions/{id:int}/grade")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<SubmissionResponse>> Grade(int id, GradeSubmissionRequest request) =>
        Ok(await _submissionService.GradeAsync(id, request, _currentUser));

    [HttpPatch("api/submissions/{id:int}/status")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<SubmissionResponse>> UpdateStatus(int id, UpdateSubmissionStatusRequest request) =>
        Ok(await _submissionService.UpdateStatusAsync(id, request.Status, _currentUser));
}
