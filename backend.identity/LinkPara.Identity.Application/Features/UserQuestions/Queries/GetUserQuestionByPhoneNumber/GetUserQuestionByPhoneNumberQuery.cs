using AutoMapper;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.UserQuestions.Queries.GetUserQuestionByPhoneNumber;
public class GetUserQuestionByPhoneNumberQuery : IRequest<string>
{
    public string PhoneNumber { get; set; }
}
public class GetUserQuestionByPhoneNumberQueryHandler : IRequestHandler<GetUserQuestionByPhoneNumberQuery, string>
{
    private readonly IRepository<UserSecurityAnswer> _answerRepository;
    private readonly IRepository<User> _userRepository;
    public GetUserQuestionByPhoneNumberQueryHandler(
        IRepository<User> userRepository,
        IRepository<UserSecurityAnswer> answerRepository)
    {
        _userRepository = userRepository;
        _answerRepository = answerRepository;
    }
    public async Task<string> Handle(GetUserQuestionByPhoneNumberQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetAll()
            .FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber &&
                                      x.UserType == UserType.Individual &&
                                      x.UserStatus != UserStatus.Inactive,
                                      cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.PhoneNumber);
        }

        var userSecurityAnswer = await _answerRepository
            .GetAll()
            .Include(q => q.SecurityQuestion)
            .FirstOrDefaultAsync(p => p.UserId == user.Id &&
                                      p.RecordStatus == RecordStatus.Active,
                                      cancellationToken);

        if (userSecurityAnswer is null)
        {
            throw new NotFoundException(nameof(UserSecurityAnswer));
        }

        return userSecurityAnswer.SecurityQuestion.Question;
    }
}