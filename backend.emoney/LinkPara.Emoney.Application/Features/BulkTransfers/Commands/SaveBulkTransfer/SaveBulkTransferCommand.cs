using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.BulkTransfers.Commands.SaveBulkTransfer;

public class SaveBulkTransferCommand : IRequest
{
    public string FileName { get; set; }
    public string SenderWalletNumber { get; set; }
    public BulkTransferType BulkTransferType { get; set; }
    public List<BulkTransferDetailRequest> BulkTransferDetails { get; set; }
}
public class BulkTransferDetailRequest
{
    public string FullName { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string Description { get; set; }
    public string Receiver { get; set; }
}

public class SaveBulkTransferCommandHandler : IRequestHandler<SaveBulkTransferCommand>
{
    private readonly IGenericRepository<BulkTransfer> _bulkTransferRepository;
    private readonly IGenericRepository<BulkTransferDetail> _bulkTransferDetailRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IAccountService _accountService;

    public SaveBulkTransferCommandHandler(IGenericRepository<BulkTransfer> bulkTransferRepository,
        IGenericRepository<BulkTransferDetail> bulkTransferDetailRepository,
        IContextProvider contextProvider,
        IAccountService accountService)
    {
        _bulkTransferRepository = bulkTransferRepository;
        _bulkTransferDetailRepository = bulkTransferDetailRepository;
        _contextProvider = contextProvider;
        _accountService = accountService;
    }

    public async Task<Unit> Handle(SaveBulkTransferCommand request, CancellationToken cancellationToken)
    {
        var createdUserId = _contextProvider.CurrentContext.UserId;

        bool isDuplicate = request.BulkTransferDetails
               .GroupBy(detail => new { detail.Receiver, detail.Amount, detail.CurrencyCode })
               .Any(group => group.Count() > 1);

        if (isDuplicate)
        {
            throw new DuplicateBulkTransferDetailException();
        }

        var accountUser = await _accountService.GetCorporateAccountUserAsync(Guid.Parse(createdUserId));

        var bulkTransfer = new BulkTransfer
        {
            BulkTransferStatus = BulkTransferStatus.Waiting,
            BulkTransferType = request.BulkTransferType,
            CreatedBy = createdUserId,
            FileName = request.FileName,
            SenderWalletNumber = request.SenderWalletNumber,
            RecordStatus = RecordStatus.Active,
            AccountId = accountUser.AccountId,
            ActionUserName = $"{accountUser.Firstname} {accountUser.Lastname}",
            ActionUser = Guid.Parse(createdUserId),
            ActionDate = DateTime.Now,
        };

        var bulkTransferDetails = new List<BulkTransferDetail>();

        request.BulkTransferDetails.ForEach(bulkTransferDetailRequest =>
        {
            var bulkTransferDetail = new BulkTransferDetail
            {
                Amount = bulkTransferDetailRequest.Amount,
                BulkTransferDetailStatus = BulkTransferDetailStatus.Waiting,
                BulkTransferId = bulkTransfer.Id,
                Description = bulkTransferDetailRequest.Description,
                Receiver = bulkTransferDetailRequest.Receiver,
                FullName = bulkTransferDetailRequest.FullName,
                CurrencyCode = bulkTransferDetailRequest.CurrencyCode,
                CreatedBy = createdUserId,
                RecordStatus = RecordStatus.Active
            };
            bulkTransferDetails.Add(bulkTransferDetail);
        });


        await _bulkTransferRepository.AddAsync(bulkTransfer);
        await _bulkTransferDetailRepository.AddRangeAsync(bulkTransferDetails);

        return Unit.Value;
    }
}
