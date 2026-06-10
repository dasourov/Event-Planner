using FluentValidation;

namespace EventPlanner.Server.Features.Bookings.LeaveEvent;

public class LeaveEventValidator : AbstractValidator<LeaveEventCommand>
{
    public LeaveEventValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
    }
}
