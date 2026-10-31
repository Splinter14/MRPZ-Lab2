using ClassroomManagement.Api.DTOs;
using ClassroomManagement.Api.Repositories;

namespace ClassroomManagement.Api.Validators;

public class ClassroomValidator
{
    private readonly IClassroomRepository _repository;

    public ClassroomValidator(IClassroomRepository repository)
    {
        _repository = repository;
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(CreateClassroomDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Number))
        {
            return (false, "Номер аудиторії є обов'язковим.");
        }

        if (string.IsNullOrWhiteSpace(dto.Building))
        {
            return (false, "Корпус є обов'язковим.");
        }

        if (dto.Capacity <= 0)
        {
            return (false, "Місткість повинна бути більшою за 0.");
        }

        var exists = await _repository.ExistsAsync(dto.Number.Trim(), dto.Building.Trim());
        if (exists)
        {
            return (false, $"Аудиторія з таким номером '{dto.Number}' вже зареєстрована у корпусі '{dto.Building}'.");
        }

        return (true, null);
    }
}
