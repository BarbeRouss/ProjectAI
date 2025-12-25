using HouseFlow.Application.DTOs;

namespace HouseFlow.Application.Interfaces;

public interface IDeviceService
{
    Task<IEnumerable<DeviceDto>> GetHouseDevicesAsync(Guid houseId, Guid userId);
    Task<DeviceDto> GetDeviceAsync(Guid deviceId, Guid userId);
    Task<DeviceDto> CreateDeviceAsync(Guid houseId, CreateDeviceRequestDto request, Guid userId);
}
