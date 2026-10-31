using System.ComponentModel.DataAnnotations;

namespace ClassroomManagement.Api.DTOs;

public class CreateBookingDto
{
    [Required(ErrorMessage = "ClassroomId є обов'язковим.")]
    public Guid ClassroomId { get; set; }

    [Required(ErrorMessage = "Поле 'Хто бронює' (ReservedBy) є обов'язковим.")]
    public string ReservedBy { get; set; } = string.Empty;

    [Required(ErrorMessage = "Час початку є обов'язковим.")]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "Час закінчення є обов'язковим.")]
    public DateTime EndTime { get; set; }
}
