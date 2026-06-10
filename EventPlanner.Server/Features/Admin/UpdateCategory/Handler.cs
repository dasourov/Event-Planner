using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Admin.UpdateCategory;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;

    public UpdateCategoryHandler(ICategoryRepository categoryRepository, IUserRepository userRepository)
    {
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
    }

    public async Task<UpdateCategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null || user.Role != UserRole.Admin)
        {
            throw new Exception("Unauthorized. Only admins can update categories.");
        }

        var category = await _categoryRepository.GetByIdAsync(request.Id);
        if (category == null)
        {
            throw new Exception("Category not found");
        }

        category.Name = request.Name;
        category.Description = request.Description;

        await _categoryRepository.UpdateAsync(category);

        return new UpdateCategoryResponse(category.Id, category.Name, category.Description);
    }
}
