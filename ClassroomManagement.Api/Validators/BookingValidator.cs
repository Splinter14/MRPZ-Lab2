using ClassroomManagement.Api.DTOs;
using ClassroomManagement.Api.Repositories;

namespace ClassroomManagement.Api.Validators;

public enum BookingValidationResultType
{
    Valid,
    BadRequest,
    Conflict,
    NotFound
}

public record BookingValidationResult(BookingValidationResultType ResultType, string? ErrorMessage);

public class BookingValidator
{
    private readonly IClassroomRepository _classroomRepository;
    private readonly IBookingRepository _bookingRepository;

    public BookingValidator(IClassroomRepository classroomRepository, IBookingRepository bookingRepository)
    {
        _classroomRepository = classroomRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<BookingValidationResult> ValidateAsync(CreateBookingDto dto)
    {
        if (dto.ClassroomId == Guid.Empty)
        {
            return new BookingValidationResult(BookingValidationResultType.BadRequest, "ClassroomId є обов'язковим.");
        }

        if (string.IsNullOrWhiteSpace(dto.ReservedBy))
        {
            return new BookingValidationResult(BookingValidationResultType.BadRequest, "Поле 'Хто бронює' (ReservedBy) є обов'язковим.");
        }

        // КНРТ-20: Перевірка допустимості часу (час початку не може бути в минулому)
        // Допускаємо невеликий лаг (1 хвилину) для мережі/обробки
        if (dto.StartTime < DateTime.UtcNow.AddMinutes(-1))
        {
            return new BookingValidationResult(BookingValidationResultType.BadRequest, "Час початку не може бути в минулому.");
        }

        // КНРТ-20: Час початку повинен бути меншим за час закінчення
        if (dto.EndTime <= dto.StartTime)
        {
            return new BookingValidationResult(BookingValidationResultType.BadRequest, "Час закінчення повинен бути пізнішим за час початку.");
        }

        // Перевірка існування аудиторії
        var classroom = await _classroomRepository.GetByIdAsync(dto.ClassroomId);
        if (classroom == null)
        {
            return new BookingValidationResult(BookingValidationResultType.NotFound, $"Аудиторія з ID {dto.ClassroomId} не знайдена.");
        }

        // КНРТ-18: Перевірка перетину часу
        var conflict = await _bookingRepository.HasConflictAsync(dto.ClassroomId, dto.StartTime, dto.EndTime);
        if (conflict)
        {
            return new BookingValidationResult(BookingValidationResultType.Conflict, "Аудиторія вже заброньована на обраний час.");
        }

        return new BookingValidationResult(BookingValidationResultType.Valid, null);
    }
}
