using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Identity.Application.Features.UserQuestions.Commands.ValidateUserAnswer;

public class ValidateUserAnswerCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public string Answer { get; set; }
}

public class ValidateUserAnswerCommandHandler : IRequestHandler<ValidateUserAnswerCommand, bool>
{
    private readonly UserManager<User> _userManager;
    private readonly IRepository<UserSecurityAnswer> _repository;
    private readonly IStringLocalizer _localizer;
    public ValidateUserAnswerCommandHandler(
        UserManager<User> userManager,
        IRepository<UserSecurityAnswer> repository,
        IStringLocalizerFactory factory)
    {
        _userManager = userManager;
        _repository = repository;
        _localizer = factory.Create("Exceptions", "LinkPara.Identity.API");
    }

    public async Task<bool> Handle(ValidateUserAnswerCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), command.UserId);
        }

        var userSecurityAnswer = await _repository.GetAll()
            .FirstOrDefaultAsync(p => p.UserId == command.UserId, cancellationToken);

        if (userSecurityAnswer is null)
        {
            throw new NotFoundException(nameof(UserSecurityAnswer), command.Answer);
        }

        var passwordVerificationResult =
            _userManager.PasswordHasher.VerifyHashedPassword(user, userSecurityAnswer.AnswerHash,
                command.Answer);

        if (passwordVerificationResult != PasswordVerificationResult.Success)
        {
            var exceptionMessage = _localizer.GetString(nameof(UserSecurityQuestionAnswerException));
            throw new UserSecurityQuestionAnswerException(exceptionMessage);
        }

        return true;
    }
}