using HouseFlow.Application.DTOs;

namespace HouseFlow.Application.Interfaces;

public interface IMaintenanceService
{
    Task<IEnumerable<MaintenanceTypeDto>> GetDeviceMaintenanceTypesAsync(Guid deviceId, Guid userId);
    Task<MaintenanceTypeDto> CreateMaintenanceTypeAsync(Guid deviceId, CreateMaintenanceTypeRequestDto request, Guid userId);
    Task<MaintenanceInstanceDto> LogMaintenanceAsync(Guid typeId, LogMaintenanceRequestDto request, Guid userId);
    Task<IEnumerable<MaintenanceInstanceDto>> GetDeviceMaintenanceHistoryAsync(Guid deviceId, Guid userId);
}
