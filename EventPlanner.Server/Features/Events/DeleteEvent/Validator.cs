using FluentValidation;

namespace EventPlanner.Server.Features.Events.DeleteEvent;

public class DeleteEventValidator : AbstractValidator<DeleteEventCommand>
{
    public DeleteEventValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
