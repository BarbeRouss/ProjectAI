using HouseFlow.Application.DTOs;
using HouseFlow.Application.Interfaces;
using HouseFlow.Core.Entities;
using HouseFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseFlow.Infrastructure.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly HouseFlowDbContext _context;

    public MaintenanceService(HouseFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MaintenanceTypeDto>> GetDeviceMaintenanceTypesAsync(Guid deviceId, Guid userId)
    {
        var device = await _context.Devices
            .Include(d => d.MaintenanceTypes)
                .ThenInclude(mt => mt.MaintenanceInstances)
            .FirstOrDefaultAsync(d => d.Id == deviceId);

        if (device == null)
        {
            throw new KeyNotFoundException("Device not found");
        }

        await ValidateDeviceAccessAsync(deviceId, userId);

        return device.MaintenanceTypes.Select(mt => new MaintenanceTypeDto(
            mt.Id,
            mt.Name,
            mt.Periodicity,
            CalculateNextDate(mt)
        ));
    }

    public async Task<MaintenanceTypeDto> CreateMaintenanceTypeAsync(Guid deviceId, CreateMaintenanceTypeRequestDto request, Guid userId)
    {
        await ValidateDeviceAccessAsync(deviceId, userId, requireWrite: true);

        var maintenanceType = new MaintenanceType
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Periodicity = request.Periodicity,
            CustomDays = request.CustomDays,
            ReminderEnabled = request.ReminderEnabled,
            ReminderDaysBefore = request.ReminderDaysBefore,
            DeviceId = deviceId,
            CreatedAt = DateTime.UtcNow
        };

        _context.MaintenanceTypes.Add(maintenanceType);
        await _context.SaveChangesAsync();

        return new MaintenanceTypeDto(
            maintenanceType.Id,
            maintenanceType.Name,
            maintenanceType.Periodicity,
            null
        );
    }

    public async Task<MaintenanceInstanceDto> LogMaintenanceAsync(Guid typeId, LogMaintenanceRequestDto request, Guid userId)
    {
        var maintenanceType = await _context.MaintenanceTypes
            .Include(mt => mt.Device)
            .FirstOrDefaultAsync(mt => mt.Id == typeId);

        if (maintenanceType == null)
        {
            throw new KeyNotFoundException("Maintenance type not found");
        }

        await ValidateDeviceAccessAsync(maintenanceType.DeviceId, userId, requireWrite: true);

        var instance = new MaintenanceInstance
        {
            Id = Guid.NewGuid(),
            Date = request.Date,
            Status = request.Status,
            Cost = request.Cost,
            Provider = request.Provider,
            Notes = request.Notes,
            MaintenanceTypeId = typeId,
            CreatedAt = DateTime.UtcNow
        };

        _context.MaintenanceInstances.Add(instance);
        await _context.SaveChangesAsync();

        return new MaintenanceInstanceDto(
            instance.Id,
            instance.Date,
            instance.Status,
            instance.Cost,
            instance.Provider,
            instance.Notes
        );
    }

    public async Task<IEnumerable<MaintenanceInstanceDto>> GetDeviceMaintenanceHistoryAsync(Guid deviceId, Guid userId)
    {
        await ValidateDeviceAccessAsync(deviceId, userId);

        var instances = await _context.MaintenanceInstances
            .Where(mi => mi.MaintenanceType!.DeviceId == deviceId)
            .OrderByDescending(mi => mi.Date)
            .ToListAsync();

        return instances.Select(mi => new MaintenanceInstanceDto(
            mi.Id,
            mi.Date,
            mi.Status,
            mi.Cost,
            mi.Provider,
            mi.Notes
        ));
    }

    private async Task ValidateDeviceAccessAsync(Guid deviceId, Guid userId, bool requireWrite = false)
    {
        var device = await _context.Devices
            .Include(d => d.House)
                .ThenInclude(h => h.Members)
            .FirstOrDefaultAsync(d => d.Id == deviceId);

        if (device == null)
        {
            throw new KeyNotFoundException("Device not found");
        }

        var houseMember = device.House?.Members.FirstOrDefault(m => m.UserId == userId && m.Status == InvitationStatus.Accepted);

        if (houseMember == null)
        {
            throw new UnauthorizedAccessException("Access denied to this device");
        }

        if (requireWrite && houseMember.Role == HouseRole.Tenant)
        {
            throw new UnauthorizedAccessException("Tenants cannot modify maintenance records");
        }
    }

    private static DateTime? CalculateNextDate(MaintenanceType maintenanceType)
    {
        var lastInstance = maintenanceType.MaintenanceInstances
            .Where(mi => mi.Status == MaintenanceStatus.Completed)
            .OrderByDescending(mi => mi.Date)
            .FirstOrDefault();

        if (lastInstance == null)
        {
            return null;
        }

        var days = maintenanceType.Periodicity switch
        {
            Periodicity.Annual => 365,
            Periodicity.Semestrial => 180,
            Periodicity.Quarterly => 90,
            Periodicity.Monthly => 30,
            Periodicity.Custom => maintenanceType.CustomDays ?? 365,
            _ => 365
        };

        return lastInstance.Date.AddDays(days);
    }
}
