using System;
using FluentValidation;

namespace EventPlanner.Server.Features.Events.CreateEvent;

public class CreateEventValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Location).NotEmpty();
        RuleFor(x => x.Date).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}
