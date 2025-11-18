using Microsoft.AspNetCore.Mvc;
using System;
using University.Api.Services.Interfaces;
using University.Domain.Entities;
using University.Persistance;

namespace University.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> CreateStudent([FromBody] Student student)
    {
        var created = await _studentService.CreateAsync(student);

        return CreatedAtAction(nameof(GetStudentById), new { id = created.Id }, created);
    }

    // READ
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudentById(Guid id)
    {
        var student = await _studentService.GetByIdAsync(id);

        if (student == null)
            return NotFound();

        return Ok(student);
    }

    // UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStudent(Guid id, [FromBody] Student updated)
    {
        var result = await _studentService.UpdateAsync(id, updated);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudent(Guid id)
    {
        var deleted = await _studentService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}