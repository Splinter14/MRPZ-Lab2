using ClassroomManagement.Api.DTOs;
using ClassroomManagement.Api.Models;
using ClassroomManagement.Api.Repositories;
using ClassroomManagement.Api.Validators;
using Microsoft.AspNetCore.Mvc;

namespace ClassroomManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassroomsController : ControllerBase
{
    private readonly IClassroomRepository _repository;
    private readonly ClassroomValidator _validator;

    public ClassroomsController(IClassroomRepository repository, ClassroomValidator validator)
    {
        _repository = repository;
        _validator = validator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClassroomResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClassroom([FromBody] CreateClassroomDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return BadRequest(new { message = validation.ErrorMessage });
        }

        var classroom = new Classroom
        {
            Number = dto.Number.Trim(),
            Building = dto.Building.Trim(),
            Capacity = dto.Capacity,
            Equipment = dto.Equipment ?? new List<string>()
        };

        var created = await _repository.AddAsync(classroom);

        var response = new ClassroomResponseDto
        {
            Id = created.Id,
            Number = created.Number,
            Building = created.Building,
            Capacity = created.Capacity,
            Equipment = created.Equipment,
            CreatedAt = created.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClassroomResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var classroom = await _repository.GetByIdAsync(id);
        if (classroom == null)
        {
            return NotFound(new { message = $"Аудиторія з ID {id} не знайдена." });
        }

        var response = new ClassroomResponseDto
        {
            Id = classroom.Id,
            Number = classroom.Number,
            Building = classroom.Building,
            Capacity = classroom.Capacity,
            Equipment = classroom.Equipment,
            CreatedAt = classroom.CreatedAt
        };

        return Ok(response);
    }
}
