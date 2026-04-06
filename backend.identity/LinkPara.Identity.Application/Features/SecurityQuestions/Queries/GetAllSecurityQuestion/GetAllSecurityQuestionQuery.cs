using AutoMapper;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using LinkPara.Identity.Application.Features.UserQuestions.Queries;
using AutoMapper.QueryableExtensions;
using LinkPara.MappingExtensions.Mapping;

namespace LinkPara.Identity.Application.Features.SecurityQuestions.Queries.GetAllSecurityQuestion;

public class GetAllSecurityQuestionQuery : SearchQueryParams, IRequest<PaginatedList<SecurityQuestionDto>>
{
    public RecordStatus? RecordStatus { get; set; }
    public string LanguageCode { get; set; }
    public string Question { get; set; }

}

public class GetAllSecurityQuestionQueryHandler : IRequestHandler<GetAllSecurityQuestionQuery, PaginatedList<SecurityQuestionDto>>
{
    private readonly IRepository<SecurityQuestion> _questionRepository;
    private readonly IMapper _mapper;

    public GetAllSecurityQuestionQueryHandler(IRepository<AgreementDocument> repository,
       IMapper mapper,
       IRepository<SecurityQuestion> questionRepository,
       IHttpContextAccessor httpContextAccessor)
    {
        _questionRepository = questionRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<SecurityQuestionDto>> Handle(GetAllSecurityQuestionQuery request, CancellationToken cancellationToken)
    {
        var questionList = _questionRepository.GetAll();

        if (request.RecordStatus.HasValue)
        {
            questionList = questionList.Where(x => x.RecordStatus == request.RecordStatus);
        }
        if (!String.IsNullOrEmpty(request.Question))
        {
            questionList = questionList.Where(x => x.Question.Contains(request.Question));
        }
        if (!String.IsNullOrEmpty(request.LanguageCode))
        {
            questionList = questionList.Where(x => x.LanguageCode.ToLower() == request.LanguageCode.ToLower());
        }

        return await questionList
            .PaginatedListWithMappingAsync<SecurityQuestion,SecurityQuestionDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
