using AssignmentSystem.Api.DTOs.ClassSubjects;
using AssignmentSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/class-subjects")]
[Authorize]
public class ClassSubjectsController : ControllerBase
{
    private readonly IClassSubjectService _classSubjectService;

    public ClassSubjectsController(IClassSubjectService classSubjectService)
    {
        _classSubjectService = classSubjectService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClassSubjectResponse>>> List() => Ok(await _classSubjectService.ListAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ClassSubjectResponse>> Create(CreateClassSubjectRequest request) =>
        Ok(await _classSubjectService.CreateAsync(request));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _classSubjectService.DeleteAsync(id);
        return NoContent();
    }
}
