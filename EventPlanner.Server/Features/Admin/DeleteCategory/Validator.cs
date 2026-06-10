using FluentValidation;

namespace EventPlanner.Server.Features.Admin.DeleteCategory;

public class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
