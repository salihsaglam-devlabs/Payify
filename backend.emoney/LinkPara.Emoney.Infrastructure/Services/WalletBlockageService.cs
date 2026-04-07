using AutoMapper;
using LinkPara.Approval.Models.Enums;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Strategies;
using LinkPara.Emoney.Application.Features.WalletBlockages;
using LinkPara.Emoney.Application.Features.WalletBlockages.Commands;
using LinkPara.Emoney.Application.Features.WalletBlockages.Queries;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.Approval;
using LinkPara.HttpProviders.Approval.Models;
using LinkPara.HttpProviders.Documents;
using LinkPara.HttpProviders.Documents.Models;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data;

namespace LinkPara.Emoney.Infrastructure.Services;

public class WalletBlockageService : IWalletBlockageService
{
    private readonly IGenericRepository<WalletBlockage> _walletBlockageRepository;
    private readonly IGenericRepository<WalletBlockageDocument> _walletBlockageDocumentRepository;
    private readonly IGenericRepository<Wallet> _walletRepository;    
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IDocumentService _documentService;
    private readonly IRequestsService _requestsService;
    private readonly IContextProvider _contextProvider;
    private readonly IWalletBlockageService _walletBlockageService;
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WalletBlockageService> _logger;
    private readonly IEmailSender _emailSender;

    public const string ApprovalRequestDisplayName = "Cüzdan Bloke İşlemi";

    public WalletBlockageService(IGenericRepository<WalletBlockage> walletBlockageRepository,
        IGenericRepository<WalletBlockageDocument> walletBlockageDocumentRepository,
        IGenericRepository<Wallet> walletRepository,
        IGenericRepository<AccountUser> accountUserRepository,
        IDocumentService documentService,
        IContextProvider contextProvider,
        IMapper mapper,
        IServiceScopeFactory scopeFactory,
        ILogger<WalletBlockageService> logger,
        IRequestsService requestsService,
        IEmailSender emailSender)

    {
        _walletBlockageRepository = walletBlockageRepository;
        _walletBlockageDocumentRepository = walletBlockageDocumentRepository;
        _walletRepository = walletRepository;
        _accountUserRepository = accountUserRepository;
        _documentService = documentService;
        _contextProvider = contextProvider;
        _mapper = mapper;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _requestsService = requestsService;
        _emailSender = emailSender;
    }

