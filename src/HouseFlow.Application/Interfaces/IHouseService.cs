using HouseFlow.Application.DTOs;

namespace HouseFlow.Application.Interfaces;

public interface IHouseService
{
    Task<IEnumerable<HouseDto>> GetUserHousesAsync(Guid userId);
    Task<HouseDetailDto> GetHouseDetailsAsync(Guid houseId, Guid userId);
    Task<HouseDto> CreateHouseAsync(CreateHouseRequestDto request, Guid userId);
    Task InviteMemberAsync(Guid houseId, InviteMemberRequestDto request, Guid userId);
}
