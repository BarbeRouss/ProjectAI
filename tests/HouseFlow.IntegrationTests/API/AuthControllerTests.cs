using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HouseFlow.Application.DTOs;
using HouseFlow.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HouseFlow.IntegrationTests.API;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn200()
    {
        // Arrange
        var request = new RegisterRequestDto($"user{Guid.NewGuid()}@test.com", "Password123!", "New User");

        // Act
        var response = await _client.PostAsJsonAsync("/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200()
    {
        // Arrange
        var email = $"logintest{Guid.NewGuid()}@test.com";
        var password = "Password123!";

        await _client.PostAsJsonAsync("/v1/auth/register",
            new RegisterRequestDto(email, password, "Login Test"));

        var loginRequest = new LoginRequestDto(email, password);

        // Act
        var response = await _client.PostAsJsonAsync("/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturn401()
    {
        // Arrange
        var email = $"failtest{Guid.NewGuid()}@test.com";

        await _client.PostAsJsonAsync("/v1/auth/register",
            new RegisterRequestDto(email, "Password123!", "Fail Test"));

        var loginRequest = new LoginRequestDto(email, "WrongPassword!");

        // Act
        var response = await _client.PostAsJsonAsync("/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
