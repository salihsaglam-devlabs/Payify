using LinkPara.ContextProvider;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.Payments.Commands.Return;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantReturnPools.Command.ActionMerchantReturnPool
{
    public class ActionMerchantReturnPoolCommand : IRequest<ReturnResponse>
    {
        public Guid MerchantReturnPoolId { get; set; }
        public ReturnStatus ReturnStatus { get; set; }
        public string RejectDescription { get; set; }
        public string RejectReason { get; set; }
    }

    public class ActionMerchantReturnPoolCommandHandler : IRequestHandler<ActionMerchantReturnPoolCommand, ReturnResponse>
    {
        private readonly IReturnService _returnService;
        private readonly IMerchantReturnPoolService _merchantReturnPoolService;
        private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
        private readonly IContextProvider _contextProvider;

        public ActionMerchantReturnPoolCommandHandler(IReturnService returnService,
            IMerchantReturnPoolService merchantReturnPoolService,
            IGenericRepository<MerchantTransaction> merchantTransactionRepository,
            IContextProvider contextProvider)
        {
            _returnService = returnService;
            _merchantReturnPoolService = merchantReturnPoolService;
            _merchantTransactionRepository = merchantTransactionRepository;
            _contextProvider = contextProvider;
        }

        public async Task<ReturnResponse> Handle(ActionMerchantReturnPoolCommand request, CancellationToken cancellationToken)
        {
            var merchantReturnPoolDto = await _merchantReturnPoolService.GetByIdAsync(request.MerchantReturnPoolId);

            ReturnResponse response;
            
            if (request.ReturnStatus == ReturnStatus.Approved)
            {
                response = await _returnService.ReturnAsync(new ReturnCommand
                {
                    MerchantId = merchantReturnPoolDto.MerchantId,
                    ConversationId = merchantReturnPoolDto.ConversationId,
                    OrderId = merchantReturnPoolDto.OrderId,
                    Amount = merchantReturnPoolDto.Amount,
                    ClientIpAddress = merchantReturnPoolDto.ClientIpAddress,
                    LanguageCode = merchantReturnPoolDto.LanguageCode,
                    IsAdminApproved = true,
                    MerchantReturnPoolId = request.MerchantReturnPoolId,
                    IsTopUpPayment = merchantReturnPoolDto.IsTopUpPayment
                });

                if (response.IsSucceed)
                {
                    await _merchantReturnPoolService.UpdateStatusAsync(request.MerchantReturnPoolId, ReturnStatus.Approved, response, request.RejectDescription, request.RejectReason);
                }
                else
                {
                    await _merchantReturnPoolService.UpdateStatusAsync(request.MerchantReturnPoolId, ReturnStatus.Pending, response, request.RejectDescription, request.RejectReason);
                }
            }
            else
            {
                await _merchantReturnPoolService.UpdateStatusAsync(request.MerchantReturnPoolId, ReturnStatus.Rejected, null, request.RejectDescription, request.RejectReason);

                var referenceMerchantTransaction = await _merchantTransactionRepository.GetAll()
                       .FirstOrDefaultAsync(s =>
                          s.RecordStatus == RecordStatus.Active &&
                          (s.TransactionStatus != TransactionStatus.Fail && s.TransactionStatus != TransactionStatus.Returned) &&
                          s.OrderId == merchantReturnPoolDto.OrderId &&
                          s.MerchantId == merchantReturnPoolDto.MerchantId &&
                          !s.IsReverse, cancellationToken);

                referenceMerchantTransaction.ReturnStatus = ReturnStatus.Rejected;
                referenceMerchantTransaction.LastModifiedBy = _contextProvider.CurrentContext.UserId;
                referenceMerchantTransaction.UpdateDate = DateTime.Now;

                await _merchantTransactionRepository.UpdateAsync(referenceMerchantTransaction);
                
                response = new ReturnResponse
                {
                    IsSucceed = true,
                    ErrorCode = null,
                    ErrorMessage = null,
                    ConversationId = merchantReturnPoolDto.ConversationId,
                    ApprovalStatus = request.ReturnStatus == ReturnStatus.Rejected
                        ? ReturnApprovalStatus.None
                        : ReturnApprovalStatus.PendingApproval
                };
            }
            
            if (response.IsSucceed)
            {
                var actionMessage = "Action" + request.ReturnStatus.ToString();

                if (request.ReturnStatus == ReturnStatus.Approved)
                {
                    response.ReturnMessage = merchantReturnPoolDto.LanguageCode.ToUpper() != "TR" 
                        ? actionMessage
                        : "İade talebi onaylandı.";
                }
                else if (request.ReturnStatus == ReturnStatus.Rejected)
                {
                    response.ReturnMessage = merchantReturnPoolDto.LanguageCode.ToUpper() != "TR"
                        ? actionMessage
                        : "İade talebi reddedildi.";
                }
            }

            return response;
        }
    }
}