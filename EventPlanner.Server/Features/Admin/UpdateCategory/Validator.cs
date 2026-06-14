using FluentValidation;

namespace EventPlanner.Server.Features.Admin.UpdateCategory;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
   public UpdateCategoryValidator()
{
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Category id is required.");

    RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Category name is required.")
        .MinimumLength(2).WithMessage("Category name must be at least 2 characters.")
        .MaximumLength(50).WithMessage("Category name must not exceed 50 characters.");

    RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
}
}
