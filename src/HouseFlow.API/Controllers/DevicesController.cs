using System.Security.Claims;
using HouseFlow.Application.DTOs;
using HouseFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseFlow.API.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IMaintenanceService _maintenanceService;

    public DevicesController(IDeviceService deviceService, IMaintenanceService maintenanceService)
    {
        _deviceService = deviceService;
        _maintenanceService = maintenanceService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    [HttpGet("v1/houses/{houseId}/devices")]
    [ProducesResponseType(typeof(IEnumerable<DeviceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHouseDevices(Guid houseId)
    {
        try
        {
            var userId = GetUserId();
            var devices = await _deviceService.GetHouseDevicesAsync(houseId, userId);
            return Ok(devices);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("v1/houses/{houseId}/devices")]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDevice(Guid houseId, [FromBody] CreateDeviceRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var device = await _deviceService.CreateDeviceAsync(houseId, request, userId);
            return CreatedAtAction(nameof(GetDevice), new { deviceId = device.Id }, device);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("v1/devices/{deviceId}")]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDevice(Guid deviceId)
    {
        try
        {
            var userId = GetUserId();
            var device = await _deviceService.GetDeviceAsync(deviceId, userId);
            return Ok(device);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("v1/devices/{deviceId}/maintenance-types")]
    [ProducesResponseType(typeof(IEnumerable<MaintenanceTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeviceMaintenanceTypes(Guid deviceId)
    {
        try
        {
            var userId = GetUserId();
            var types = await _maintenanceService.GetDeviceMaintenanceTypesAsync(deviceId, userId);
            return Ok(types);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("v1/devices/{deviceId}/maintenance-types")]
    [ProducesResponseType(typeof(MaintenanceTypeDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateMaintenanceType(Guid deviceId, [FromBody] CreateMaintenanceTypeRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var type = await _maintenanceService.CreateMaintenanceTypeAsync(deviceId, request, userId);
            return CreatedAtAction(nameof(GetDeviceMaintenanceTypes), new { deviceId }, type);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("v1/devices/{deviceId}/maintenance-instances")]
    [ProducesResponseType(typeof(IEnumerable<MaintenanceInstanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeviceMaintenanceHistory(Guid deviceId)
    {
        try
        {
            var userId = GetUserId();
            var instances = await _maintenanceService.GetDeviceMaintenanceHistoryAsync(deviceId, userId);
            return Ok(instances);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
