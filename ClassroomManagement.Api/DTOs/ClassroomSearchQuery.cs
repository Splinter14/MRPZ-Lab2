namespace ClassroomManagement.Api.DTOs;

public class ClassroomSearchQuery
{
    public int? MinCapacity { get; set; }
    public string? Equipment { get; set; }
    public string? Building { get; set; }
}
