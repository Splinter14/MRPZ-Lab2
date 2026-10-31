using ClassroomManagement.Api.DTOs;
using ClassroomManagement.Api.Models;
using ClassroomManagement.Api.Repositories;
using FluentAssertions;
using Xunit;

namespace ClassroomManagement.Tests;

public class ClassroomSearchTests
{
    private readonly InMemoryClassroomRepository _repository;

    public ClassroomSearchTests()
    {
        _repository = new InMemoryClassroomRepository();
        SeedSampleData(_repository).GetAwaiter().GetResult();
    }

    private static async Task SeedSampleData(InMemoryClassroomRepository repo)
    {
        await repo.AddAsync(new Classroom
        {
            Number = "101",
            Building = "Корпус 18",
            Capacity = 25,
            Equipment = new List<string> { "Дошка", "Проектор" }
        });

        await repo.AddAsync(new Classroom
        {
            Number = "202",
            Building = "Корпус 18",
            Capacity = 50,
            Equipment = new List<string> { "Проектор", "Кондиціонер", "Комп'ютери" }
        });

        await repo.AddAsync(new Classroom
        {
            Number = "303",
            Building = "Корпус 7",
            Capacity = 100,
            Equipment = new List<string> { "Проектор", "Мікрофон", "Аудіосистема" }
        });

        await repo.AddAsync(new Classroom
        {
            Number = "404",
            Building = "Корпус 7",
            Capacity = 15,
            Equipment = new List<string> { "Дошка" }
        });
    }

    [Fact] // KHPT-14: Перевірка роботи фільтра за місткістю
    public async Task SearchAsync_FilterByMinCapacity_ShouldReturnClassroomsWithCapacityGreaterOrEqualToMin()
    {
        // Arrange
        var query = new ClassroomSearchQuery { MinCapacity = 50 };

        // Act
        var results = (await _repository.SearchAsync(query)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(c => c.Capacity >= 50);
        results.Select(c => c.Number).Should().BeEquivalentTo(new[] { "202", "303" });
    }

    [Fact] // KHPT-14: Перевірка роботи фільтра за обладнанням
    public async Task SearchAsync_FilterByEquipment_ShouldReturnOnlyClassroomsWithRequiredEquipment()
    {
        // Arrange
        var query = new ClassroomSearchQuery { Equipment = "Комп'ютери" };

        // Act
        var results = (await _repository.SearchAsync(query)).ToList();

        // Assert
        results.Should().HaveCount(1);
        results.First().Number.Should().Be("202");
    }

    [Fact] // KHPT-15: Поєднання декількох умов пошуку (Місткість + Обладнання + Корпус)
    public async Task SearchAsync_CombinedConditions_ShouldReturnAccurateMatch()
    {
        // Arrange: Місткість >= 40, наявність "Проектор", у "Корпус 18"
        var query = new ClassroomSearchQuery
        {
            MinCapacity = 40,
            Equipment = "Проектор",
            Building = "Корпус 18"
        };

        // Act
        var results = (await _repository.SearchAsync(query)).ToList();

        // Assert
        results.Should().HaveCount(1);
        results.First().Number.Should().Be("202");
    }

    [Fact] // KHPT-15: Порожній запит повертає всі аудиторії
    public async Task SearchAsync_EmptyQuery_ShouldReturnAllClassrooms()
    {
        // Arrange
        var query = new ClassroomSearchQuery();

        // Act
        var results = (await _repository.SearchAsync(query)).ToList();

        // Assert
        results.Should().HaveCount(4);
    }

    [Fact] // KHPT-15: Запит без збігів повертає порожній список
    public async Task SearchAsync_NoMatch_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new ClassroomSearchQuery
        {
            MinCapacity = 500
        };

        // Act
        var results = await _repository.SearchAsync(query);

        // Assert
        results.Should().BeEmpty();
    }
}
