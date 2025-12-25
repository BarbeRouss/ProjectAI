using System.Security.Claims;
using HouseFlow.Application.DTOs;
using HouseFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseFlow.API.Controllers;

[ApiController]
[Route("v1/maintenance-types")]
[Authorize]
[Produces("application/json")]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;

    public MaintenanceController(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    [HttpPost("{typeId}/instances")]
    [ProducesResponseType(typeof(MaintenanceInstanceDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> LogMaintenance(Guid typeId, [FromBody] LogMaintenanceRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var instance = await _maintenanceService.LogMaintenanceAsync(typeId, request, userId);
            return CreatedAtAction(nameof(LogMaintenance), new { typeId }, instance);
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
}
