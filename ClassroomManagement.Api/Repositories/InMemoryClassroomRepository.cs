using System.Collections.Concurrent;
using ClassroomManagement.Api.DTOs;
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

    public Task<IEnumerable<Classroom>> SearchAsync(ClassroomSearchQuery query)
    {
        var items = _classrooms.Values.AsEnumerable();

        if (query.MinCapacity.HasValue)
        {
            items = items.Where(c => c.Capacity >= query.MinCapacity.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Building))
        {
            items = items.Where(c => c.Building.Contains(query.Building, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Equipment))
        {
            var requestedEquipments = query.Equipment
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var req in requestedEquipments)
            {
                items = items.Where(c => c.Equipment.Any(e => e.Equals(req, StringComparison.OrdinalIgnoreCase)));
            }
        }

        return Task.FromResult<IEnumerable<Classroom>>(items.ToList());
    }
}
