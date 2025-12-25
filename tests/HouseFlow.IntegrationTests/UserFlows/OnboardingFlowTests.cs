using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HouseFlow.Application.DTOs;

namespace HouseFlow.IntegrationTests.UserFlows;

/// <summary>
/// Tests pour le Flux 1: Onboarding (First Time Experience)
/// L'utilisateur s'inscrit et crée sa première maison
/// </summary>
public class OnboardingFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OnboardingFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UserFlow_Onboarding_CompleteFirstTimeExperience_ShouldSucceed()
    {
        // ÉTAPE 1: Inscription (Email/Pass)
        var email = $"newuser{Guid.NewGuid()}@houseflow.com";
        var password = "SecurePass123!";
        var name = "Jean Dupont";

        var registerRequest = new RegisterRequestDto(email, password, name);
        var registerResponse = await _client.PostAsJsonAsync("/v1/auth/register", registerRequest);

        // Vérification: Inscription réussie
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResult.Should().NotBeNull();
        authResult!.Token.Should().NotBeNullOrEmpty();
        authResult.User.Email.Should().Be(email);
        authResult.User.Name.Should().Be(name);

        var token = authResult.Token;
        var userId = authResult.User.Id;

        // ÉTAPE 2: Vérifier que l'Organisation "Default" a été créée automatiquement (invisible)
        // Note: Ceci nécessiterait un endpoint pour récupérer les organisations de l'utilisateur
        // Pour l'instant, on vérifie que le token fonctionne

        // ÉTAPE 3: Création de la première maison (Wizard "Ma Première Maison")
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createHouseRequest = new CreateHouseRequestDto(
            Name: "Ma Maison Principale",
            Address: "123 Rue de la Paix",
            ZipCode: "75001",
            City: "Paris"
        );

        var createHouseResponse = await _client.PostAsJsonAsync("/v1/houses", createHouseRequest);

        // Vérification: Maison créée avec succès
        createHouseResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var house = await createHouseResponse.Content.ReadFromJsonAsync<HouseDto>();
        house.Should().NotBeNull();
        house!.Name.Should().Be("Ma Maison Principale");
        house.Address.Should().Be("123 Rue de la Paix");
        house.City.Should().Be("Paris");

        // ÉTAPE 4: Récupérer la liste des maisons (Dashboard)
        var getHousesResponse = await _client.GetAsync("/v1/houses");
        getHousesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var houses = await getHousesResponse.Content.ReadFromJsonAsync<List<HouseDto>>();
        houses.Should().NotBeNull();
        houses!.Should().HaveCount(1);
        houses[0].Name.Should().Be("Ma Maison Principale");

        // RÉSULTAT: L'utilisateur est inscrit, a une organisation par défaut, et sa première maison
        // → Prêt à utiliser l'application (Dashboard Maison avec call-to-action)
    }

    [Fact]
    public async Task UserFlow_Onboarding_LoginAfterRegistration_ShouldSucceed()
    {
        // CONTEXTE: Un utilisateur qui a déjà créé un compte
        var email = $"existinguser{Guid.NewGuid()}@houseflow.com";
        var password = "MyPassword123!";

        await _client.PostAsJsonAsync("/v1/auth/register",
            new RegisterRequestDto(email, password, "Test User"));

        // FLUX: Re-connexion
        var loginRequest = new LoginRequestDto(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/v1/auth/login", loginRequest);

        // Vérification: Login réussi avec le même token
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResult.Should().NotBeNull();
        authResult!.Token.Should().NotBeNullOrEmpty();
        authResult.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task UserFlow_Onboarding_InvalidCredentials_ShouldFail()
    {
        // FLUX: Tentative de connexion avec des identifiants invalides
        var loginRequest = new LoginRequestDto("nonexistent@test.com", "WrongPassword!");
        var loginResponse = await _client.PostAsJsonAsync("/v1/auth/login", loginRequest);

        // Vérification: Échec attendu
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UserFlow_Onboarding_DuplicateEmail_ShouldFail()
    {
        // CONTEXTE: Un utilisateur existe déjà
        var email = $"duplicate{Guid.NewGuid()}@houseflow.com";
        await _client.PostAsJsonAsync("/v1/auth/register",
            new RegisterRequestDto(email, "Password123!", "First User"));

        // FLUX: Tentative d'inscription avec le même email
        var duplicateRegister = await _client.PostAsJsonAsync("/v1/auth/register",
            new RegisterRequestDto(email, "DifferentPass123!", "Second User"));

        // Vérification: Échec attendu
        duplicateRegister.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
