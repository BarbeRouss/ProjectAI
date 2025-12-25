using System.Security.Claims;
using HouseFlow.Application.DTOs;
using HouseFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseFlow.API.Controllers;

[ApiController]
[Route("v1/houses")]
[Authorize]
[Produces("application/json")]
public class HousesController : ControllerBase
{
    private readonly IHouseService _houseService;

    public HousesController(IHouseService houseService)
    {
        _houseService = houseService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HouseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHouses()
    {
        var userId = GetUserId();
        var houses = await _houseService.GetUserHousesAsync(userId);
        return Ok(houses);
    }

    [HttpPost]
    [ProducesResponseType(typeof(HouseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateHouse([FromBody] CreateHouseRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var house = await _houseService.CreateHouseAsync(request, userId);
            return CreatedAtAction(nameof(GetHouse), new { houseId = house.Id }, house);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
    }

    [HttpGet("{houseId}")]
    [ProducesResponseType(typeof(HouseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHouse(Guid houseId)
    {
        try
        {
            var userId = GetUserId();
            var house = await _houseService.GetHouseDetailsAsync(houseId, userId);
            return Ok(house);
        }
        catch (UnauthorizedAccessException)
        {
            return NotFound();
        }
    }

    [HttpPost("{houseId}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> InviteMember(Guid houseId, [FromBody] InviteMemberRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            await _houseService.InviteMemberAsync(houseId, request, userId);
            return Ok(new { message = "Invitation sent" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
