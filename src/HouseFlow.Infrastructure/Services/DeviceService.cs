using System.Text.Json;
using HouseFlow.Application.DTOs;
using HouseFlow.Application.Interfaces;
using HouseFlow.Core.Entities;
using HouseFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseFlow.Infrastructure.Services;

public class DeviceService : IDeviceService
{
    private readonly HouseFlowDbContext _context;

    public DeviceService(HouseFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DeviceDto>> GetHouseDevicesAsync(Guid houseId, Guid userId)
    {
        await ValidateHouseAccessAsync(houseId, userId);

        var devices = await _context.Devices
            .Where(d => d.HouseId == houseId)
            .Include(d => d.MaintenanceTypes)
                .ThenInclude(mt => mt.MaintenanceInstances)
            .ToListAsync();

        return devices.Select(d => new DeviceDto(
            d.Id,
            d.Name,
            d.Type,
            d.InstallDate,
            d.Metadata != null ? JsonSerializer.Deserialize<object>(d.Metadata) : null,
            CalculateNextMaintenanceDate(d.MaintenanceTypes)
        ));
    }

    public async Task<DeviceDto> GetDeviceAsync(Guid deviceId, Guid userId)
    {
        var device = await _context.Devices
            .Include(d => d.House)
            .Include(d => d.MaintenanceTypes)
                .ThenInclude(mt => mt.MaintenanceInstances)
            .FirstOrDefaultAsync(d => d.Id == deviceId);

        if (device == null)
        {
            throw new KeyNotFoundException("Device not found");
        }

        await ValidateHouseAccessAsync(device.HouseId, userId);

        return new DeviceDto(
            device.Id,
            device.Name,
            device.Type,
            device.InstallDate,
            device.Metadata != null ? JsonSerializer.Deserialize<object>(device.Metadata) : null,
            CalculateNextMaintenanceDate(device.MaintenanceTypes)
        );
    }

    public async Task<DeviceDto> CreateDeviceAsync(Guid houseId, CreateDeviceRequestDto request, Guid userId)
    {
        await ValidateHouseAccessAsync(houseId, userId, requireWrite: true);

        var device = new Device
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            InstallDate = request.InstallDate,
            Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null,
            HouseId = houseId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        return new DeviceDto(
            device.Id,
            device.Name,
            device.Type,
            device.InstallDate,
            request.Metadata,
            null
        );
    }

    private async Task ValidateHouseAccessAsync(Guid houseId, Guid userId, bool requireWrite = false)
    {
        var houseMember = await _context.HouseMembers
            .FirstOrDefaultAsync(hm => hm.HouseId == houseId && hm.UserId == userId && hm.Status == InvitationStatus.Accepted);

        if (houseMember == null)
        {
            throw new UnauthorizedAccessException("Access denied to this house");
        }

        if (requireWrite && houseMember.Role == HouseRole.Tenant)
        {
            throw new UnauthorizedAccessException("Tenants cannot modify devices");
        }
    }

    private static DateTime? CalculateNextMaintenanceDate(IEnumerable<MaintenanceType> maintenanceTypes)
    {
        DateTime? nextDate = null;

        foreach (var type in maintenanceTypes)
        {
            var lastInstance = type.MaintenanceInstances
                .Where(mi => mi.Status == MaintenanceStatus.Completed)
                .OrderByDescending(mi => mi.Date)
                .FirstOrDefault();

            if (lastInstance != null)
            {
                var days = type.Periodicity switch
                {
                    Periodicity.Annual => 365,
                    Periodicity.Semestrial => 180,
                    Periodicity.Quarterly => 90,
                    Periodicity.Monthly => 30,
                    Periodicity.Custom => type.CustomDays ?? 365,
                    _ => 365
                };

                var calculatedDate = lastInstance.Date.AddDays(days);

                if (nextDate == null || calculatedDate < nextDate)
                {
                    nextDate = calculatedDate;
                }
            }
        }

        return nextDate;
    }
}
