using AutoMapper;
using LinkPara.PF.Application.Commons.Models.DeductionTransactions;
using LinkPara.PF.Application.Commons.Models.MerchantDeductions;
using LinkPara.PF.Application.Features.MerchantDues;
using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Application.Features.PostingBalances;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantDeductions.Queries.GetMerchantDeductionByIdQuery
{
    public class GetMerchantDeductionByIdQuery : IRequest<DeductionDetailsResponse>
    {
        public Guid Id { get; set; }
    }

    public class GetMerchantDeductionByIdQueryHandler : IRequestHandler<GetMerchantDeductionByIdQuery, DeductionDetailsResponse>
    {
        private readonly IGenericRepository<MerchantDeduction> _merchantDeductionRepository;
        private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
        private readonly IGenericRepository<MerchantDue> _merchantDueRepository;
        private readonly IGenericRepository<DeductionTransaction> _deductionTransactionRepository;
        private readonly IGenericRepository<PostingBalance> _postingBalanceRepository;
        private readonly IMapper _mapper;

        public GetMerchantDeductionByIdQueryHandler(
            IGenericRepository<MerchantDeduction> merchantDeductionRepository,
            IGenericRepository<MerchantTransaction> merchantTransactionRepository,
            IGenericRepository<MerchantDue> merchantDueRepository,
            IGenericRepository<DeductionTransaction> deductionTransactionRepository,
            IGenericRepository<PostingBalance> postingBalanceRepository,
            IMapper mapper)
        {
            _merchantDeductionRepository = merchantDeductionRepository;
            _merchantTransactionRepository = merchantTransactionRepository;
            _merchantDueRepository = merchantDueRepository;
            _deductionTransactionRepository = deductionTransactionRepository;
            _postingBalanceRepository = postingBalanceRepository;
            _mapper = mapper;
        }
        public async Task<DeductionDetailsResponse> Handle(GetMerchantDeductionByIdQuery request, CancellationToken cancellationToken)
        {
            var merchantDeduction = await _merchantDeductionRepository.GetAll()
                .FirstOrDefaultAsync(d => d.Id == request.Id && d.RecordStatus == RecordStatus.Active, cancellationToken: cancellationToken);

            if (merchantDeduction is null)
            {
                throw new NotFoundException(nameof(merchantDeduction), request.Id);
            }
            
            var deductionTransactions = await _deductionTransactionRepository.GetAll()
                .Where(d => d.MerchantDeductionId == merchantDeduction.Id && d.RecordStatus == RecordStatus.Active)
                .ToListAsync(cancellationToken: cancellationToken);

            var merchantDue = await _merchantDueRepository.GetAll()
                .Include(d => d.DueProfile)
                .Where(d => d.Id == merchantDeduction.MerchantDueId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            var merchantTransaction = await _merchantTransactionRepository.GetAll()
                .Where(t => t.Id == merchantDeduction.MerchantTransactionId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            
            var postingBalance = await _postingBalanceRepository.GetAll()
                .Where(t => t.Id == merchantDeduction.PostingBalanceId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            var relatedDeductions = new List<MerchantDeduction>();
            
            var merchantTransactionNotRelatedTypes = new List<DeductionType>
            {
                DeductionType.Due, 
                DeductionType.ExcessReturn,
                DeductionType.DueTransfer,
                DeductionType.ExcessReturnTransfer,
                DeductionType.ExcessReturnOnCommission
            };

            if (!merchantTransactionNotRelatedTypes.Contains(merchantDeduction.DeductionType))
            {
                relatedDeductions = await _merchantDeductionRepository.GetAll()
                    .Where(s => s.MerchantTransactionId == merchantDeduction.MerchantTransactionId && s.Id != merchantDeduction.Id)
                    .OrderByDescending(s => s.CreateDate)
                    .ToListAsync(cancellationToken: cancellationToken);
            }

            if ((merchantDeduction.DeductionType is DeductionType.Due or DeductionType.ExcessReturn) && 
                (merchantDeduction.DeductionStatus is DeductionStatus.Transferred or DeductionStatus.PartialTransfer))
            {
                relatedDeductions.Add(await _merchantDeductionRepository.GetAll()
                    .Where(s => s.SubMerchantDeductionId == merchantDeduction.Id).FirstOrDefaultAsync(cancellationToken: cancellationToken));
            }

            var deductionDetailsResponse = new DeductionDetailsResponse()
            {
                MerchantDeduction = _mapper.Map<MerchantDeductionDto>(merchantDeduction),
                MerchantDue = (merchantDue is not null) ? _mapper.Map<MerchantDueDto>(merchantDue) : null,
                MerchantTransaction = (merchantTransaction is not null) ? _mapper.Map<MerchantTransactionDto>(merchantTransaction) : null,
                PostingBalance = (postingBalance is not null) ? _mapper.Map<PostingBalanceDto>(postingBalance) : null,
                Transactions = _mapper.Map<List<DeductionTransactionDto>>(deductionTransactions),
                RelatedDeductions = _mapper.Map<List<MerchantDeductionDto>>(relatedDeductions)
            };

            return deductionDetailsResponse;
        }
    }
}
