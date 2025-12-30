using System.ComponentModel.DataAnnotations;
using HouseFlow.Core.Entities;

namespace HouseFlow.Application.DTOs;

public record CreateMaintenanceTypeRequestDto(
    [Required(ErrorMessage = "Maintenance type name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters")]
    string Name,

    [EnumDataType(typeof(Periodicity), ErrorMessage = "Invalid periodicity")]
    Periodicity Periodicity,

    [Range(1, 3650, ErrorMessage = "Custom days must be between 1 and 3650 (10 years)")]
    int? CustomDays,

    bool ReminderEnabled,

    [Range(1, 365, ErrorMessage = "Reminder days must be between 1 and 365")]
    int ReminderDaysBefore
);

public record MaintenanceTypeDto(Guid Id, string Name, Periodicity Periodicity, DateTime? NextDate);

public record LogMaintenanceRequestDto(
    [Required(ErrorMessage = "Date is required")]
    DateTime Date,

    [EnumDataType(typeof(MaintenanceStatus), ErrorMessage = "Invalid status")]
    MaintenanceStatus Status,

    [Range(0, 1000000, ErrorMessage = "Cost must be between 0 and 1,000,000")]
    decimal? Cost,

    [StringLength(255, ErrorMessage = "Provider name cannot exceed 255 characters")]
    string? Provider,

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    string? Notes
);

public record MaintenanceInstanceDto(Guid Id, DateTime Date, MaintenanceStatus Status, decimal? Cost, string? Provider, string? Notes);
