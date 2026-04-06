using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.UserQuestions.Queries.GetAllQuestions;

public class GetAllQuestionsQuery : IRequest<List<SecurityQuestionDto>>
{
}

public class GetQuestionsQueryHandler : IRequestHandler<GetAllQuestionsQuery, List<SecurityQuestionDto>>
{
    private readonly IRepository<SecurityQuestion> _securityQuestionRepository;
    private readonly IMapper _mapper;
    private readonly IContextProvider _contextProvider;

    public GetQuestionsQueryHandler(IRepository<SecurityQuestion> securityQuestionRepository,
        IMapper mapper,
        IContextProvider contextProvider)
    {
        _securityQuestionRepository = securityQuestionRepository;
        _mapper = mapper;
        _contextProvider = contextProvider;
    }

    public async Task<List<SecurityQuestionDto>> Handle(GetAllQuestionsQuery request,
        CancellationToken cancellationToken)
    {
        var languageCode = string.IsNullOrEmpty(_contextProvider.CurrentContext.Language)
                            ? "tr"
                            : _contextProvider.CurrentContext.Language.Substring(0, 2);

        var entities = 
            await _securityQuestionRepository.GetAll().Where(x=>x.RecordStatus==RecordStatus.Active && x.LanguageCode.ToLower() == languageCode.ToLower()).
            ToListAsync(cancellationToken);

        return _mapper.Map<List<SecurityQuestion>, List<SecurityQuestionDto>>(entities);
    }
}