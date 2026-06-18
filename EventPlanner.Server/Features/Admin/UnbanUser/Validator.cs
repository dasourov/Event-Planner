using FluentValidation;

namespace EventPlanner.Server.Features.Admin.UnbanUser;

public class UnbanUserValidator : AbstractValidator<UnbanUserCommand>
{
    public UnbanUserValidator()
    {
        RuleFor(x => x.TargetUserId).NotEmpty();
        RuleFor(x => x.AdminUserId).NotEmpty();
    }
}
