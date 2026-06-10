using System.Collections.Generic;
using MediatR;

namespace EventPlanner.Server.Features.Categories.GetCategories;

public record GetCategoriesQuery() : IRequest<List<GetCategoriesResponse>>;
