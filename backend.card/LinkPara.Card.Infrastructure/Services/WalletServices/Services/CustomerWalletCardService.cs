using AutoMapper;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.WalletModels.CardModels;
using LinkPara.Card.Application.Features.WalletServices.CardServices.Commands.UpdateCustomerWalletCardName;
using LinkPara.Card.Application.Features.WalletServices.CardServices.Queries.GetCustomerWalletCardsQuery;
using LinkPara.Card.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.WalletServices.Services;

public class CustomerWalletCardService : ICustomerWalletCardService
{
    private readonly IMapper _mapper;
    private readonly IGenericRepository<CustomerWalletCard> _repository;
    public CustomerWalletCardService(
        IMapper mapper,
        IGenericRepository<CustomerWalletCard> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<List<CustomerWalletCardDto>> GetCustomerWalletCardsAsync(GetCustomerWalletCardsQuery request)
    {
        var cardList = await _repository
            .GetAll()
            .Where(x => x.WalletNumber == request.WalletNumber && 
                        x.IsActive &&
                        x.RecordStatus == RecordStatus.Active)
            .ToListAsync();
        return _mapper.Map<List<CustomerWalletCardDto>>(cardList);
    }

    public async Task UpdateCustomerWalletCardNameAsync(UpdateCustomerWalletCardNameCommand request)
    {
        var card = await _repository
            .GetAll()
            .FirstOrDefaultAsync(x => x.CardNumber == request.CardNumber &&
                                      x.IsActive &&
                                      x.RecordStatus == RecordStatus.Active);

        if (card is null)
        {
            throw new NotFoundException();
        }

        card.CardName = request.CardName;
        await _repository.UpdateAsync(card);
    }
}
