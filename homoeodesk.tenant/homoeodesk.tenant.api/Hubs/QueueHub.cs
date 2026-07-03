using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using HomoeoDesk.Tenant.Application.Common.Interfaces;

namespace HomoeoDesk.Tenant.Api.Hubs;

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

    public async Task JoinDoctorQueue(int doctorId, int branchId)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();
        var groupName = $"Queue_{organizationId}_{branchId}_{doctorId}";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserJoined", Context.User?.Identity?.Name);
    }

    public async Task LeaveDoctorQueue(int doctorId, int branchId)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();
        var groupName = $"Queue_{organizationId}_{branchId}_{doctorId}";
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserLeft", Context.User?.Identity?.Name);
    }

    public async Task JoinBranchUpdates(int branchId)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();
        var groupName = $"Branch_{organizationId}_{branchId}";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up any groups the user was part of
        await base.OnDisconnectedAsync(exception);
    }
}
