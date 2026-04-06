using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.Links;
using LinkPara.PF.Application.Features.Links.Command.DeleteLink;
using LinkPara.PF.Application.Features.Links.Command.SaveLink;
using LinkPara.PF.Application.Features.Links.Queries.GetAllLink;
using LinkPara.PF.Application.Features.Links.Queries.GetCreateLinkRequirement;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Transactions;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.Security;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LinkPara.PF.Infrastructure.Services;

public class LinkService : ILinkService
{
    private readonly PfDbContext _context;
    private readonly IGenericRepository<Link> _linkRepository;
    private readonly IGenericRepository<LinkTransaction> _linkTransactionRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<MerchantUser> _merchantUserRepository;
    private readonly IGenericRepository<SubMerchantUser> _subMerchantUserRepository;
    private readonly IGenericRepository<PricingProfile> _pricingProfileRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IAuditLogService _auditLogService;
    private readonly IVaultClient _vaultClient;
    private readonly ILogger<Link> _logger;
    private readonly ISecureRandomGenerator _randomGenerator;
    private readonly IRestrictionService _restrictionService;
    public LinkService(PfDbContext context,
        IGenericRepository<Link> linkRepository,
        IGenericRepository<LinkTransaction> linkTransactionRepository,
        IGenericRepository<Merchant> merchantRepository,
        IGenericRepository<MerchantUser> merchantUserRepository,
        IGenericRepository<PricingProfile> pricingProfileRepository,
        IContextProvider contextProvider,
        IAuditLogService auditLogService,
        IVaultClient vaultClient,
        ILogger<Link> logger,
        ISecureRandomGenerator randomGenerator,
        IRestrictionService restrictionService,
        IGenericRepository<SubMerchantUser> subMerchantUserRepository)
    {
        _context = context;
        _linkRepository = linkRepository;
        _linkTransactionRepository = linkTransactionRepository;
        _merchantRepository = merchantRepository;
        _merchantUserRepository = merchantUserRepository;
        _pricingProfileRepository = pricingProfileRepository;
        _contextProvider = contextProvider;
        _auditLogService = auditLogService;
        _vaultClient = vaultClient;
        _logger = logger;
        _randomGenerator = randomGenerator;
        _restrictionService = restrictionService;
        _subMerchantUserRepository = subMerchantUserRepository;
    }
    public async Task<LinkResponse> SaveAsync(SaveLinkCommand request)
    {
        try
        {
            await _restrictionService.IsUserAuthorizedAsync(request.MerchantId);

            var merchant = await _context.Merchant.FindAsync(request.MerchantId);

            var subMerchant = new SubMerchant();
            if (request.SubMerchantId.HasValue && request.SubMerchantId.Value != Guid.Empty)
            {
                subMerchant = await _context.SubMerchant.FindAsync(request.SubMerchantId.Value);

            }

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

            if (merchant is null)
            {
                throw new NotFoundException(nameof(merchant));
            }

            var parentMerchantFinancialTransaction = true;
            if (merchant.ParentMerchantId is not null && merchant.ParentMerchantId != Guid.Empty)
            {
                var parentMerchant = await _merchantRepository.GetByIdAsync(merchant.ParentMerchantId);
                if (parentMerchant is not null)
                {
                    parentMerchantFinancialTransaction = parentMerchant.FinancialTransactionAllowed;
                }
            }
            ValidateMerchant(merchant, parentMerchantFinancialTransaction);

            var link = CreateLink(request, merchant, subMerchant, parseUserId);

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                link.MaxUsageCount = request.LinkType == LinkType.MultipleUse ? request.MaxUsageCount : 1;

                if (request.LinkAmountType == LinkAmountType.FixedAmount)
                {
                    link.Amount = request.Amount;
                }

                if (request.Installments is not null)
                {
                    foreach (var installment in request.Installments)
                    {
                        var linkInstallment = CreateLinkInstallment(link, installment, parseUserId);

                        await _context.LinkInstallment.AddAsync(linkInstallment);
                    }
                }

                link.LinkCode = GenerateLinkUrl();

                await _context.Link.AddAsync(link);
                await _context.SaveChangesAsync();
                scope.Complete();

            });
            await _auditLogService.AuditLogAsync(
                  new AuditLog
                  {
                      IsSuccess = true,
                      LogDate = DateTime.Now,
                      Operation = "CreateLink",
                      SourceApplication = "PF",
                      Resource = "Link",
                      Details = new Dictionary<string, string>
                     {
                       {"Id", link.Id.ToString() }
                     }
                  });