    public async Task<PaginatedList<WalletBlockageDto>> GetWalletBlockageAsync(GetWalletBlockageQuery request)
    {
        var list = _walletBlockageRepository.GetAll()
            .Where(x => x.RecordStatus == RecordStatus.Active);

        if (!string.IsNullOrEmpty(request.WalletNumber))
        {
            list = list.Where(s => (s.WalletNumber == request.WalletNumber));
        }

        if (!string.IsNullOrEmpty(request.WalletId.ToString()))
        {
            list = list.Where(s => (s.WalletId == request.WalletId));
        }

        if (!string.IsNullOrEmpty(request.AccountName))
        {
            list = list.Where(s => s.AccountName == request.AccountName);
        }        

        if (!string.IsNullOrEmpty(request.BlockageStatus.ToString()))
        {
            list = list.Where(s => s.BlockageStatus == request.BlockageStatus);
        }                        

        return await list
                    .PaginatedListWithMappingAsync<WalletBlockage, WalletBlockageDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task AddWalletBlockageAsync(AddWalletBlockageCommand request)
    {
        var wallet = new Wallet();

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        try
        {
            var strategy = new NoRetryExecutionStrategy(dbContext);

            await strategy.ExecuteAsync(async () =>
            {
                await using var transactionScope = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    wallet = await _walletRepository.GetAll()
                                .FirstOrDefaultAsync(s => s.WalletNumber == request.WalletNumber);

                    if (wallet is null)
                    {
                        throw new NotFoundException("WalletNumber : ", request.WalletNumber);
                    }

                    await CheckActiveApprovalsForWallet(wallet.Id.ToString());

                    var accountUser = await _accountUserRepository.GetAll()
                        .FirstOrDefaultAsync(s => s.AccountId == wallet.AccountId);

                    if (accountUser == null)
                    {
                        throw new NotFoundException(nameof(Account), wallet.AccountId);
                    }

                    if (request.OperationType == WalletBlockageOperationType.AddBlockage)
                    {
                        if (wallet.AvailableBalance < request.CashBlockageAmount + request.CreditBlockageAmount)
                        {
                            throw new WalletBlockageAmountException();
                        }
                    }

                    var blockageStatus = (wallet.AvailableBalance == request.CashBlockageAmount + request.CreditBlockageAmount) ? WalletBlockageStatus.Blocked : WalletBlockageStatus.PartiallyBlocked;

                    var walletBlockage = new WalletBlockage
                    {
                        WalletId = wallet.Id,
                        WalletNumber = wallet.WalletNumber,
                        AccountName = string.Concat(accountUser.Firstname, " ", accountUser.Lastname),
                        WalletCurrencyCode = wallet.CurrencyCode,
                        CashBlockageAmount = request.CashBlockageAmount,
                        CreditBlockageAmount = request.CreditBlockageAmount,
                        OperationType = request.OperationType,
                        BlockageStatus = blockageStatus,
                        BlockageType = request.BlockageType,
                        BlockageDescription = request.BlockageDescription,
                        BlockageStartDate = request.BlockageStartDate,
                        BlockageEndDate = request.BlockageEndDate,
                        CreateDate = DateTime.Now,
                        CreatedBy = _contextProvider.CurrentContext.UserId
                    };

                    if (request.OperationType == WalletBlockageOperationType.RemoveBlockage)
                    {
                        request.CashBlockageAmount = request.CashBlockageAmount * (-1);
                        request.CreditBlockageAmount = request.CreditBlockageAmount * (-1);
                    }

                    wallet.BlockedBalance += request.CashBlockageAmount;
                    wallet.BlockedBalanceCredit += request.CreditBlockageAmount;
                    wallet.IsBlocked = (blockageStatus == WalletBlockageStatus.Blocked) ? true : false;
                    wallet.LastModifiedBy = _contextProvider.CurrentContext.UserId;
                    wallet.UpdateDate = DateTime.Now;
                    await _walletRepository.UpdateAsync(wallet);
                    await _walletBlockageRepository.AddAsync(walletBlockage);

                    await transactionScope.CommitAsync();

                    var templateData = new Dictionary<string, string>
                    {
                        { "walletnumber", walletBlockage.WalletNumber },
                        { "name", walletBlockage.AccountName },
                        { "blockagestartdate", walletBlockage.BlockageStartDate.ToString("dd/MM/yyyy HH:mm:ss") },
                        { "blockageenddate", (walletBlockage.BlockageEndDate != null) ? Convert.ToDateTime(walletBlockage.BlockageEndDate).ToString("dd /MM/yyyy HH:mm:ss") : "Belirtilmemiş" },
                        { "cashblockageamount", walletBlockage.CashBlockageAmount.ToString("N2") },
                        { "creditblockageamount", walletBlockage.CreditBlockageAmount.ToString("N2") }
                    };

                    await SendInformationMailAsync(templateData, accountUser.Email);
                }
                catch (Exception ex)
                {
                    await transactionScope.RollbackAsync();
                    throw;
                }


            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"AddWalletBlockageAsync Error {exception}", exception);
            throw;
        }
    }

    private async Task CheckActiveApprovalsForWallet(string id)
    {
        var approvalRequests = await _requestsService.GetAllWalletBlocakgeRequestsAsync(new GetWalletBlockageRequestsQuery
        {
            Statuses = new string[] { ApprovalStatus.FirstApprovePending.ToString(), ApprovalStatus.SecondApprovePending.ToString() },
            ActionType = ActionType.Post.ToString(),
        });

        var activeRequests = approvalRequests.Items
            .Where(x => x.DisplayName == ApprovalRequestDisplayName
                     && x.Status == 0 || x.Status == 1).ToList();

        foreach (var request in activeRequests)
        {
            if (request.Body.Contains("\"WalletId\":\"" + id + "\""))
            {
                throw new WalletBlockageException();
            }
        }
    }
    
    public async Task<WalletBlockageDocumentDto> AddWalletBlockageDocumentAsync(AddWalletBlockageDocumentCommand request)
    {
        try
        {
            var wallet = await _walletRepository.GetByIdAsync(request.WalletId);

            if (wallet == null)
            {
                throw new NotFoundException("WalletId : ", request.WalletId);
            }

            var document = new CreateDocumentRequest()
            {
                OriginalFileName = request.OriginalFileName,
                ContentType = request.ContentType,
                Bytes = request.Bytes,
                UserId = new Guid(_contextProvider.CurrentContext.UserId),
                DocumentTypeId = request.DocumentTypeId
            };

            var documentResponse = await _documentService.CreateDocument(document);

            var WalletBlockageDocument = new WalletBlockageDocument
            {
                WalletId = request.WalletId,
                DocumentId = documentResponse.Id,
                DocumentTypeId = documentResponse.DocumentTypeId,
                FileName = request.OriginalFileName,
                Description = request.DocumentDescription
            };
            await _walletBlockageDocumentRepository.AddAsync(WalletBlockageDocument);

            return _mapper.Map<WalletBlockageDocumentDto>(WalletBlockageDocument);
        }
        catch(Exception ex) 
        { 
            throw; 
        }
    }

    public async Task<bool> RemoveWalletBlockageDocumentAsync(RemoveWalletBlockageDocumentCommand request)
    {
        var WalletBlockageDocument = await _walletBlockageDocumentRepository.GetByIdAsync(request.WalletBlockageDocumentId);

        if (WalletBlockageDocument == null)
        {
            throw new NotFoundException("WalletBlockageDocumentId : ", request.WalletBlockageDocumentId);
        }

        WalletBlockageDocument.RecordStatus = RecordStatus.Passive;
        WalletBlockageDocument.UpdateDate = DateTime.Now;
        WalletBlockageDocument.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? WalletBlockageDocument.LastModifiedBy;
        await _walletBlockageDocumentRepository.UpdateAsync(WalletBlockageDocument);

        return true;
    }

    public async Task<List<WalletBlockageDocumentDto>> GetWalletBlockageDocumentsAsync(GetWalletBlockageDocumentQuery request)
    {
        var wallet = await _walletRepository.GetAll()
                                .FirstOrDefaultAsync(s => s.WalletNumber == request.WalletNumber);

        if (wallet is null)
        {
            throw new NotFoundException("WalletNumber : ", request.WalletNumber);
        }

        var WalletBlockageDocumentList = await _walletBlockageDocumentRepository.GetAll()
                                    .Where(x => x.WalletId == wallet.Id
                                    && x.RecordStatus == RecordStatus.Active)
                                    .ToListAsync();        

        return _mapper.Map<List<WalletBlockageDocumentDto>>(WalletBlockageDocumentList);
    }

    public async Task<Wallet> CalculateWalletBlockageAmounts(Wallet wallet, decimal cashBlockageAmount, decimal creditBlockageAmount)
    {
        decimal totalCashBlockageAmount = cashBlockageAmount;
        decimal totalCreditBlockageAmount = creditBlockageAmount;

        var walletBlockages = await _walletBlockageRepository.GetAll().
                Where(s => s.WalletId == wallet.Id
                && s.RecordStatus == RecordStatus.Active).ToListAsync();

        foreach (var blockage in walletBlockages)
        {
            if (blockage.OperationType == WalletBlockageOperationType.AddBlockage)
            {
                totalCashBlockageAmount += blockage.CashBlockageAmount;
                totalCreditBlockageAmount += blockage.CreditBlockageAmount;
            }

            if (blockage.OperationType == WalletBlockageOperationType.RemoveBlockage)
            {
                totalCashBlockageAmount -= blockage.CashBlockageAmount;
                totalCreditBlockageAmount -= blockage.CreditBlockageAmount;
            }
        }

        wallet.BlockedBalance = totalCashBlockageAmount;
        wallet.BlockedBalanceCredit = totalCreditBlockageAmount;


        return wallet;
    }

    public async Task<List<WalletBlockage>> RemoveExpiredBlockagesAsync()
    {
        var list = new List<WalletBlockage>();

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
        try
        {
            var expiredWalletBlockages = await _walletBlockageRepository.GetAll().
                   Where(s => s.OperationType == WalletBlockageOperationType.AddBlockage
                      && ((DateTime)s.BlockageEndDate).Date == DateTime.Now.Date  
                      && s.RecordStatus == RecordStatus.Active).ToListAsync();

            foreach (WalletBlockage blockage in expiredWalletBlockages)
            {
                var wallet = await _walletRepository.GetAll()
                        .FirstOrDefaultAsync(s => s.WalletNumber == blockage.WalletNumber);

                if (wallet is null)
                {
                    throw new NotFoundException("WalletNumber : ", blockage.WalletNumber);
                }

                var strategy = new NoRetryExecutionStrategy(dbContext);

                await strategy.ExecuteAsync(async () =>
                {
                    await using var transactionScope = await dbContext.Database.BeginTransactionAsync();
                    try
                    {
                        wallet.BlockedBalance = wallet.BlockedBalance - blockage.CashBlockageAmount;
                        wallet.BlockedBalanceCredit = wallet.BlockedBalanceCredit - blockage.CreditBlockageAmount;
                        await _walletRepository.UpdateAsync(wallet);

                        blockage.BlockageStatus = WalletBlockageStatus.Unblocked;
                        blockage.LastModifiedBy = "BATCH";
                        await _walletBlockageRepository.UpdateAsync(blockage);

                        await transactionScope.CommitAsync();

                        list.Add(blockage);
                    }
                    catch
                    {
                        await transactionScope.RollbackAsync();
                        throw;
                    }
                });
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"RemoveExpiredBlockagesAsync Error {exception}", exception);
            throw;
        }
        return list;
    }

    private async Task SendInformationMailAsync(Dictionary<string, string> templateData, string email)
    {
        var mailParams = new SendEmail
        {
            TemplateName = "AddWalletBlockageTemplate",
            DynamicTemplateData = templateData,
            ToEmail = email
        };

        await _emailSender.SendEmailAsync(mailParams);
    }
}
