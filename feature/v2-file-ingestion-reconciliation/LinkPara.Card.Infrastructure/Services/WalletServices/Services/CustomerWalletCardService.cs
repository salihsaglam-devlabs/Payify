using AutoMapper;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.WalletMoodels.CardModels;
using LinkPara.Card.Application.Features.WalletServices.CardServices.Queries.GetCustomerWalletCardsQuery;
using LinkPara.Card.Domain.Constants;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.HttpProviders.Vault;
using Microsoft.Extensions.Configuration;

namespace LinkPara.Card.Infrastructure.Services.WalletServices.Services;

public class CustomerWalletCardService : ICustomerWalletCardService
{
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly CardDbContext _dbContext;
    private readonly IVaultClient _vaultClient;

    public CustomerWalletCardService(
        IConfiguration configuration,
        IMapper mapper,
        IVaultClient vaultClient,
        CardDbContext dbContext)
    {
        _configuration = configuration;
        _mapper = mapper;
        _vaultClient = vaultClient;
        _dbContext = dbContext;
    }

    public async Task<GetCustomerWalletCardsResponse> GetCustomerWalletCardsAsync(GetCustomerWalletCardsQuery request)
    {
        try
        {
            GetCustomerWalletCardsResponse response = new GetCustomerWalletCardsResponse(); 
            var cardList = _dbContext.CustomerWalletCard
                .Where(x => x.BankingCustomerNo == request.CustomerNumber && x.IsActive)
                .ToList();

            if (cardList.Any())
            {
                response.CustomerWalletCards = new List<CustomerWalletCardsResponse>();
                response.IsSuccess = true;
                response.Description = ResponseDescription.SUCCESS;

                foreach (var card in cardList)
                {
                    response.CustomerWalletCards.Add(
                        new CustomerWalletCardsResponse
                        {
                            BankingCustomerNo = card.BankingCustomerNo,
                            IsActive = card.IsActive,
                            CardNumber = card.CardNumber,
                            CardStatus = card.CardStatus,
                            ProductCode = card.ProductCode,
                            WalletNumber = card.WalletNumber,
                            CardName = card.CardName
                        });
                }
            } 

            return response;
        }
        catch (Exception)
        {
            return new GetCustomerWalletCardsResponse
            {
                IsSuccess = false,
                Description = ResponseDescription.ERROR
            };
        }
    }
}
