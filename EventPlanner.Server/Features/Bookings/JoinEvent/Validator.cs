using FluentValidation;

namespace EventPlanner.Server.Features.Bookings.JoinEvent;

public class JoinEventValidator : AbstractValidator<JoinEventCommand>
{
    public JoinEventValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
    }
}
