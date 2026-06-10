using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Admin.CreateCategory;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;

    public CreateCategoryHandler(ICategoryRepository categoryRepository, IUserRepository userRepository)
    {
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
    }

    public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null || user.Role != UserRole.Admin)
        {
            throw new Exception("Unauthorized. Only admins can create categories.");
        }

        var existing = await _categoryRepository.GetByNameAsync(request.Name);
        if (existing != null)
        {
            throw new Exception("Category with this name already exists.");
        }

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        await _categoryRepository.CreateAsync(category);

        return new CreateCategoryResponse(category.Id, category.Name, category.Description);
    }
}
