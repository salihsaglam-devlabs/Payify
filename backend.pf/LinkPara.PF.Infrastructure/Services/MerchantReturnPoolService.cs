using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.MerchantTransactions;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.MerchantReturnPools;
using LinkPara.PF.Application.Features.MerchantReturnPools.Queries.GetMerchantReturnPools;
using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Services
{

    public class MerchantReturnPoolService : IMerchantReturnPoolService
    {
        private readonly IGenericRepository<MerchantReturnPool> _repository;
        private readonly IContextProvider _contextProvider;
        private readonly IMerchantTransactionService _merchantTransactionService;
        private readonly IMapper _mapper;

        public MerchantReturnPoolService(IGenericRepository<MerchantReturnPool> repository,
            IMerchantTransactionService merchantTransactionService,
            IMapper mapper,
            IContextProvider contextProvider)
        {
            _repository = repository;
            _merchantTransactionService = merchantTransactionService;
            _mapper = mapper;
            _contextProvider = contextProvider;
        }

        public async Task AddAsync(MerchantReturnPoolDto req)
        {
            await _repository.AddAsync(new MerchantReturnPool
            {
                ActionDate = req.ActionDate,
                MerchantId = req.MerchantId,
                ActionUser = req.ActionUser,
                Amount = req.Amount,
                ClientIpAddress = req.ClientIpAddress,
                ConversationId = req.ConversationId,
                LanguageCode = req.LanguageCode,
                OrderId = req.OrderId,
                RecordStatus = RecordStatus.Active,
                ReturnStatus = req.ReturnStatus,
                CardNumber = req.CardNumber,
                BankCode = req.BankCode,
                BankName = req.BankName,
                CurrencyCode = req.CurrencyCode,
                CreateDate = DateTime.Now,
                CreatedBy = req.MerchantId.ToString(),
                IsTopUpPayment = req.IsTopUpPayment
            });
        }

        public async Task<MerchantReturnPoolDto> UpdateStatusAsync(Guid merchantReturnPoolId,
            ReturnStatus returnStatus, ReturnResponse returnResponse, string rejectDescription, string rejectReason)
        {
            var merchantReturnPool = await _repository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == merchantReturnPoolId);

            if (merchantReturnPool is null)
            {
                throw new NotFoundException(nameof(MerchantReturnPool));
            }

            merchantReturnPool.ReturnStatus = returnStatus;
            merchantReturnPool.ActionDate = DateTime.Now;
            merchantReturnPool.RejectDescription = rejectDescription;
            merchantReturnPool.RejectReason = rejectReason;
            merchantReturnPool.ActionUser = Guid.Parse(_contextProvider.CurrentContext.UserId);
            merchantReturnPool.UpdateDate = DateTime.Now;
            merchantReturnPool.LastModifiedBy = _contextProvider.CurrentContext.UserId.ToString();
            merchantReturnPool.BankStatus = returnResponse?.IsSucceed;
            merchantReturnPool.BankResponseCode = returnResponse?.ResponseCode;
            merchantReturnPool.BankResponseDescription = returnResponse?.ResponseMessage;

            await _repository.UpdateAsync(merchantReturnPool);

            return _mapper.Map<MerchantReturnPoolDto>(merchantReturnPool);
        }

        public async Task<List<MerchantReturnPoolDto>> GetMerchantReturnPoolByOrderIdAsync(string orderId)
        {
            var merchantReturnPool = await _repository.GetAll()
                .Where(s => s.OrderId == orderId)
                .ToListAsync();

            return _mapper.Map<List<MerchantReturnPoolDto>>(merchantReturnPool);
        }

        public async Task<MerchantReturnPoolDto> GetByIdAsync(Guid id)
        {
            var merchantReturnPool = await _repository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == id);

            return _mapper.Map<MerchantReturnPoolDto>(merchantReturnPool);
        }

        public async Task<List<MerchantTransactionDto>> GetMerchantMonthlyReturnTransactionsAsync(Guid merchantId)
        {
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            
            var merchantTransactions = await _merchantTransactionService
                .GetMerchantTransactionsAsync(new MerchantTransactionRequest
                {
                    MerchantId = merchantId,
                    StartDate = firstDayOfMonth,
                    EndDate = DateTime.Now,
                    TransactionType = TransactionType.Return
                });

            return merchantTransactions;
        }

        public async Task<List<MerchantReturnPoolDto>> GetPendingPoolAsync()
        {
            var merchantReturnPool = await _repository.GetAll()
                .Where(s => s.ReturnStatus == ReturnStatus.Pending)
                .ToListAsync();

            return _mapper.Map<List<MerchantReturnPoolDto>>(merchantReturnPool);
        }

        public async Task<PaginatedList<MerchantReturnPoolDto>> GetPaginatedPendingPoolAsync(GetMerchantReturnPoolsQuery request)
        {
            var transactions = _repository.GetAll();

            if (!string.IsNullOrEmpty(request.ConversationId))
            {
                transactions = transactions.Where(b => b.ConversationId.Contains(request.ConversationId));
            }

            if (request.MerchantId is not null)
            {
                transactions = transactions.Where(b => b.MerchantId == request.MerchantId);
            }

            if (request.ReturnStatus is not null)
            {
                transactions = transactions.Where(b => b.ReturnStatus == request.ReturnStatus);
            }

            if (request.ActionDate is not null)
            {
                transactions = transactions.Where(b => b.ActionDate == request.ActionDate);
            }

            if (request.ActionUser is not null)
            {
                transactions = transactions.Where(b => b.ActionUser == request.ActionUser);
            }

            if (request.CreateDateStart is not null)
            {
                transactions = transactions.Where(b => b.CreateDate
                                   >= request.CreateDateStart);
            }

            if (request.CreateDateEnd is not null)
            {
                transactions = transactions.Where(b => b.CreateDate
                                   <= request.CreateDateEnd);
            }
            if (!string.IsNullOrEmpty(request.FirstCardNumber))
            {
                transactions = transactions.Where(b => b.CardNumber.Substring(0, 6).Contains(request.FirstCardNumber));
            }

            if (!string.IsNullOrEmpty(request.LastCardNumber))
            {
                transactions = transactions.Where(b => b.CardNumber.Substring(12, 4).Contains(request.LastCardNumber));
            }
            if (request.BankCode is not null)
            {
                transactions = transactions.Where(b => b.BankCode == request.BankCode);
            }
            if (!string.IsNullOrEmpty(request.BankName))
            {
                transactions = transactions.Where(b => b.BankName.Contains(request.BankName));
            }
            if (!string.IsNullOrEmpty(request.OrderId))
            {
                transactions = transactions.Where(b => b.OrderId.Contains(request.OrderId));
            }

            if (request.BankStatus is not null)
            {
                transactions = transactions.Where(b => b.BankStatus == request.BankStatus);
            }

            return await transactions.Include(p => p.Merchant)
                .PaginatedListWithMappingAsync<MerchantReturnPool,MerchantReturnPoolDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
        }
    }
}