using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace EventPlanner.Server.Infrastructure.SignalR;

public class CommentHub : Hub
{
    public async Task JoinEventGroup(string eventId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, eventId);
    }

    public async Task LeaveEventGroup(string eventId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, eventId);
    }
}
