using System.Collections.Generic;
using MediatR;

namespace EventPlanner.Server.Features.Admin.GetUsers;

public record GetUsersQuery(string UserId) : IRequest<List<GetUsersResponse>>;
