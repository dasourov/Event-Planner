using System;
using FluentValidation;

namespace EventPlanner.Server.Features.Events.CreateEvent;

public class CreateEventValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty();

        RuleFor(x => x.Location)
            .NotEmpty();

        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.Date)
            .Must(BeInFuture)
            .WithMessage("Date must be in the future.");

        RuleFor(x => x.MaxAttendees)
            .GreaterThan(0)
            .When(x => x.MaxAttendees.HasValue);
    }

    private static bool BeInFuture(DateTime date)
    {
        return date.ToUniversalTime() > DateTime.UtcNow.AddMinutes(-1);
    }
}