using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Categories.GetCategories;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, List<GetCategoriesResponse>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<GetCategoriesResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.ListAsync();
        return categories.Select(c => new GetCategoriesResponse(c.Id, c.Name, c.Description)).ToList();
    }
}
