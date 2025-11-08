using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using ClinicCare.Application.Common.Interfaces;

namespace ClinicCare.API.Hubs;

[Authorize]
public class QueueHub : Hub
{
    private readonly ITenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public QueueHub(ITenantService tenantService, ICurrentUserService currentUserService)
    {
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task JoinDoctorQueue(int doctorId, int clinicId)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();
        var groupName = $"Queue_{organizationId}_{clinicId}_{doctorId}";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserJoined", Context.User?.Identity?.Name);
    }

    public async Task LeaveDoctorQueue(int doctorId, int clinicId)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();
        var groupName = $"Queue_{organizationId}_{clinicId}_{doctorId}";
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserLeft", Context.User?.Identity?.Name);
    }

    public async Task JoinClinicUpdates(int clinicId)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();
        var groupName = $"Clinic_{organizationId}_{clinicId}";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up any groups the user was part of
        await base.OnDisconnectedAsync(exception);
    }
}
