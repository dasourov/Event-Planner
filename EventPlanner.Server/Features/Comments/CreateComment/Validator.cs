using FluentValidation;

namespace EventPlanner.Server.Features.Comments.CreateComment;

public class CreateCommentValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(500);
    }
}
