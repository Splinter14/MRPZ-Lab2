namespace ClassroomManagement.Api.DTOs;

public class BookingResponseDto
{
    public Guid Id { get; set; }
    public Guid ClassroomId { get; set; }
    public string ReservedBy { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
}
