using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HouseFlow.Application.DTOs;
using HouseFlow.Core.Entities;

namespace HouseFlow.IntegrationTests.UserFlows;

/// <summary>
/// Tests pour le Flux 3: Collaboration et Partage
/// </summary>
public class CollaborationFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CollaborationFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UserFlow_InviteCollaborator_AsOwner_ShouldSucceed()
    {
        // CONTEXTE: Propriétaire avec une maison
        var (ownerToken, houseId) = await CreateUserWithHouse();

        // Créer d'abord l'utilisateur collaborateur
        var collaboratorEmail = $"collaborator{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/v1/auth/register",
            new RegisterRequestDto(collaboratorEmail, "Test123!", "Collaborator User"));

        // Le propriétaire invite le collaborateur
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ownerToken);

        var inviteRequest = new InviteMemberRequestDto(
            Email: collaboratorEmail,
            Role: HouseRole.Collaborator
        );

        var inviteResponse = await _client.PostAsJsonAsync($"/v1/houses/{houseId}/members", inviteRequest);

        // Vérification: Invitation envoyée
        inviteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // ÉTAPE 2: Vérifier que l'invitation est visible dans les membres
        var getHouseResponse = await _client.GetAsync($"/v1/houses/{houseId}");
        getHouseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var houseDetail = await getHouseResponse.Content.ReadFromJsonAsync<HouseDetailDto>();
        houseDetail.Should().NotBeNull();
        houseDetail!.Members.Should().Contain(m => m.Email == collaboratorEmail && m.Role == HouseRole.Collaborator);

        // RÉSULTAT: Le collaborateur est invité et peut accéder à la maison
    }

    [Fact]
    public async Task UserFlow_InviteTenant_AsOwner_ShouldSucceed()
    {
        // CONTEXTE: Propriétaire avec une maison
        var (ownerToken, houseId) = await CreateUserWithHouse();

        // Créer d'abord l'utilisateur locataire
        var tenantEmail = $"tenant{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/v1/auth/register",
            new RegisterRequestDto(tenantEmail, "Test123!", "Tenant User"));

        // Le propriétaire invite le locataire
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ownerToken);

        var inviteRequest = new InviteMemberRequestDto(
            Email: tenantEmail,
            Role: HouseRole.Tenant
        );

        var inviteResponse = await _client.PostAsJsonAsync($"/v1/houses/{houseId}/members", inviteRequest);

        // Vérification: Invitation envoyée
        inviteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // ÉTAPE 2: Vérifier que le locataire est visible dans les membres
        var getHouseResponse = await _client.GetAsync($"/v1/houses/{houseId}");
        getHouseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var houseDetail = await getHouseResponse.Content.ReadFromJsonAsync<HouseDetailDto>();
        houseDetail.Should().NotBeNull();
        houseDetail!.Members.Should().Contain(m => m.Email == tenantEmail && m.Role == HouseRole.Tenant);

        // RÉSULTAT: Le locataire a un accès en lecture seule
    }

    [Fact]
    public async Task UserFlow_CannotInviteNonExistentUser_ShouldFail()
    {
        // CONTEXTE: Propriétaire avec une maison
        var (ownerToken, houseId) = await CreateUserWithHouse();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ownerToken);

        // FLUX: Tenter d'inviter un utilisateur qui n'existe pas
        var nonExistentEmail = $"nonexistent{Guid.NewGuid()}@test.com";
        var inviteRequest = new InviteMemberRequestDto(nonExistentEmail, HouseRole.Collaborator);

        var inviteResponse = await _client.PostAsJsonAsync($"/v1/houses/{houseId}/members", inviteRequest);

        // Vérification: L'invitation échoue
        inviteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // RÉSULTAT: L'utilisateur doit d'abord être enregistré dans le système
    }

    [Fact]
    public async Task UserFlow_ViewHouseMembers_ShowsAllRoles_ShouldSucceed()
    {
        // CONTEXTE: Propriétaire avec plusieurs membres
        var (ownerToken, houseId) = await CreateUserWithHouse();

        // Créer les utilisateurs collaborateur et locataire
        var collaboratorEmail = $"collaborator{Guid.NewGuid()}@test.com";
        var tenantEmail = $"tenant{Guid.NewGuid()}@test.com";

        await _client.PostAsJsonAsync("/v1/auth/register",
            new RegisterRequestDto(collaboratorEmail, "Test123!", "Collaborator User"));
        await _client.PostAsJsonAsync("/v1/auth/register",
            new RegisterRequestDto(tenantEmail, "Test123!", "Tenant User"));

        // Inviter les membres
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ownerToken);

        await _client.PostAsJsonAsync($"/v1/houses/{houseId}/members",
            new InviteMemberRequestDto(collaboratorEmail, HouseRole.Collaborator));
        await _client.PostAsJsonAsync($"/v1/houses/{houseId}/members",
            new InviteMemberRequestDto(tenantEmail, HouseRole.Tenant));

        // FLUX: Récupérer les détails de la maison avec tous les membres
        var getHouseResponse = await _client.GetAsync($"/v1/houses/{houseId}");
        getHouseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var houseDetail = await getHouseResponse.Content.ReadFromJsonAsync<HouseDetailDto>();
        houseDetail.Should().NotBeNull();

        // Vérification: Tous les membres sont visibles avec leurs rôles
        houseDetail!.Members.Should().HaveCountGreaterThanOrEqualTo(3); // Owner + Collaborator + Tenant
        houseDetail.Members.Should().Contain(m => m.Role == HouseRole.Owner); // Owner
        houseDetail.Members.Should().Contain(m => m.Email == collaboratorEmail && m.Role == HouseRole.Collaborator);
        houseDetail.Members.Should().Contain(m => m.Email == tenantEmail && m.Role == HouseRole.Tenant);
    }

    [Fact(Skip = "Requires Premium subscription to create multiple houses")]
    public async Task UserFlow_MultipleHouses_EachWithOwnMembers_RequiresPremium()
    {
        // NOTE: Ce test est désactivé car il nécessite un abonnement Premium
        // Le plan gratuit limite à 1 maison
        // Ce test devrait être activé dans un contexte de tests Premium

        // CONTEXTE: Utilisateur avec plusieurs maisons (Premium uniquement)
        var (token, house1Id) = await CreateUserWithHouse();

        // FLUX: Tenter de créer une deuxième maison en tant qu'utilisateur Free
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var house2Response = await _client.PostAsJsonAsync("/v1/houses",
            new CreateHouseRequestDto("Maison 2", "456 Test Ave", "54321", "Test City 2"));

        // Vérification: L'utilisateur Free ne peut pas créer une 2ème maison
        house2Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // RÉSULTAT: Le système force l'upgrade vers Premium pour gérer plusieurs maisons
    }

    // Helper method
    private async Task<(string token, Guid houseId)> CreateUserWithHouse()
    {
        var email = $"user{Guid.NewGuid()}@test.com";
        var registerResponse = await _client.PostAsJsonAsync("/v1/auth/register",
            new RegisterRequestDto(email, "Test123!", "Test User"));
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.Token);

        var houseResponse = await _client.PostAsJsonAsync("/v1/houses",
            new CreateHouseRequestDto("Test House", "123 Test St", "12345", "Test City"));
        var house = await houseResponse.Content.ReadFromJsonAsync<HouseDto>();

        return (authResult.Token, house!.Id);
    }
}
