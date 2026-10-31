using ClassroomManagement.Api.DTOs;
using ClassroomManagement.Api.Models;
using ClassroomManagement.Api.Repositories;
using ClassroomManagement.Api.Validators;
using Microsoft.AspNetCore.Mvc;

namespace ClassroomManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingRepository _bookingRepository;
    private readonly BookingValidator _validator;

    public BookingsController(IBookingRepository bookingRepository, BookingValidator validator)
    {
        _bookingRepository = bookingRepository;
        _validator = validator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);

        switch (validation.ResultType)
        {
            case BookingValidationResultType.NotFound:
                return NotFound(new { message = validation.ErrorMessage });

            case BookingValidationResultType.Conflict:
                return Conflict(new { message = validation.ErrorMessage });

            case BookingValidationResultType.BadRequest:
                return BadRequest(new { message = validation.ErrorMessage });

            case BookingValidationResultType.Valid:
            default:
                break;
        }

        var booking = new Booking
        {
            ClassroomId = dto.ClassroomId,
            ReservedBy = dto.ReservedBy.Trim(),
            StartTime = dto.StartTime,
            EndTime = dto.EndTime
        };

        var created = await _bookingRepository.AddAsync(booking);

        var response = new BookingResponseDto
        {
            Id = created.Id,
            ClassroomId = created.ClassroomId,
            ReservedBy = created.ReservedBy,
            StartTime = created.StartTime,
            EndTime = created.EndTime,
            CreatedAt = created.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking == null)
        {
            return NotFound(new { message = $"Бронювання з ID {id} не знайдено." });
        }

        var response = new BookingResponseDto
        {
            Id = booking.Id,
            ClassroomId = booking.ClassroomId,
            ReservedBy = booking.ReservedBy,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            CreatedAt = booking.CreatedAt
        };

        return Ok(response);
    }

    [HttpGet("classroom/{classroomId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<BookingResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByClassroomId(Guid classroomId)
    {
        var bookings = await _bookingRepository.GetByClassroomIdAsync(classroomId);
        var response = bookings.Select(b => new BookingResponseDto
        {
            Id = b.Id,
            ClassroomId = b.ClassroomId,
            ReservedBy = b.ReservedBy,
            StartTime = b.StartTime,
            EndTime = b.EndTime,
            CreatedAt = b.CreatedAt
        });

        return Ok(response);
    }
}
