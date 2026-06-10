using FluentValidation;

namespace EventPlanner.Server.Features.Comments.DeleteComment;

public class DeleteCommentValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
    }
}
