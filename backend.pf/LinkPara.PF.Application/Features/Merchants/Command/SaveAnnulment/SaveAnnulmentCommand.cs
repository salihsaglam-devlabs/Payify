using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.HttpProviders.IKS;
using LinkPara.HttpProviders.IKS.Models.Request;
using LinkPara.HttpProviders.IKS.Models.Response;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace LinkPara.PF.Application.Features.Merchants.Command.SaveAnnulment
{
    public class SaveAnnulmentCommand : IRequest<IKSResponse<IKSAnnulmentResponse>>
    {
        public Guid Id { get; set; }
        public string AnnulmentCode { get; set; }
        public string AnnulmentCodeDescription { get; set; }
        public string AnnulmentDescription { get; set; }
        public bool IsCancelCode { get; set; }
    }
    public class SaveAnnulmentCommandHandler : IRequestHandler<SaveAnnulmentCommand, IKSResponse<IKSAnnulmentResponse>>
    {
        private readonly IIKSService _iKSService;
        private readonly IGenericRepository<Merchant> _merchantRepository;
        private readonly IGenericRepository<MerchantBusinessPartner> _merchantBusinessPartnerRepository;
        private readonly ILogger<SaveAnnulmentCommand> _logger;
        private readonly IIksPfService _iksPfService;        
        private readonly ICustomerService _customerService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IVaultClient _vaultClient;

        public SaveAnnulmentCommandHandler(IIKSService ıKSService,
            IGenericRepository<Merchant> merchantRepository,
            ILogger<SaveAnnulmentCommand> logger,
            IGenericRepository<MerchantBusinessPartner> merchantBusinessPartnerRepository,
            IIksPfService iksPfService,
            ICustomerService customerService,
            IApplicationUserService applicationUserService,
            IVaultClient vaultClient)
        {
            _iKSService = ıKSService;
            _merchantRepository = merchantRepository;
            _logger = logger;
            _merchantBusinessPartnerRepository = merchantBusinessPartnerRepository;
            _iksPfService = iksPfService;
            _customerService = customerService;
            _applicationUserService = applicationUserService;
            _vaultClient = vaultClient;
        }
        public async Task<IKSResponse<IKSAnnulmentResponse>> Handle(SaveAnnulmentCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var isIksEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "IksEnabled");
                if (isIksEnabled)
                {
                    var errorRes = new IKSError
                    {
                        moreInformation = "IKS parametresi kapalı olduğundan fesih bildirimi yapılamadı."
                    };

                   return new IKSResponse<IKSAnnulmentResponse>
                    {
                        Error = errorRes,
                                                
                    };
                }
                    var merchant = await _merchantRepository.GetAll()
                .Include(x => x.Customer)
                .ThenInclude(x => x.AuthorizedPerson)
                .FirstOrDefaultAsync(x => x.RecordStatus == RecordStatus.Active
                                     && x.Id == command.Id, cancellationToken);

                if (merchant is null)
                {
                    throw new NotFoundException(nameof(Merchant), command.Id);
                }

                var merchantBusinessPartner = await _merchantBusinessPartnerRepository.GetAll()
                                                                       .Where(x => x.MerchantId == command.Id
                                                                        && x.RecordStatus == RecordStatus.Active)
                                                                       .Select(x => x.IdentityNumber)
                                                                       .ToArrayAsync(cancellationToken);

                var Partner2IdentityNo = string.Empty;
                var Partner3IdentityNo = string.Empty;
                var Partner4IdentityNo = string.Empty;
                var Partner5IdentityNo = string.Empty;

                for (int counter = 0; counter < merchantBusinessPartner.Length; counter++)
                {
                    switch (counter)
                    {
                        case 0:
                            Partner2IdentityNo = merchantBusinessPartner[counter];
                            break;
                        case 1:
                            Partner3IdentityNo = merchantBusinessPartner[counter];
                            break;
                        case 2:
                            Partner4IdentityNo = merchantBusinessPartner[counter];
                            break;
                        case 3:
                            Partner5IdentityNo = merchantBusinessPartner[counter];
                            break;
                    }
                }

                if (!command.IsCancelCode && String.IsNullOrEmpty(merchant.AnnulmentId))
                {
                    var saveAnnulmentRequest = new IKSSaveAnnulmentRequest
                    {
                        MerchantId = merchant.Id,
                        GlobalMerchantId = merchant.GlobalMerchantId,
                        OwnerIdentityNo = !String.IsNullOrEmpty(merchant.Customer?.AuthorizedPerson?.IdentityNumber) ?
                                          merchant.Customer?.AuthorizedPerson?.IdentityNumber : merchant.Customer?.TaxNumber,
                        Code = command.AnnulmentCode,
                        CodeDescription = command.AnnulmentCodeDescription,
                        Explanation = (command.AnnulmentCode == "E" || command.AnnulmentCode == "G" ||
                                      command.AnnulmentCode == "I" || command.AnnulmentCode == "L"
                                      || command.AnnulmentCode == "N" || command.AnnulmentCode == "O") ? command.AnnulmentDescription : null,
                        Partner2IdentityNo = !String.IsNullOrEmpty(Partner2IdentityNo) ? Partner2IdentityNo : null,
                        Partner3IdentityNo = !String.IsNullOrEmpty(Partner3IdentityNo) ? Partner3IdentityNo : null,
                        Partner4IdentityNo = !String.IsNullOrEmpty(Partner4IdentityNo) ? Partner4IdentityNo : null,
                        Partner5IdentityNo = !String.IsNullOrEmpty(Partner5IdentityNo) ? Partner5IdentityNo : null,
                    };

                    var result = await _iKSService.SaveAnnulmentAsync(saveAnnulmentRequest);
                    if (result.IsSuccess)
                    {

                        merchant.AnnulmentCode = command.AnnulmentCode;
                        merchant.AnnulmentId = result.Data?.AnnulmentId;
                        merchant.AnnulmentDescription = command.AnnulmentDescription;
                        merchant.AnnulmentDate = DateTime.Now;
                        merchant.MerchantStatus = MerchantStatus.Annulment;
                        merchant.IsAnnulment = true;

                        await UpdateMerchantInfoAsync(merchant);

                        await UpdateCustomerAsync(merchant);
                    }
                    return result;
                }
                else
                {

                    var updateAnnulmentRequest = new IKSUpdateAnnulmentRequest
                    {
                        MerchantId = merchant.Id,
                        GlobalMerchantId = merchant.GlobalMerchantId,
                        OwnerIdentityNo = !String.IsNullOrEmpty(merchant.Customer?.AuthorizedPerson?.IdentityNumber) ?
                                          merchant.Customer?.AuthorizedPerson?.IdentityNumber : merchant.Customer?.TaxNumber,
                        Code = command.AnnulmentCode,
                        Explanation = (command.AnnulmentCode == "E" || command.AnnulmentCode == "G" ||
                                       command.AnnulmentCode == "I" || command.AnnulmentCode == "L"
                                       || command.AnnulmentCode == "N" || command.AnnulmentCode == "O") ? command.AnnulmentDescription : null,
                        IsCancelCode = command.IsCancelCode,
                        Partner2IdentityNo = !String.IsNullOrEmpty(Partner2IdentityNo) ? Partner2IdentityNo : null,
                        Partner3IdentityNo = !String.IsNullOrEmpty(Partner3IdentityNo) ? Partner3IdentityNo : null,
                        Partner4IdentityNo = !String.IsNullOrEmpty(Partner4IdentityNo) ? Partner4IdentityNo : null,
                        Partner5IdentityNo = !String.IsNullOrEmpty(Partner5IdentityNo) ? Partner5IdentityNo : null,
                    };

                    var result = await _iKSService.UpdateAnnulmentAsync(updateAnnulmentRequest);
                    if (result.IsSuccess)
                    {
                        merchant.AnnulmentDescription = command.AnnulmentDescription;
                        merchant.AnnulmentCode = command.AnnulmentCode;
                        merchant.AnnulmentDate = DateTime.Now;
                        merchant.MerchantStatus = command.IsCancelCode ? MerchantStatus.Active : MerchantStatus.Annulment;
                        merchant.IsAnnulment = !command.IsCancelCode;

                        await UpdateMerchantInfoAsync(merchant);

                        await UpdateCustomerAsync(merchant);

                        if (command.IsCancelCode)
                        {
                            await _iksPfService.UpdateMerchantStatus(merchant);
                        }
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save Annulment Error {ex}", ex.Message);
                throw;
            }
        }
        private async Task UpdateMerchantInfoAsync(Merchant merchant)
        {
           await _merchantRepository.UpdateAsync(merchant);
           var merchantList = await _merchantRepository.GetAll()
                .Where(x => x.GlobalMerchantId == merchant.GlobalMerchantId && x.Id != merchant.Id).ToListAsync();

            if (merchantList.Count > 0)
            {
                foreach (var merc in merchantList)
                {
                    merc.AnnulmentCode = merchant.AnnulmentCode;
                    merc.AnnulmentId = merchant.AnnulmentId;
                    merc.AnnulmentDescription = merchant.AnnulmentDescription;
                    merc.AnnulmentDate = merchant.AnnulmentDate;
                    merc.MerchantStatus = merchant.MerchantStatus;
                    merc.IsAnnulment = merchant.IsAnnulment;
                }

                await _merchantRepository.UpdateRangeAsync(merchantList);
            }
        }

        private async Task UpdateCustomerAsync(Merchant merchant)
        {
            var customer = await _customerService.GetCustomerAsync(merchant.Customer.CustomerId);
            var customerRequest = PopulateCustomerRequest(customer);
            var pfProduct = customerRequest.CreateCustomerProducts.FirstOrDefault(s => s.MerchantId == merchant.Id);
            if (pfProduct is null)
            {
                pfProduct = new CustomerProductDto
                {
                    OpeningDate = DateTime.Now,
                    MerchantId = merchant.Id,
                    ProductType = ProductType.PF,
                    CustomerProductStatus = CustomerProductStatus.Active
                };
                customerRequest.CreateCustomerProducts.Add(pfProduct);
            }
            var productStatus = merchant.MerchantStatus switch
            {
                MerchantStatus.Active => CustomerProductStatus.Active,
                MerchantStatus.Annulment => CustomerProductStatus.Suspended,
                _ => CustomerProductStatus.Inactive
            };

            if (pfProduct.CustomerProductStatus != productStatus)
            {
                switch (productStatus)
                {
                    case CustomerProductStatus.Suspended:
                        pfProduct.SuspendedDate = DateTime.Now;
                        pfProduct.RecordStatus = RecordStatus.Passive;
                        break;
                    case CustomerProductStatus.Inactive:
                        pfProduct.ClosingDate = DateTime.Now;
                        pfProduct.RecordStatus = RecordStatus.Passive;
                        break;
                    default:
                        pfProduct.ReopeningDate = DateTime.Now;
                        pfProduct.RecordStatus = RecordStatus.Active;
                        break;
                }

                pfProduct.CustomerProductStatus = productStatus;
            }
            await _customerService.CreateCustomerAsync(customerRequest);
        }
        
        private CreateCustomerRequest PopulateCustomerRequest(CustomerDto customer) =>
            new()
            {
                UserId = _applicationUserService.ApplicationUserId,
                CustomerId = customer.Id,
                CommercialTitle = customer.CommercialTitle,
                TradeRegistrationNumber = customer.TradeRegistrationNumber,
                TaxAdministration = customer.TaxAdministration,
                TaxNumber = customer.TaxNumber,
                IdentityNumber = customer.IdentityNumber,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                DocumentType = customer.DocumentType,
                SerialNumber = customer.SerialNumber,
                BirthDate = customer.BirthDate,
                Profession = customer.Profession,
                NationCountryId = customer.NationCountryId,
                NationCountry = customer.NationCountry,
                CustomerType = customer.CustomerType,
                CreateCustomerProducts = customer.CustomerProducts
            }; 
    }
}

