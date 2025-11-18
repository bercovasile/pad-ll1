using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University.Api.Services.Interfaces;
using University.Domain.Entities;
using University.Persistance;

namespace University.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] Group group)
    {
        var created = await _groupService.CreateAsync(group);

        return CreatedAtAction(nameof(GetGroupById), new { id = created.Id }, created);
    }

    // READ by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGroupById(Guid id)
    {
        var group = await _groupService.GetByIdAsync(id);
        if (group == null)
            return NotFound();

        return Ok(group);
    }

    // UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] Group updated)
    {
        var result = await _groupService.UpdateAsync(id, updated);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        var deleted = await _groupService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
