using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.Merchants.Command.UpdateMerchantIKS
{
    public class UpdateMerchantIKSCommand : IRequest
    {
        public Guid Id { get; set; }
        public string GlobalMerchantId { get; set; }
        public MerchantStatus MerchantStatus { get; set; }
    }

    public class UpdateMerchantIKSCommandHandler : IRequestHandler<UpdateMerchantIKSCommand>
    {
        private readonly IGenericRepository<Merchant> _merchantRepository;
        private readonly ILogger<Merchant> _logger;
        private readonly ICustomerService _customerService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IIksPfService _iksPfService;

        public UpdateMerchantIKSCommandHandler(IGenericRepository<Merchant> merchantRepository,
                                                ILogger<Merchant> logger,
                                                ICustomerService customerService,
                                                IApplicationUserService applicationUserService, 
                                                IIksPfService iksPfService)
        {
            _merchantRepository = merchantRepository;
            _logger = logger;
            _customerService = customerService;
            _applicationUserService = applicationUserService;
            _iksPfService = iksPfService;
        }
        public async Task<Unit> Handle(UpdateMerchantIKSCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var merchant = await _merchantRepository.GetAll()
                    .Include(b => b.MerchantVposList)
                        .ThenInclude(c => c.Vpos)
                    .Include(b => b.Customer)
                        .ThenInclude(b => b.AuthorizedPerson)
                    .Where(x => x.Id == request.Id && x.RecordStatus == RecordStatus.Active)
                    .FirstOrDefaultAsync(cancellationToken);

                if (merchant == null)
                {
                    throw new NotFoundException(nameof(merchant));
                }

                merchant.MerchantStatus = request.MerchantStatus;
                merchant.GlobalMerchantId = request.GlobalMerchantId;
                await _merchantRepository.UpdateAsync(merchant);
                if (request.MerchantStatus is MerchantStatus.Closed or MerchantStatus.Reject or MerchantStatus.Annulment)
                {
                    await UpdateCustomerAsync(merchant);
                }

                if (request.MerchantStatus is MerchantStatus.Active)
                {
                    await _iksPfService.IKSCreateTerminalAsync(merchant);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "UpdateMerchantIKSError : {Exception}", exception);
                throw;
            }

            return Unit.Value;

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
