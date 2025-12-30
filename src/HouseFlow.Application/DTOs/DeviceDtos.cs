using System.ComponentModel.DataAnnotations;

namespace HouseFlow.Application.DTOs;

public record CreateDeviceRequestDto(
    [Required(ErrorMessage = "Device name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Device name must be between 1 and 255 characters")]
    string Name,

    [Required(ErrorMessage = "Device type is required")]
    [StringLength(100, ErrorMessage = "Device type cannot exceed 100 characters")]
    string Type,

    object? Metadata,

    DateTime? InstallDate
);

public record DeviceDto(Guid Id, string Name, string Type, DateTime? InstallDate, object? Metadata, DateTime? NextMaintenanceDate);
