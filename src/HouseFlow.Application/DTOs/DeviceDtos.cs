namespace HouseFlow.Application.DTOs;

public record CreateDeviceRequestDto(string Name, string Type, object? Metadata, DateTime? InstallDate);

public record DeviceDto(Guid Id, string Name, string Type, DateTime? InstallDate, object? Metadata, DateTime? NextMaintenanceDate);
