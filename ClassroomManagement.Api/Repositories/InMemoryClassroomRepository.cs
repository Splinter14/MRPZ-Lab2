using System.Collections.Concurrent;
using ClassroomManagement.Api.Models;

namespace ClassroomManagement.Api.Repositories;

public class InMemoryClassroomRepository : IClassroomRepository
{
    private readonly ConcurrentDictionary<Guid, Classroom> _classrooms = new();

    public Task<Classroom> AddAsync(Classroom classroom)
    {
        if (classroom.Id == Guid.Empty)
        {
            classroom.Id = Guid.NewGuid();
        }
        _classrooms[classroom.Id] = classroom;
        return Task.FromResult(classroom);
    }

    public Task<Classroom?> GetByIdAsync(Guid id)
    {
        _classrooms.TryGetValue(id, out var classroom);
        return Task.FromResult(classroom);
    }

    public Task<IEnumerable<Classroom>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Classroom>>(_classrooms.Values.ToList());
    }

    public Task<bool> ExistsAsync(string number, string building)
    {
        var exists = _classrooms.Values.Any(c => 
            c.Number.Equals(number, StringComparison.OrdinalIgnoreCase) && 
            c.Building.Equals(building, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }
}
