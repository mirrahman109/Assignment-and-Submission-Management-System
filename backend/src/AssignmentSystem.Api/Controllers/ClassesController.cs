using AssignmentSystem.Api.DTOs.Classes;
using AssignmentSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/classes")]
[Authorize]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;
    private readonly ICurrentUserService _currentUser;

    public ClassesController(IClassService classService, ICurrentUserService currentUser)
    {
        _classService = classService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClassCourseResponse>>> List() => Ok(await _classService.ListAsync(_currentUser));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ClassCourseResponse>> Create(CreateClassCourseRequest request) =>
        Ok(await _classService.CreateAsync(request));

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ClassCourseResponse>> Update(int id, UpdateClassCourseRequest request) =>
        Ok(await _classService.UpdateAsync(id, request));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _classService.DeleteAsync(id);
        return NoContent();
    }
}
