using System.Collections.Generic;
using MediatR;

namespace EventPlanner.Server.Features.Comments.GetEventComments;

public record GetEventCommentsQuery(string EventId) : IRequest<List<GetEventCommentsResponse>>;
