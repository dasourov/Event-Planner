using MediatR;

namespace EventPlanner.Server.Features.Admin.UpdateCategory;

public record UpdateCategoryCommand(string Id, string Name, string Description, string UserId) : IRequest<UpdateCategoryResponse>;
