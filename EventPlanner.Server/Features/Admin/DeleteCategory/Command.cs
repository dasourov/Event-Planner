using MediatR;

namespace EventPlanner.Server.Features.Admin.DeleteCategory;

public record DeleteCategoryCommand(string Id, string UserId) : IRequest<DeleteCategoryResponse>;
