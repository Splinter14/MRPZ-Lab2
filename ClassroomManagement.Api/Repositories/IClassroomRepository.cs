using ClassroomManagement.Api.DTOs;
using ClassroomManagement.Api.Models;

namespace ClassroomManagement.Api.Repositories;

public interface IClassroomRepository
{
    Task<Classroom> AddAsync(Classroom classroom);
    Task<Classroom?> GetByIdAsync(Guid id);
    Task<IEnumerable<Classroom>> GetAllAsync();
    Task<bool> ExistsAsync(string number, string building);
    Task<IEnumerable<Classroom>> SearchAsync(ClassroomSearchQuery query);
}
