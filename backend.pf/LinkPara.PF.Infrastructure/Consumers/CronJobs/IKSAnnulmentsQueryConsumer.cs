using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.HttpProviders.IKS;
using LinkPara.HttpProviders.IKS.Models.Request;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs
{
    public class IKSAnnulmentsQueryConsumer : IConsumer<IKSAnnulmentsQuery>
    {
        private readonly ILogger<IKSAnnulmentsQueryConsumer> _logger;
        private readonly IGenericRepository<Merchant> _merchantRepository;
        private readonly IIKSService _iKSService;
        private readonly ICustomerService _customerService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IBus _bus;

        public IKSAnnulmentsQueryConsumer(ILogger<IKSAnnulmentsQueryConsumer> logger,
                                                         IGenericRepository<Merchant> merchantRepository,
                                                         IIKSService iKSService,
                                                         ICustomerService customerService,
                                                         IApplicationUserService applicationUserService,
                                                         IBus bus)
        {
            _logger = logger;
            _merchantRepository = merchantRepository;
            _iKSService = iKSService;
            _customerService = customerService;
            _applicationUserService = applicationUserService;
            _bus = bus;
        }

        public async Task Consume(ConsumeContext<IKSAnnulmentsQuery> context)
        {
            await AnnulmentsQueryAsync();
        }

        private async Task AnnulmentsQueryAsync()
        {
            try
            {
                var merchants = await _merchantRepository.GetAll()
                                                          .Include(m => m.Customer)
                                                          .Where(b => b.RecordStatus == RecordStatus.Active
                                                                && b.MerchantStatus == MerchantStatus.Active
                                                                && !String.IsNullOrEmpty(b.GlobalMerchantId))
                                                          .ToListAsync();
                if (merchants.Any())
                {
                    try
                    {
                        var updatedMerchants = await AnnulmentQueryItemAsync(merchants);
                        
                        if (updatedMerchants.Any())
                        {
                            await _merchantRepository.UpdateRangeAsync(updatedMerchants);
                        
                            foreach (var merchant in updatedMerchants)
                            {
                                await UpdateCustomerAsync(merchant);
                            }
                        
                            var merchantStrings = 
                                string.Join("</br>", updatedMerchants.Select(s => $"{s.Name} - {s.Number}").ToList());

                            try
                            {
                                await _bus.Publish(new IKSAnnulment
                                {
                                    MerchantName = merchantStrings
                                });                      
                            }
                            catch (Exception exception)
                            {
                                _logger.LogError($"IKSAnnulmentsQueryConsumer: {exception}");
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError($"AnnulmentsQuery Consumer Error {exception}");
                    }
                    
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"AnnulmentsQuery Consumer Error {exception}");
            }
        }

        private async Task<List<Merchant>> AnnulmentQueryItemAsync(List<Merchant> merchants)
        {
            try
            {
                var updatedMerchants = new List<Merchant>();
                
                foreach (var merchant in merchants)
                {
                    var annulmentsQuery = await _iKSService.AnnulmentQueryAsync(new IKSAnnulmentsQueryRequest
                    {
                        GlobalMerchantId = merchant.GlobalMerchantId,
                    });

                    var annulment = annulmentsQuery?.Data?.Annulments.FirstOrDefault(x => x.InformType == ((int)InformType.InformTypeAnnulment).ToString()
                        && !x.OwnAnnulment);

                    if (annulment != null && (!merchant.IsAnnulment.HasValue || merchant.IsAnnulment.HasValue==false))
                    {
                        merchant.AnnulmentAdditionalInfo = "Otomatik yapılan fesih sorgulama sonucunda firma pasife alınmıştır.";
                        merchant.MerchantStatus = MerchantStatus.Closed;

                        updatedMerchants.Add(merchant);
                    }
                }

                return updatedMerchants;
            }
            catch (Exception exception)
            {
                _logger.LogError($"AnnulmentsQueryItem Error {exception}");
                throw;
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
