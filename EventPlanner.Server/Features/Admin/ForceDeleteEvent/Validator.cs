using FluentValidation;

namespace EventPlanner.Server.Features.Admin.ForceDeleteEvent;

public class ForceDeleteEventValidator : AbstractValidator<ForceDeleteEventCommand>
{
    public ForceDeleteEventValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
