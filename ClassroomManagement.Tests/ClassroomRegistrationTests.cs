using ClassroomManagement.Api.DTOs;
using ClassroomManagement.Api.Models;
using ClassroomManagement.Api.Repositories;
using ClassroomManagement.Api.Validators;
using FluentAssertions;
using Xunit;

namespace ClassroomManagement.Tests;

public class ClassroomRegistrationTests
{
    private readonly InMemoryClassroomRepository _repository;
    private readonly ClassroomValidator _validator;

    public ClassroomRegistrationTests()
    {
        _repository = new InMemoryClassroomRepository();
        _validator = new ClassroomValidator(_repository);
    }

    [Fact]
    public async Task ValidateAsync_ValidData_ShouldReturnValid()
    {
        // Arrange
        var dto = new CreateClassroomDto
        {
            Number = "305-18",
            Building = "Корпус 18",
            Capacity = 45,
            Equipment = new List<string> { "Проектор", "Кондиціонер" }
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(-50)]
    public async Task ValidateAsync_NonPositiveCapacity_ShouldReturnInvalid(int capacity)
    {
        // Arrange
        var dto = new CreateClassroomDto
        {
            Number = "101",
            Building = "Корпус 1",
            Capacity = capacity
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Місткість повинна бути більшою за 0");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateAsync_EmptyNumber_ShouldReturnInvalid(string? number)
    {
        // Arrange
        var dto = new CreateClassroomDto
        {
            Number = number!,
            Building = "Корпус 1",
            Capacity = 30
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Номер аудиторії є обов'язковим");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateAsync_EmptyBuilding_ShouldReturnInvalid(string? building)
    {
        // Arrange
        var dto = new CreateClassroomDto
        {
            Number = "202",
            Building = building!,
            Capacity = 30
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Корпус є обов'язковим");
    }

    [Fact]
    public async Task ValidateAsync_DuplicateClassroomInSameBuilding_ShouldReturnInvalid()
    {
        // Arrange
        await _repository.AddAsync(new Classroom
        {
            Number = "305",
            Building = "Корпус 18",
            Capacity = 40
        });

        var duplicateDto = new CreateClassroomDto
        {
            Number = "305",
            Building = "Корпус 18",
            Capacity = 50
        };

        // Act
        var result = await _validator.ValidateAsync(duplicateDto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("вже зареєстрована");
    }
}
