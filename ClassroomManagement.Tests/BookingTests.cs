using ClassroomManagement.Api.DTOs;
using ClassroomManagement.Api.Models;
using ClassroomManagement.Api.Repositories;
using ClassroomManagement.Api.Validators;
using FluentAssertions;
using Xunit;

namespace ClassroomManagement.Tests;

public class BookingTests
{
    private readonly InMemoryClassroomRepository _classroomRepo;
    private readonly InMemoryBookingRepository _bookingRepo;
    private readonly BookingValidator _validator;
    private readonly Guid _sampleClassroomId;

    public BookingTests()
    {
        _classroomRepo = new InMemoryClassroomRepository();
        _bookingRepo = new InMemoryBookingRepository();
        _validator = new BookingValidator(_classroomRepo, _bookingRepo);

        var classroom = new Classroom
        {
            Number = "301",
            Building = "Корпус 1",
            Capacity = 30
        };
        _classroomRepo.AddAsync(classroom).GetAwaiter().GetResult();
        _sampleClassroomId = classroom.Id;
    }

    [Fact]
    public async Task ValidateAsync_ValidBooking_ShouldReturnValid()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(2);
        var endTime = startTime.AddHours(2);

        var dto = new CreateBookingDto
        {
            ClassroomId = _sampleClassroomId,
            ReservedBy = "Доц. Іваненко",
            StartTime = startTime,
            EndTime = endTime
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.ResultType.Should().Be(BookingValidationResultType.Valid);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact] // KHPT-20: Перевірка, що час початку не може бути в минулому
    public async Task ValidateAsync_StartTimeInPast_ShouldReturnBadRequest()
    {
        // Arrange
        var pastTime = DateTime.UtcNow.AddHours(-2);
        var endTime = DateTime.UtcNow.AddHours(1);

        var dto = new CreateBookingDto
        {
            ClassroomId = _sampleClassroomId,
            ReservedBy = "Студент Петро",
            StartTime = pastTime,
            EndTime = endTime
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.ResultType.Should().Be(BookingValidationResultType.BadRequest);
        result.ErrorMessage.Should().Contain("не може бути в минулому");
    }

    [Fact] // KHPT-20: Перевірка, що час закінчення має бути пізніше за час початку
    public async Task ValidateAsync_EndTimeBeforeOrEqualStartTime_ShouldReturnBadRequest()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(2);
        var invalidEndTime = startTime.AddMinutes(-30);

        var dto = new CreateBookingDto
        {
            ClassroomId = _sampleClassroomId,
            ReservedBy = "Студент Петро",
            StartTime = startTime,
            EndTime = invalidEndTime
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.ResultType.Should().Be(BookingValidationResultType.BadRequest);
        result.ErrorMessage.Should().Contain("пізнішим за час початку");
    }

    [Fact]
    public async Task ValidateAsync_NonExistentClassroom_ShouldReturnNotFound()
    {
        // Arrange
        var dto = new CreateBookingDto
        {
            ClassroomId = Guid.NewGuid(), // non-existent
            ReservedBy = "Викладач",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3)
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.ResultType.Should().Be(BookingValidationResultType.NotFound);
        result.ErrorMessage.Should().Contain("не знайдена");
    }

    [Fact] // KHPT-18, KHPT-21: Перетин часу - однаковий інтервал
    public async Task ValidateAsync_ExactIntervalConflict_ShouldReturnConflict()
    {
        // Arrange
        var start = DateTime.UtcNow.AddHours(5);
        var end = start.AddHours(2);

        await _bookingRepo.AddAsync(new Booking
        {
            ClassroomId = _sampleClassroomId,
            ReservedBy = "Існуюче бронювання",
            StartTime = start,
            EndTime = end
        });

        var conflictDto = new CreateBookingDto
        {
            ClassroomId = _sampleClassroomId,
            ReservedBy = "Нове бронювання",
            StartTime = start,
            EndTime = end
        };

        // Act
        var result = await _validator.ValidateAsync(conflictDto);

        // Assert
        result.ResultType.Should().Be(BookingValidationResultType.Conflict);
        result.ErrorMessage.Should().Contain("вже заброньована на обраний час");
    }

    [Fact] // KHPT-18, KHPT-21: Новий інтервал всередині існуючого
    public async Task ValidateAsync_IntervalInsideExisting_ShouldReturnConflict()
    {
        // Arrange
        var existingStart = DateTime.UtcNow.AddHours(10);
        var existingEnd = existingStart.AddHours(4);

        await _bookingRepo.AddAsync(new Booking
        {
            ClassroomId = _sampleClassroomId,
            ReservedBy = "Конференція",
            StartTime = existingStart,
            EndTime = existingEnd
        });

        var insideDto = new CreateBookingDto
        {
            ClassroomId = _sampleClassroomId,
            ReservedBy = "Семінар",
            StartTime = existingStart.AddHours(1),
            EndTime = existingStart.AddHours(2)
        };

        // Act
        var result = await _validator.ValidateAsync(insideDto);

        // Assert
        result.ResultType.Should().Be(BookingValidationResultType.Conflict);
    }

    [Fact] // KHPT-18, KHPT-21: Суміжні інтервали (один закінчується рівно тоді, коли починається інший) - НЕ конфлікт
    public async Task ValidateAsync_AdjacentIntervals_ShouldReturnValid()
    {
        // Arrange
        var slot1Start = DateTime.UtcNow.AddHours(3);
        var slot1End = slot1Start.AddHours(1);

        await _bookingRepo.AddAsync(new Booking
        {
            ClassroomId = _sampleClassroomId,
            ReservedBy = "Пара 1",
            StartTime = slot1Start,
            EndTime = slot1End
        });

        // Нове бронювання починається рівно в slot1End
        var slot2Dto = new CreateBookingDto
        {
            ClassroomId = _sampleClassroomId,
            ReservedBy = "Пара 2",
            StartTime = slot1End,
            EndTime = slot1End.AddHours(1)
        };

        // Act
        var result = await _validator.ValidateAsync(slot2Dto);

        // Assert
        result.ResultType.Should().Be(BookingValidationResultType.Valid);
    }
}
