using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.BusinessParameter.Models;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.ManualTransfer.Queries.GetAllManualTransfers;

public class GetAllManualTransfersQuery : SearchQueryParams, IRequest<PaginatedList<ManualTransferDto>>
{
    public TransactionType? TransactionType { get; set; }
    public string WalletNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetAllManualTransferQueryHandler : IRequestHandler<GetAllManualTransfersQuery, PaginatedList<ManualTransferDto>>
{
    private readonly IGenericRepository<Transaction> _transactionRepository;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IGenericRepository<ManualTransferReference> _manualTransferReferenceRepository;
    private readonly IParameterService _parameterService;

    public GetAllManualTransferQueryHandler(IGenericRepository<Transaction> transactionRepository, IGenericRepository<Wallet> walletRepository, IGenericRepository<ManualTransferReference> manualTransferReferenceRepository, IParameterService parameterService)
    {
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
        _manualTransferReferenceRepository = manualTransferReferenceRepository;
        _parameterService = parameterService;
    }

    public async Task<PaginatedList<ManualTransferDto>> Handle(GetAllManualTransfersQuery request, CancellationToken cancellationToken)
    {
        var transactions = _transactionRepository.GetAll();

        if (request.TransactionType != null)
        {
            transactions = transactions.Where(s => s.TransactionType == request.TransactionType);
        }
        else
        {
            transactions = transactions.Where(s =>
                s.TransactionType == TransactionType.ManualTransferWithdraw ||
                s.TransactionType == TransactionType.ManualTransferDeposit);
        }

        if (!string.IsNullOrEmpty(request.WalletNumber))
        {
            var wallet = await _walletRepository
                .GetAll()
                .Where(s => s.WalletNumber == request.WalletNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (wallet != null)
            {
                transactions = transactions.Where(a => a.WalletId == wallet.Id);
            }
        }

        if (request.StartDate != null)
        {
            transactions = transactions.Where(s => s.CreateDate > request.StartDate);
        }

        if (request.EndDate != null)
        {
            transactions = transactions.Where(s => s.CreateDate < request.EndDate);
        }

        var companyWalletNumber = await _parameterService.GetParameterAsync("ManualTransfer", "ManualTransferWalletNumber");

        if (companyWalletNumber == null)
        {
            throw new NotFoundException(nameof(ParameterDto), $"Company Account Parameter not found: GroupCode: CompanyContactInformation, ParameterCode: ManualTransferWalletNumber");
        }

        var companyWallet = _walletRepository.GetAll().FirstOrDefault(s => s.WalletNumber == companyWalletNumber.ParameterValue);

        transactions = transactions.Where(s => s.WalletId != companyWallet.Id);

        var result = await transactions
            .PaginatedListAsync<Transaction>(request.Page, request.Size, request.OrderBy, request.SortBy);

        return new PaginatedList<ManualTransferDto>
        {
            PageNumber = result.PageNumber,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount,
            OrderBy = result.OrderBy,
            SortBy = result.SortBy,
            Items = result.Items.Select(PopulateManualTransferDto).ToList()
        };

    }
    private ManualTransferDto PopulateManualTransferDto(Transaction transaction)
    {
        var wallet = _walletRepository
            .GetAll()
            .Include(s => s.Account)
            .First(a => a.Id == transaction.WalletId);

        var manualTransferReferences = _manualTransferReferenceRepository
            .GetAll()
            .Where(s => s.TransactionId == transaction.Id)
            .ToList();

        var documentList = manualTransferReferences.ToDictionary(s =>
            s.DocumentType.ToString(),
            s => s.DocumentId);

        return new ManualTransferDto
        {
            DocumentList = documentList,
            ApprovalId = manualTransferReferences.Count > 0 ? manualTransferReferences[0].ApprovalRequestId : Guid.Empty,
            TransactionId = transaction.Id,
            TransactionType = transaction.TransactionType,
            WalletNumber = wallet.WalletNumber,
            Name = wallet.Account.Name,
            Amount = transaction.Amount,
            TransactionDate = transaction.TransactionDate
        };
    }
}