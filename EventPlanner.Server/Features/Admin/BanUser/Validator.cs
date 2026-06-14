using FluentValidation;

namespace EventPlanner.Server.Features.Admin.BanUser;

public class BanUserValidator : AbstractValidator<BanUserCommand>
{
    public BanUserValidator()
    {
         RuleFor(x => x.TargetUserId)
        .NotEmpty().WithMessage("Target user id is required.")
        .NotEqual(x => x.AdminUserId).WithMessage("Admins cannot ban themselves.");
    }
}
