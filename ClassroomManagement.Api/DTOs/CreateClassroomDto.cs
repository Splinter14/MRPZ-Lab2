using System.ComponentModel.DataAnnotations;

namespace ClassroomManagement.Api.DTOs;

public class CreateClassroomDto
{
    [Required(ErrorMessage = "Номер аудиторії є обов'язковим.")]
    public string Number { get; set; } = string.Empty;

    [Required(ErrorMessage = "Корпус є обов'язковим.")]
    public string Building { get; set; } = string.Empty;

    [Range(1, 1000, ErrorMessage = "Місткість повинна бути більшою за 0.")]
    public int Capacity { get; set; }

    public List<string> Equipment { get; set; } = new();
}
