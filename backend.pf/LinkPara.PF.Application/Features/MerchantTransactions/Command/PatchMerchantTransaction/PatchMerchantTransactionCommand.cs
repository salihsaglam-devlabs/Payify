using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Commons.Models.MerchantTransactions;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.PF.Application.Features.MerchantTransactions.Command.PatchMerchantTransaction
{
    public class PatchMerchantTransactionCommand : IRequest<UpdateMerchantTransactionRequest>
    {
        public Guid Id { get; set; }
        public JsonPatchDocument<UpdateMerchantTransactionRequest> MerchantTransaction { get; set; }
        public List<MerchantDocumentDto> Files { get; set; }
    }

    public class PatchMerchantTransactionCommandHandler : IRequestHandler<PatchMerchantTransactionCommand, UpdateMerchantTransactionRequest>
    {
        private readonly IMerchantTransactionService _merchantTransactionService;

        public PatchMerchantTransactionCommandHandler(IMerchantTransactionService merchantTransactionService)
        {
            _merchantTransactionService = merchantTransactionService;
        }

        public async Task<UpdateMerchantTransactionRequest> Handle(PatchMerchantTransactionCommand request, CancellationToken cancellationToken)
        {
            request.MerchantTransaction.Operations.RemoveAll(x => x.path.Contains("files"));

            return await _merchantTransactionService.PatchAsync(request);
        }
    }
}
