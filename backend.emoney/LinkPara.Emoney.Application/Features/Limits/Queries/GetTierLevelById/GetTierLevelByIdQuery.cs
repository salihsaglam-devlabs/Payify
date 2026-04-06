using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Limits.Queries.GetTierLevelById
{
    public class GetTierLevelByIdQuery : IRequest<TierLevelDto>
    {
        public Guid Id { get; set; }
    }

    public class GetTierLevelByIdQueryHandler : IRequestHandler<GetTierLevelByIdQuery, TierLevelDto>
    {
        private readonly IGenericRepository<TierLevel> _repository;
        private readonly IGenericRepository<AccountCustomTier> _accountCustomTierRepository;
        private readonly IMapper _mapper;
        public GetTierLevelByIdQueryHandler(
            IGenericRepository<TierLevel> repository,
            IMapper mapper,
            IGenericRepository<AccountCustomTier> accountCustomTierRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _accountCustomTierRepository = accountCustomTierRepository;   
        }

        public async Task<TierLevelDto> Handle(GetTierLevelByIdQuery request, CancellationToken cancellationToken)
        {
            var tierLevel = await _repository.GetAll(x => x.Currency)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (tierLevel == null)
            {
                throw new NotFoundException(nameof(TierLevel), request.Id);
            }

            var resModel = _mapper.Map<TierLevelDto>(tierLevel);

            var accountCustomtierLevelList = await _accountCustomTierRepository.GetAll()
                .Where(x => x.TierLevelId == request.Id
                && x.RecordStatus == RecordStatus.Active)
                .Include(x => x.Account)
                .Select(x=> new AccountCustomTierDto
                {
                    Id = x.Id,
                    AccountId = x.AccountId,
                    TierLevelId = x.TierLevelId,
                    AccountName = x.AccountName,
                    PhoneNumber = x.PhoneNumber,
                    PhoneCode = x.PhoneCode,
                    Email = x.Email,
                    AccountType = x.Account.AccountType,
                })
                .ToListAsync(cancellationToken);

            resModel.AccountCustomTierList = accountCustomtierLevelList;

            return resModel;
        }
    }
}
