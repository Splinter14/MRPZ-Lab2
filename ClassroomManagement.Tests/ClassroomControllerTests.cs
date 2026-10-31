using ClassroomManagement.Api.Controllers;
using ClassroomManagement.Api.DTOs;
using ClassroomManagement.Api.Models;
using ClassroomManagement.Api.Repositories;
using ClassroomManagement.Api.Validators;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ClassroomManagement.Tests;

public class ClassroomControllerTests
{
    private readonly InMemoryClassroomRepository _repository;
    private readonly ClassroomValidator _validator;
    private readonly ClassroomsController _controller;

    public ClassroomControllerTests()
    {
        _repository = new InMemoryClassroomRepository();
        _validator = new ClassroomValidator(_repository);
        _controller = new ClassroomsController(_repository, _validator);
    }

    [Fact]
    public async Task CreateClassroom_ValidDto_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var dto = new CreateClassroomDto
        {
            Number = "401-18",
            Building = "Корпус 18",
            Capacity = 60,
            Equipment = new List<string> { "Проектор", "Комп'ютери" }
        };

        // Act
        var result = await _controller.CreateClassroom(dto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var response = createdResult.Value.Should().BeOfType<ClassroomResponseDto>().Subject;
        response.Number.Should().Be("401-18");
        response.Capacity.Should().Be(60);
        response.Equipment.Should().Contain("Проектор");
    }

    [Fact]
    public async Task CreateClassroom_InvalidCapacity_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new CreateClassroomDto
        {
            Number = "402-18",
            Building = "Корпус 18",
            Capacity = 0
        };

        // Act
        var result = await _controller.CreateClassroom(dto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Search_MatchingQuery_ShouldReturnOkWithFilteredClassrooms()
    {
        // Arrange
        await _repository.AddAsync(new Classroom
        {
            Number = "105",
            Building = "Корпус 18",
            Capacity = 40,
            Equipment = new List<string> { "Проектор" }
        });

        var query = new ClassroomSearchQuery { MinCapacity = 30 };

        // Act
        var result = await _controller.Search(query);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var classrooms = okResult.Value.Should().BeAssignableTo<IEnumerable<ClassroomResponseDto>>().Subject;
        classrooms.Should().ContainSingle(c => c.Number == "105");
    }
}
