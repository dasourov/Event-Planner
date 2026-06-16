using Microsoft.AspNetCore.SignalR;
using Moq;
using EventPlanner.Server.Infrastructure.SignalR;

namespace EventPlanner.UnitTests;

// Shared helper for mocking the SignalR CommentHub context in handler tests
public static class CommentHubMocks
{
    public static (Mock<IHubContext<CommentHub>> HubContext, Mock<IClientProxy> ClientProxy) Create()
    {
        var clientProxy = new Mock<IClientProxy>();
        var clients = new Mock<IHubClients>();
        clients.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxy.Object);

        var hubContext = new Mock<IHubContext<CommentHub>>();
        hubContext.Setup(h => h.Clients).Returns(clients.Object);

        return (hubContext, clientProxy);
    }
}