            var baseUrl = _vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "LinkPaymentBaseUrl");

            return new LinkResponse()
            {
                Id = link.Id,
                IsSuccess = true,
                LinkUrl = baseUrl + "/" + link.LinkCode,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"CreateLinkError : {ex}");
            throw;
        }
    }

    private static void ValidateMerchant(Merchant merchant, bool parentMerchantFinancialTransaction)
    {
        if (merchant.MerchantStatus != MerchantStatus.Active || !merchant.FinancialTransactionAllowed || !parentMerchantFinancialTransaction) 
        {
            throw new InvalidMerchantStatusException();
        }

        if (!merchant.IntegrationMode.ToString().Contains(IntegrationMode.LinkPaymentPage.ToString()))
        {
            throw new IntegrationModeNotAllowedException();
        }
    }

    public async Task<LinkRequirementResponse> GetCreateLinkRequirements(GetCreateLinkRequirementQuery request)
    {

        var merchant = await _merchantRepository.GetByIdAsync(request.MerchantId);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(merchant));
        }

        var linkRequirementResponse = new LinkRequirementResponse()
        {
            Is3dRequired = merchant.IsLinkPayment3dRequired
        };

        if (!merchant.InstallmentAllowed)
        {
            return linkRequirementResponse;
        }

        var profileItem = await _pricingProfileRepository.GetAll()
            .Include(p => p.PricingProfileItems)
            .Where(p => p.PricingProfileNumber == merchant.PricingProfileNumber && p.ProfileStatus == ProfileStatus.InUse)
            .FirstOrDefaultAsync();

        if (profileItem is null)
        {
            throw new NotFoundException(nameof(profileItem));
        }

        linkRequirementResponse.AvailableInstallmentCounts = profileItem.PricingProfileItems
                    .Where(item => item.InstallmentNumber != 0 && item.IsActive)
                    .OrderBy(item => item.InstallmentNumber)
                    .Select(item => item.InstallmentNumber)
                    .ToList();

        return linkRequirementResponse;
    }
    public async Task<PaginatedList<LinkDto>> GetListAsync(GetAllLinkQuery request)
    {
        await _restrictionService.IsUserAuthorizedAsync(request.MerchantId);

        var linkList = _linkRepository.GetAll();
        var merchantUserList = _merchantUserRepository.GetAll();
        var subMerchantUserList = _subMerchantUserRepository.GetAll();

        if (request.MerchantId != Guid.Empty)
        {
            linkList = linkList.Where(b => b.MerchantId == request.MerchantId);
        }

        if (request.SubMerchantId is not null)
        {
            linkList = linkList.Where(b => b.SubMerchantId == request.SubMerchantId);
        }

        switch (request.LinkSearchType)
        {
            case LinkSearchType.LinkInfoSearch:
                linkList = FilterLinkListByLinkInfo(linkList, request.LinkInfoSearchRequest);
                break;
            case LinkSearchType.TransactionInfoSearch:
                linkList = await FilterLinkListByTransactionInfoAsync(linkList, request.LinkTransactionSearchRequest);
                break;
            case LinkSearchType.CustomerInfoSearch:
                linkList = await FilterLinkListByCustomerInfo(linkList, request.LinkCustomerSearchRequest);
                break;
            default:
                linkList = FilterLinkListByLinkInfo(linkList, request.LinkInfoSearchRequest);
                break;
        }

        var baseUrl = _vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "LinkPaymentBaseUrl");

        return await linkList
            .Select(l => new LinkDto
            {
                Id = l.Id,
                LinkCode = l.LinkCode,
                LinkUrl = baseUrl + "/" + l.LinkCode,
                LinkStatus = l.LinkStatus,
                LinkPaymentStatus = l.LinkPaymentStatus,
                LinkType = l.LinkType,
                ExpiryDate = l.ExpiryDate,
                CreateDate = l.CreateDate,
                CurrentUsageCount = l.CurrentUsageCount,
                MaxUsageCount = l.MaxUsageCount,
                OrderId = l.OrderId,
                LinkAmountType = l.LinkAmountType,
                Amount = l.Amount,
                Currency = l.Currency,
                CommissionFromCustomer = l.CommissionFromCustomer,
                Is3dRequired = l.Is3dRequired,
                MerchantName = l.MerchantName,
                MerchantNumber = l.MerchantNumber,
                SubMerchantId = l.SubMerchantId,
                SubMerchantNumber = l.SubMerchantNumber,
                SubMerchantName = l.SubMerchantName,
                ProductName = l.ProductName,
                ProductDescription = l.ProductDescription,
                ReturnUrl = l.ReturnUrl,
                CreatedNameBy = merchantUserList.Where(mu => mu.UserId.ToString() == l.CreatedBy).Select(mu => $"{mu.Name} {mu.Surname}").FirstOrDefault() ?? subMerchantUserList.Where(mu => mu.UserId.ToString() == l.CreatedBy).Select(mu => $"{mu.Name} {mu.Surname}").FirstOrDefault() 
            })
            .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task DeleteAsync(DeleteLinkCommand command)
    {
        var link = await _linkRepository.GetByIdAsync(command.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (link is null)
        {
            throw new NotFoundException(nameof(Link), command.Id);
        }

        try
        {
            link.RecordStatus = RecordStatus.Passive;
            link.LinkStatus = ChannelStatus.Cancelled;
            link.LastModifiedBy = parseUserId.ToString();
            link.UpdateDate = DateTime.Now;

            await _linkRepository.UpdateAsync(link);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeleteLink",
                    SourceApplication = "PF",
                    Resource = "Link",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                    {"Id", command.Id.ToString() },
                    }
                });

        }
        catch (Exception exception)
        {
            _logger.LogError($"LinkDeleteError : {exception}");
        }
    }

    private Link CreateLink(SaveLinkCommand command, Merchant merchant, SubMerchant subMerchant, Guid userId)
    {
        var link = new Link
        {
            LinkType = command.LinkType,
            OrderId = command.OrderId,
            LinkStatus = ChannelStatus.Active,
            LinkPaymentStatus = ChannelPaymentStatus.Pending,
            ExpiryDate = command.ExpiryDate,
            LinkAmountType = command.LinkAmountType,
            Currency = command.Currency,
            CommissionFromCustomer = command.CommissionFromCustomer,
            Is3dRequired = command.Is3dRequired,
            MerchantId = merchant.Id,
            MerchantName = merchant.Name,
            MerchantNumber = merchant.Number,
            ProductName = command.ProductName,
            ProductDescription = command.ProductDescription,
            ReturnUrl = command.ReturnUrl,
            IsNameRequired = command.IsNameRequired,
            IsEmailRequired = command.IsEmailRequired,
            IsPhoneNumberRequired = command.IsPhoneNumberRequired,
            IsAddressRequired = command.IsAddressRequired,
            IsNoteRequired = command.IsNoteRequired,
            CreatedBy = userId.ToString(),
            RecordStatus = RecordStatus.Active,
        };
        if (!string.IsNullOrEmpty(subMerchant.Name))
        {
            link.SubMerchantId = subMerchant.Id;
            link.SubMerchantName = subMerchant.Name;
            link.SubMerchantNumber = subMerchant.Number;
        }
        return link;
    }
    private LinkInstallment CreateLinkInstallment(Link link, int installment, Guid userId)
    {
        var linkInstallment = new LinkInstallment
        {
            LinkId = link.Id,
            Installment = installment,
            CreatedBy = userId.ToString(),
            RecordStatus = RecordStatus.Active,
        };
        return linkInstallment;
    }
    private string GetUniqueKey()
    {
        int maxSize = 8;
        char[] chars = new char[52];
        string a = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        chars = a.ToCharArray();

        int size = maxSize;

        StringBuilder result = new StringBuilder(size);
        for (int i = 0; i < size; i++)
        {
            result.Append(chars[_randomGenerator.GenerateSecureRandomNumber(0, chars.Length)]);
        }

        return result.ToString();
    }
    private string GenerateLinkUrl()
    {
        var any = false;
        var url = string.Empty;

        do
        {
            url = GetUniqueKey();

            any = _linkRepository.GetAll().Any(s => s.LinkCode == url);
        }
        while (any);

        return url;
    }
    private async Task<IQueryable<Link>> FilterLinkListByTransactionInfoAsync(IQueryable<Link> linkList, LinkTransactionSearchRequest request)
    {
        var linkTransactionList = _linkTransactionRepository.GetAll();

        if (request.TransactionDateStart is not null)
        {
            linkTransactionList = linkTransactionList.Where(b => b.TransactionDate
                               >= request.TransactionDateStart);
        }

        if (request.TransactionDateEnd is not null)
        {
            linkTransactionList = linkTransactionList.Where(b => b.TransactionDate
                               <= request.TransactionDateEnd);
        }

        if (request.OrderId is not null)
        {
            linkTransactionList = linkTransactionList.Where(b => b.OrderId.ToLower().Contains(request.OrderId.ToLower()));
        }

        if (request.TransactionType is not null)
        {
            linkTransactionList = linkTransactionList.Where(b => b.TransactionType
                               == request.TransactionType);
        }

        if (request.LinkPaymentStatus is not null)
        {
            linkTransactionList = linkTransactionList.Where(b => b.LinkPaymentStatus
                               == request.LinkPaymentStatus);
        }

        if (request.CommissionFromCustomer is not null)
        {
            linkTransactionList = linkTransactionList.Where(b => b.CommissionFromCustomer
                               == request.CommissionFromCustomer);
        }

        var linkCodeList = await linkTransactionList
            .Select(b => b.LinkCode).ToListAsync();

        return linkList.Where(b => linkCodeList.Contains(b.LinkCode));
    }
    private async Task<IQueryable<Link>> FilterLinkListByCustomerInfo(IQueryable<Link> linkList, LinkCustomerSearchRequest request)
    {
        var linkCustomerList = (from linkTransaction in _context.LinkTransaction
                                join linkCustomer in _context.LinkCustomer on linkTransaction.Id equals linkCustomer.LinkTransactionId
                                select new
                                {
                                    LinkCode = linkTransaction.LinkCode,
                                    LinkCustomer = linkCustomer,
                                });

        if (!string.IsNullOrEmpty(request.Name))
        {
            linkCustomerList = linkCustomerList.Where(e => e.LinkCustomer.Name.ToLower()
            .Contains(request.Name.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.Email))
        {
            linkCustomerList = linkCustomerList.Where(e => e.LinkCustomer.Email.ToLower()
            .Contains(request.Email.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            linkCustomerList = linkCustomerList.Where(e => e.LinkCustomer.PhoneNumber.ToLower()
            .Contains(request.PhoneNumber.ToLower()));
        }

        var linkCodeList = await linkCustomerList
                    .Select(b => b.LinkCode).ToListAsync();

        return linkList.Where(b => linkCodeList.Contains(b.LinkCode));
    }
    private IQueryable<Link> FilterLinkListByLinkInfo(IQueryable<Link> linkList, LinkInfoSearchRequest request)
    {
        if (request.LinkCode is not null)
        {
            linkList = linkList.Where(b => b.LinkCode
                               == request.LinkCode);
        }
        if (!string.IsNullOrEmpty(request.ProductName))
        {
            linkList = linkList.Where(b => b.ProductName.ToLower()
                             .Contains(request.ProductName.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.ProductDescription))
        {
            linkList = linkList.Where(b => b.ProductDescription.ToLower()
                             .Contains(request.ProductDescription.ToLower()));
        }

        if (request.ExpiryDateStart is not null)
        {
            linkList = linkList.Where(b => b.ExpiryDate
                               >= request.ExpiryDateStart);
        }

        if (request.ExpiryDateEnd is not null)
        {
            linkList = linkList.Where(b => b.ExpiryDate
                               <= request.ExpiryDateEnd);
        }

        if (request.CreateDateStart is not null)
        {
            linkList = linkList.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            linkList = linkList.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        if (request.LinkPaymentStatus is not null)
        {
            linkList = linkList.Where(b => b.LinkPaymentStatus
                               == request.LinkPaymentStatus);
        }

        if (request.LinkStatus is not null)
        {
            linkList = linkList.Where(b => b.LinkStatus
                               == request.LinkStatus);
        }

        if (request.LinkAmountType is not null)
        {
            linkList = linkList.Where(b => b.LinkAmountType
                               == request.LinkAmountType);
        }

        if (request.LinkType is not null)
        {
            linkList = linkList.Where(b => b.LinkType
                               == request.LinkType);
        }

        if (request.MinAmount is not null)
        {
            linkList = linkList.Where(b => b.Amount
                               >= request.MinAmount);
        }

        if (request.MaxAmount is not null)
        {
            linkList = linkList.Where(b => b.Amount
                               <= request.MaxAmount);
        }

        return linkList;
    }
}
