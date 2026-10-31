namespace ClassroomManagement.Api.Models;

public class Classroom
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Number { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public List<string> Equipment { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
