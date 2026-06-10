using FluentValidation;

namespace EventPlanner.Server.Features.Admin.BanUser;

public class BanUserValidator : AbstractValidator<BanUserCommand>
{
    public BanUserValidator()
    {
        RuleFor(x => x.TargetUserId).NotEmpty();
    }
}
