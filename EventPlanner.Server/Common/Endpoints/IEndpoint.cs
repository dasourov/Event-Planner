using Microsoft.AspNetCore.Routing;

namespace EventPlanner.Server.Common.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
