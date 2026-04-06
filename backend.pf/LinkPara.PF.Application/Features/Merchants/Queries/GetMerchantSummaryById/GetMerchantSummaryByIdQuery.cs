using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.Security;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.Merchants.Queries.GetMerchantSummaryById
{
    public class GetMerchantSummaryByIdQuery : IRequest<MerchantSummaryDto>
    {
    }

    public class GetMerchantPanelByIdQueryHandler : IRequestHandler<GetMerchantSummaryByIdQuery, MerchantSummaryDto>
    {
        private readonly IGenericRepository<Merchant> _repository;
        private readonly IGenericRepository<MerchantUser> _merchantUserRepository;
        private readonly IContextProvider _contextProvider;
        private readonly IMapper _mapper;
        private readonly IVaultClient _vaultClient;
        private readonly IEncryptionService _encryptionService;
        private readonly IRestrictionService _restrictionService;
        public GetMerchantPanelByIdQueryHandler(
            IGenericRepository<Merchant> repository,
            IGenericRepository<MerchantUser> merchantRepository,
            IContextProvider contextProvider,
            IMapper mapper,
            IVaultClient vaultClient,
            IEncryptionService encryptionService,
            IRestrictionService restrictionService)
        {
            _repository = repository;
            _merchantUserRepository = merchantRepository;
            _contextProvider = contextProvider;
            _mapper = mapper;
            _vaultClient = vaultClient;
            _encryptionService = encryptionService;
            _restrictionService = restrictionService;
        }
        public async Task<MerchantSummaryDto> Handle(GetMerchantSummaryByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
            
            var merchantUser = await _merchantUserRepository.GetAll()
                .FirstOrDefaultAsync(s => s.UserId == parseUserId, cancellationToken);

            if (merchantUser is null)
            {
                throw new NotFoundException(nameof(MerchantUser), parseUserId);
            }
            
            await _restrictionService.IsUserAuthorizedAsync(merchantUser.MerchantId);

            var merchant = await _repository.GetAll()
                .Include(b => b.Customer)
                .ThenInclude(b => b.AuthorizedPerson)
                .Include(b => b.MerchantBankAccounts)
                .Include(b => b.MerchantWallets)
                .Include(b => b.MerchantApiKeyList)
                .Select(b => new MerchantSummaryDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Number = b.Number,
                    MerchantStatus = b.MerchantStatus,
                    MerchantType = b.MerchantType,
                    IntegrationMode = b.IntegrationMode,
                    CustomerId = b.CustomerId,
                    CommercialTitle = b.Customer.CommercialTitle,
                    TaxAdministration = b.Customer.TaxAdministration,
                    TaxNumber = b.Customer.TaxNumber,
                    AuthorizedPersonId = b.Customer.AuthorizedPerson.Id,
                    AuthorizedPersonCompanyEmail = b.Customer.AuthorizedPerson.CompanyEmail,
                    WebSiteUrl = b.WebSiteUrl,
                    Is3dRequired = b.Is3dRequired,
                    IsManuelPayment3dRequired = b.IsManuelPayment3dRequired,
                    IsLinkPayment3dRequired = b.IsLinkPayment3dRequired,
                    IsHostedPayment3dRequired = b.IsHostedPayment3dRequired,
                    IsCvvPaymentAllowed = b.IsCvvPaymentAllowed,
                    IsPostAuthAmountHigherAllowed = b.IsPostAuthAmountHigherAllowed,
                    IsReturnApproved = b.IsReturnApproved,
                    InstallmentAllowed = b.InstallmentAllowed,
                    IsExcessReturnAllowed = b.IsExcessReturnAllowed,
                    InternationalCardAllowed = b.InternationalCardAllowed,
                    PreAuthorizationAllowed = b.PreAuthorizationAllowed,
                    FinancialTransactionAllowed = b.FinancialTransactionAllowed,
                    PaymentAllowed = b.PaymentAllowed,
                    PaymentReturnAllowed = b.PaymentReturnAllowed,
                    PaymentReverseAllowed = b.PaymentReverseAllowed,
                    PricingProfileNumber = b.PricingProfileNumber,
                    PostingPaymentChannel = b.PostingPaymentChannel,
                    ParentMerchantId = b.ParentMerchantId ?? Guid.Empty,
                    ParentMerchantName = b.ParentMerchantName,
                    ParentMerchantNumber = b.ParentMerchantNumber,
                    CreateDate = b.CreateDate,
                    MerchantBankAccounts = b.MerchantBankAccounts.Where(mba => mba.RecordStatus == RecordStatus.Active)
                    .Select(mba => new MerchantBankAccountDto
                    {
                        Id = mba.Id,
                        BankCode = mba.BankCode,
                        Iban = mba.Iban
                    }).ToList(),
                    MerchantWallets = b.MerchantWallets.Where(mwa => mwa.RecordStatus == RecordStatus.Active)
                    .Select(mwa => new MerchantWalletDto
                    {
                        Id = mwa.Id,
                        WalletNumber = mwa.WalletNumber
                    }).ToList(),
                    MerchantApiKeyList = b.MerchantApiKeyList.Select(a => new MerchantApiKeyDto
                    {
                        Id = a.Id,
                        MerchantId = a.MerchantId,
                        PublicKey = a.PublicKey,
                        PrivateKeyEncrypted = a.PrivateKeyEncrypted

                    }).ToList()

                }).FirstOrDefaultAsync(b => b.Id == merchantUser.MerchantId, cancellationToken);

            if (merchant is null)
            {
                throw new NotFoundException(nameof(Merchant), merchantUser.MerchantId);
            }

            var merchantDto = _mapper.Map<MerchantSummaryDto>(merchant);

            var keyConstant = await _vaultClient.GetSecretValueAsync<string>("SharedSecrets", "SignatureKeyConstant", "PfApiKeyEncryptionKey");

            merchantDto.MerchantApiKeyList.ForEach((x) =>
            {
                x.PrivateKey = _encryptionService.Decrypt(x.PrivateKeyEncrypted, keyConstant);
            });

            return merchantDto;
        }
    }
}
