using FluentValidation;

namespace EventPlanner.Server.Features.Events.PublishEvent;

public class PublishEventValidator : AbstractValidator<PublishEventCommand>
{
    public PublishEventValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
