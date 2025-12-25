using HouseFlow.Core.Entities;

namespace HouseFlow.Application.DTOs;

public record CreateMaintenanceTypeRequestDto(string Name, Periodicity Periodicity, int? CustomDays, bool ReminderEnabled, int ReminderDaysBefore);

public record MaintenanceTypeDto(Guid Id, string Name, Periodicity Periodicity, DateTime? NextDate);

public record LogMaintenanceRequestDto(DateTime Date, MaintenanceStatus Status, decimal? Cost, string? Provider, string? Notes);

public record MaintenanceInstanceDto(Guid Id, DateTime Date, MaintenanceStatus Status, decimal? Cost, string? Provider, string? Notes);
