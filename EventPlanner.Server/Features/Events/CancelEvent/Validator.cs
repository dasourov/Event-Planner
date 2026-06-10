using FluentValidation;

namespace EventPlanner.Server.Features.Events.CancelEvent;

public class CancelEventValidator : AbstractValidator<CancelEventCommand>
{
    public CancelEventValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
