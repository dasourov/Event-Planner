using FluentValidation;

namespace EventPlanner.Server.Features.Admin.ForceDeleteComment;

public class ForceDeleteCommentValidator : AbstractValidator<ForceDeleteCommentCommand>
{
    public ForceDeleteCommentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
