using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.UserQuestions.Queries;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LinkPara.Identity.Application.Common.Mappings;
using AutoMapper;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Application.Features.SecurityQuestions.Queries.GetSecurityQuestionById
{
    public class GetSecurityQuestionByIdQuery : IRequest<SecurityQuestionDto>, IMapFrom<SecurityQuestion>
    {
        public Guid Id { get; set; }
    }

    public class GetSecurityQuestionQueryHandler : IRequestHandler<GetSecurityQuestionByIdQuery, SecurityQuestionDto>
    {
        private readonly IRepository<SecurityQuestion> _repository;
        private readonly IMapper _mapper;

        public GetSecurityQuestionQueryHandler(IRepository<SecurityQuestion> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<SecurityQuestionDto> Handle(GetSecurityQuestionByIdQuery request, CancellationToken cancellationToken)
        {
            var securityQuestion = await _repository.GetAll()
                .SingleOrDefaultAsync(p => p.Id == request.Id,
                                       cancellationToken);

            if (securityQuestion is null)
            {
                throw new NotFoundException(nameof(SecurityQuestion));
            }

            return _mapper.Map<SecurityQuestionDto>(securityQuestion);  
        }
    }
}