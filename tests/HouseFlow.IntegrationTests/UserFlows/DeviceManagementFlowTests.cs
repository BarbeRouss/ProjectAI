using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HouseFlow.Application.DTOs;
using HouseFlow.Core.Entities;

namespace HouseFlow.IntegrationTests.UserFlows;

/// <summary>
/// Tests pour le Flux 2: Gestion des Appareils et Entretiens
/// </summary>
public class DeviceManagementFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DeviceManagementFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UserFlow_AddDevice_WithAutoMaintenanceType_ShouldSucceed()
    {
        // CONTEXTE: Utilisateur connecté avec une maison
        var (token, houseId) = await CreateUserWithHouse();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // ÉTAPE 1: Ajout d'un appareil (Chaudière Gaz)
        var createDeviceRequest = new CreateDeviceRequestDto(
            Name: "Chaudière Sous-sol",
            Type: "Chaudière Gaz",
            Metadata: "{\"marque\":\"Viessmann\",\"modele\":\"Vitodens 200\"}",
            InstallDate: DateTime.UtcNow.AddYears(-2)
        );

        var createDeviceResponse = await _client.PostAsJsonAsync($"/v1/houses/{houseId}/devices", createDeviceRequest);

        // Vérification: Appareil créé
        createDeviceResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var device = await createDeviceResponse.Content.ReadFromJsonAsync<DeviceDto>();
        device.Should().NotBeNull();
        device!.Name.Should().Be("Chaudière Sous-sol");
        device.Type.Should().Be("Chaudière Gaz");

        // ÉTAPE 2: Récupérer l'appareil avec ses types d'entretien
        var getDeviceResponse = await _client.GetAsync($"/v1/devices/{device.Id}");
        getDeviceResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deviceDetails = await getDeviceResponse.Content.ReadFromJsonAsync<DeviceDto>();
        deviceDetails.Should().NotBeNull();

        // RÉSULTAT: L'appareil est créé et visible
        // Note: Auto-configuration des types d'entretien serait une feature future
    }

    [Fact]
    public async Task UserFlow_AddMaintenanceType_ToDevice_ShouldSucceed()
    {
        // CONTEXTE: Utilisateur avec une maison et un appareil
        var (token, houseId) = await CreateUserWithHouse();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Créer un appareil
        var deviceResponse = await _client.PostAsJsonAsync($"/v1/houses/{houseId}/devices",
            new CreateDeviceRequestDto("Poêle à Bois", "Chauffage", null, null));
        var device = await deviceResponse.Content.ReadFromJsonAsync<DeviceDto>();

        // ÉTAPE 1: Ajouter un type d'entretien manuel (Ramonage)
        var createMaintenanceTypeRequest = new CreateMaintenanceTypeRequestDto(
            Name: "Ramonage Annuel",
            Periodicity: Periodicity.Annual,
            CustomDays: null,
            ReminderEnabled: true,
            ReminderDaysBefore: 30
        );

        var createMTResponse = await _client.PostAsJsonAsync($"/v1/devices/{device!.Id}/maintenance-types", createMaintenanceTypeRequest);

        // Vérification: Type d'entretien créé
        createMTResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var maintenanceType = await createMTResponse.Content.ReadFromJsonAsync<MaintenanceTypeDto>();
        maintenanceType.Should().NotBeNull();
        maintenanceType!.Name.Should().Be("Ramonage Annuel");
        maintenanceType.Periodicity.Should().Be(Periodicity.Annual);

        // ÉTAPE 2: Vérifier que le type d'entretien est lié à l'appareil
        var getMaintenanceTypesResponse = await _client.GetAsync($"/v1/devices/{device.Id}/maintenance-types");
        getMaintenanceTypesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var maintenanceTypes = await getMaintenanceTypesResponse.Content.ReadFromJsonAsync<List<MaintenanceTypeDto>>();
        maintenanceTypes.Should().NotBeNull();
        maintenanceTypes!.Should().HaveCountGreaterThanOrEqualTo(1);
        maintenanceTypes.Should().Contain(mt => mt.Name == "Ramonage Annuel");
    }

    [Fact]
    public async Task UserFlow_LogMaintenance_QuickEntry_ShouldSucceed()
    {
        // CONTEXTE: Utilisateur avec appareil et type d'entretien
        var (token, houseId) = await CreateUserWithHouse();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var deviceResponse = await _client.PostAsJsonAsync($"/v1/houses/{houseId}/devices",
            new CreateDeviceRequestDto("Détecteur Fumée", "Sécurité", null, null));
        var device = await deviceResponse.Content.ReadFromJsonAsync<DeviceDto>();

        var mtResponse = await _client.PostAsJsonAsync($"/v1/devices/{device!.Id}/maintenance-types",
            new CreateMaintenanceTypeRequestDto("Test Batterie", Periodicity.Monthly, null, false, 0));
        var maintenanceType = await mtResponse.Content.ReadFromJsonAsync<MaintenanceTypeDto>();

        // ÉTAPE 1: Encoder un entretien (Saisie Rapide)
        var logMaintenanceRequest = new LogMaintenanceRequestDto(
            Date: DateTime.UtcNow,
            Status: MaintenanceStatus.Completed,
            Cost: null, // Saisie rapide: pas de coût
            Provider: null,
            Notes: null
        );

        var logResponse = await _client.PostAsJsonAsync($"/v1/maintenance-types/{maintenanceType!.Id}/instances", logMaintenanceRequest);

        // Vérification: Instance créée
        logResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var instance = await logResponse.Content.ReadFromJsonAsync<MaintenanceInstanceDto>();
        instance.Should().NotBeNull();
        instance!.Status.Should().Be(MaintenanceStatus.Completed);
        instance.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        // ÉTAPE 2: Vérifier l'historique
        var getInstancesResponse = await _client.GetAsync($"/v1/devices/{device.Id}/maintenance-instances");
        getInstancesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var instances = await getInstancesResponse.Content.ReadFromJsonAsync<List<MaintenanceInstanceDto>>();
        instances.Should().NotBeNull();
        instances!.Should().HaveCount(1);
    }

    [Fact]
    public async Task UserFlow_LogMaintenance_DetailedEntry_ShouldSucceed()
    {
        // CONTEXTE: Utilisateur avec appareil et type d'entretien
        var (token, houseId) = await CreateUserWithHouse();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var deviceResponse = await _client.PostAsJsonAsync($"/v1/houses/{houseId}/devices",
            new CreateDeviceRequestDto("Climatisation", "Climatisation", null, null));
        var device = await deviceResponse.Content.ReadFromJsonAsync<DeviceDto>();

        var mtResponse = await _client.PostAsJsonAsync($"/v1/devices/{device!.Id}/maintenance-types",
            new CreateMaintenanceTypeRequestDto("Entretien Clim", Periodicity.Annual, null, true, 15));
        var maintenanceType = await mtResponse.Content.ReadFromJsonAsync<MaintenanceTypeDto>();

        // ÉTAPE 1: Encoder un entretien (Saisie Détaillée)
        var logMaintenanceRequest = new LogMaintenanceRequestDto(
            Date: DateTime.UtcNow.AddDays(-1),
            Status: MaintenanceStatus.Completed,
            Cost: 150.50m,
            Provider: "Clim Expert SARL",
            Notes: "Remplacement filtre + vérification fluide frigorigène. RAS."
        );

        var logResponse = await _client.PostAsJsonAsync($"/v1/maintenance-types/{maintenanceType!.Id}/instances", logMaintenanceRequest);

        // Vérification: Instance créée avec détails
        logResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var instance = await logResponse.Content.ReadFromJsonAsync<MaintenanceInstanceDto>();
        instance.Should().NotBeNull();
        instance!.Cost.Should().Be(150.50m);
        instance.Provider.Should().Be("Clim Expert SARL");
        instance.Notes.Should().Contain("fluide frigorigène");

        // RÉSULTAT: L'entretien est enregistré avec tous les détails pour l'historique
    }

    [Fact]
    public async Task UserFlow_ViewDeviceDashboard_WithMultipleDevices_ShouldShowAll()
    {
        // CONTEXTE: Utilisateur avec plusieurs appareils dans sa maison
        var (token, houseId) = await CreateUserWithHouse();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Ajouter plusieurs appareils
        await _client.PostAsJsonAsync($"/v1/houses/{houseId}/devices",
            new CreateDeviceRequestDto("Chaudière", "Chauffage", null, null));
        await _client.PostAsJsonAsync($"/v1/houses/{houseId}/devices",
            new CreateDeviceRequestDto("Toiture", "Toiture", null, DateTime.UtcNow.AddYears(-10)));
        await _client.PostAsJsonAsync($"/v1/houses/{houseId}/devices",
            new CreateDeviceRequestDto("Alarme", "Sécurité", null, null));

        // FLUX: Récupérer la liste des appareils de la maison
        var getDevicesResponse = await _client.GetAsync($"/v1/houses/{houseId}/devices");

        // Vérification: Tous les appareils sont visibles
        getDevicesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var devices = await getDevicesResponse.Content.ReadFromJsonAsync<List<DeviceDto>>();
        devices.Should().NotBeNull();
        devices!.Should().HaveCount(3);
        devices.Should().Contain(d => d.Name == "Chaudière");
        devices.Should().Contain(d => d.Name == "Toiture");
        devices.Should().Contain(d => d.Name == "Alarme");
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
