using ClassroomManagement.Api.Controllers;
using ClassroomManagement.Api.DTOs;
using ClassroomManagement.Api.Models;
using ClassroomManagement.Api.Repositories;
using ClassroomManagement.Api.Validators;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ClassroomManagement.Tests;

public class BookingsControllerTests
{
    private readonly InMemoryClassroomRepository _classroomRepo;
    private readonly InMemoryBookingRepository _bookingRepo;
    private readonly BookingValidator _validator;
    private readonly BookingsController _controller;
    private readonly Guid _sampleClassroomId;

    public BookingsControllerTests()
    {
        _classroomRepo = new InMemoryClassroomRepository();
        _bookingRepo = new InMemoryBookingRepository();
        _validator = new BookingValidator(_classroomRepo, _bookingRepo);
        _controller = new BookingsController(_bookingRepo, _validator);

        var classroom = new Classroom
        {
            Number = "102-18",
            Building = "Корпус 18",
            Capacity = 30
        };
        _classroomRepo.AddAsync(classroom).GetAwaiter().GetResult();
        _sampleClassroomId = classroom.Id;
    }

    [Fact]
    public async Task CreateBooking_ValidData_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var start = DateTime.UtcNow.AddHours(2);
        var end = start.AddHours(2);
        var dto = new CreateBookingDto
        {
            ClassroomId = _sampleClassroomId,
            ReservedBy = "Кафедра ПЗ",
            StartTime = start,
            EndTime = end
        };

        // Act
        var result = await _controller.CreateBooking(dto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var response = createdResult.Value.Should().BeOfType<BookingResponseDto>().Subject;
        response.ReservedBy.Should().Be("Кафедра ПЗ");
        response.ClassroomId.Should().Be(_sampleClassroomId);
    }

    [Fact]
    public async Task CreateBooking_Conflict_ShouldReturnConflictResult()
    {
        // Arrange
        var start = DateTime.UtcNow.AddHours(4);
        var end = start.AddHours(2);

        await _bookingRepo.AddAsync(new Booking
        {
            ClassroomId = _sampleClassroomId,
            ReservedBy = "Викладач 1",
            StartTime = start,
            EndTime = end
        });

        var conflictDto = new CreateBookingDto
        {
            ClassroomId = _sampleClassroomId,
            ReservedBy = "Викладач 2",
            StartTime = start.AddMinutes(30),
            EndTime = end.AddMinutes(30)
        };

        // Act
        var result = await _controller.CreateBooking(conflictDto);

        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task CreateBooking_NonExistentClassroom_ShouldReturnNotFoundResult()
    {
        // Arrange
        var dto = new CreateBookingDto
        {
            ClassroomId = Guid.NewGuid(),
            ReservedBy = "Хтось",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        // Act
        var result = await _controller.CreateBooking(dto);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }
}
