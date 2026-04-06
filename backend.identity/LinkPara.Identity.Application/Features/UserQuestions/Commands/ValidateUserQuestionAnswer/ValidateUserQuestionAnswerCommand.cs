using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Identity.Application.Features.UserQuestions.Commands.ValidateUserQuestionAnswer;
public class ValidateUserQuestionAnswerCommand : IRequest<bool>
{
    public string PhoneNumber { get; set; }
    public string Answer { get; set; }
}
public class ValidateUserQuestionAnswerCommandHandler : IRequestHandler<ValidateUserQuestionAnswerCommand, bool>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserSecurityAnswer> _answerRepository;
    private readonly UserManager<User> _userManager;
    private readonly IStringLocalizer _localizer;
    public ValidateUserQuestionAnswerCommandHandler(
        IRepository<User> userRepository,
        IRepository<UserSecurityAnswer> answerRepository,
        UserManager<User> userManager,
        IStringLocalizerFactory factory)
    {
        _userRepository = userRepository;
        _answerRepository = answerRepository;
        _userManager = userManager;
        _localizer = factory.Create("Exceptions", "LinkPara.Identity.API");
    }
    public async Task<bool> Handle(ValidateUserQuestionAnswerCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetAll()
            .FirstOrDefaultAsync(x => x.PhoneNumber == command.PhoneNumber &&
                                      x.UserType == UserType.Individual &&
                                      x.UserStatus != UserStatus.Inactive,
                                      cancellationToken);
        if (user is null)
        {
            throw new NotFoundException(nameof(User), command.PhoneNumber);
        }

        var userSecurityAnswer = await _answerRepository
            .GetAll()
            .FirstOrDefaultAsync(p => p.UserId == user.Id,
            cancellationToken);

        if (userSecurityAnswer is null)
        {
            throw new NotFoundException(nameof(UserSecurityAnswer), command.Answer);
        }

        var passwordVerificationResult = _userManager.PasswordHasher
            .VerifyHashedPassword(user, userSecurityAnswer.AnswerHash, command.Answer);

        if (passwordVerificationResult != PasswordVerificationResult.Success)
        {
            var exceptionMessage = _localizer.GetString(nameof(UserSecurityQuestionAnswerException));
            throw new UserSecurityQuestionAnswerException(exceptionMessage);
        }
        return true;
    }
}
