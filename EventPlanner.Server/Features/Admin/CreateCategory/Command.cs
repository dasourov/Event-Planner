using MediatR;

namespace EventPlanner.Server.Features.Admin.CreateCategory;

public record CreateCategoryCommand(string Name, string Description, string UserId) : IRequest<CreateCategoryResponse>;
