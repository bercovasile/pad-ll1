using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University.Api.Services.Interfaces;
using University.Domain.Entities;
using University.Persistance;

namespace University.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfController : ControllerBase
{
    private readonly IProfService _profService;

    public ProfController(IProfService profService)
    {
        _profService = profService;
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> CreateProf([FromBody] Prof prof)
    {
        var created = await _profService.CreateAsync(prof);

        return CreatedAtAction(nameof(GetProfById), new { id = created.Id }, created);
    }

    // READ
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfById(Guid id)
    {
        var prof = await _profService.GetByIdAsync(id);

        if (prof == null)
            return NotFound();

        return Ok(prof);
    }

    // UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProf(Guid id, [FromBody] Prof updated)
    {
        var result = await _profService.UpdateAsync(id, updated);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProf(Guid id)
    {
        var deleted = await _profService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}