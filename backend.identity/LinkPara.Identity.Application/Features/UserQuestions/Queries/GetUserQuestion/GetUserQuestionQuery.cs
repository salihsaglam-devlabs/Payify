using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.UserQuestions.Queries.GetUserQuestion;

public class GetUserQuestionQuery : IRequest<SecurityQuestionDto>
{
    public Guid UserId { get; set; }
}

public class GetUserQuestionQueryHandler : IRequestHandler<GetUserQuestionQuery, SecurityQuestionDto>
{
    private readonly IRepository<UserSecurityAnswer> _repository;

    public GetUserQuestionQueryHandler(IRepository<UserSecurityAnswer> repository)
    {
        _repository = repository;
    }
    
    public async Task<SecurityQuestionDto> Handle(GetUserQuestionQuery request, CancellationToken cancellationToken)
    {
        var userSecurityAnswer = await _repository.GetAll()
            .Include(q=> q.SecurityQuestion)
            .SingleOrDefaultAsync(p => p.UserId == request.UserId
                                    && p.RecordStatus == RecordStatus.Active, 
                                   cancellationToken);
        
        if (userSecurityAnswer is null)
        {
            throw new NotFoundException(nameof(UserSecurityAnswer));
        }

        return new SecurityQuestionDto
        {
            Id= userSecurityAnswer.SecurityQuestionId,
            Question = userSecurityAnswer.SecurityQuestion.Question,
            LanguageCode = userSecurityAnswer.SecurityQuestion.LanguageCode
        };
    }
}