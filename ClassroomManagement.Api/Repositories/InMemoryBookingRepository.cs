using System.Collections.Concurrent;
using ClassroomManagement.Api.Models;

namespace ClassroomManagement.Api.Repositories;

public class InMemoryBookingRepository : IBookingRepository
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();

    public Task<Booking> AddAsync(Booking booking)
    {
        if (booking.Id == Guid.Empty)
        {
            booking.Id = Guid.NewGuid();
        }
        _bookings[booking.Id] = booking;
        return Task.FromResult(booking);
    }

    public Task<Booking?> GetByIdAsync(Guid id)
    {
        _bookings.TryGetValue(id, out var booking);
        return Task.FromResult(booking);
    }

    public Task<IEnumerable<Booking>> GetByClassroomIdAsync(Guid classroomId)
    {
        var bookings = _bookings.Values.Where(b => b.ClassroomId == classroomId).ToList();
        return Task.FromResult<IEnumerable<Booking>>(bookings);
    }

    public Task<bool> HasConflictAsync(Guid classroomId, DateTime start, DateTime end)
    {
        // Алгоритм перевірки перетинів часу (KHPT-18):
        // Два часові інтервали [Start1, End1] та [Start2, End2] перетинаються, якщо:
        // Start1 < End2 && End1 > Start2
        var hasConflict = _bookings.Values.Any(b =>
            b.ClassroomId == classroomId &&
            start < b.EndTime &&
            end > b.StartTime);

        return Task.FromResult(hasConflict);
    }
}
