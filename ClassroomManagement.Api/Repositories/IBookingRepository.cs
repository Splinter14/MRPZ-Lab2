using ClassroomManagement.Api.Models;

namespace ClassroomManagement.Api.Repositories;

public interface IBookingRepository
{
    Task<Booking> AddAsync(Booking booking);
    Task<Booking?> GetByIdAsync(Guid id);
    Task<IEnumerable<Booking>> GetByClassroomIdAsync(Guid classroomId);
    Task<bool> HasConflictAsync(Guid classroomId, DateTime start, DateTime end);
}
