using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Commons.Interfaces.HttpClients;
using LinkPara.CampaignManagement.Application.Features.IWalletCards;
using LinkPara.CampaignManagement.Application.Features.IWalletCards.Commands;
using LinkPara.CampaignManagement.Application.Features.IWalletCards.Queries.GetCard;
using LinkPara.CampaignManagement.Domain.Entities;
using LinkPara.CampaignManagement.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit.Initializers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.CampaignManagement.Infrastructure.Services;

public class IWalletCardService : IIWalletCardService
{
    private readonly IGenericRepository<IWalletCard> _cardRepository;
    private readonly IIWalletHttpClient _iwalletHttpClient;
    private readonly IApplicationUserService _applicationUserService;

    public IWalletCardService(IGenericRepository<IWalletCard> cardRepository,
        IIWalletHttpClient iwalletHttpClient,
        IApplicationUserService applicationUserService)
    {
        _cardRepository = cardRepository;
        _iwalletHttpClient = iwalletHttpClient;
        _applicationUserService = applicationUserService;
    }

    public async Task<IWalletCard> CreateCardAsync(CreateCardCommand request)
    {
        var card = PopulateUserIWalletCard(request);
        await _cardRepository.AddAsync(card);

        card = await _iwalletHttpClient.CreateCardAsync(card);

        if (!string.IsNullOrWhiteSpace(card.CardNumber))
        {
            card.CardApplicationStatus = CardApplicationStatus.Completed;
        }
        else
        {
            card.CardApplicationStatus = CardApplicationStatus.Error;
        }

        await _cardRepository.UpdateAsync(card);

        if (card.CardApplicationStatus != CardApplicationStatus.Completed)
        {
            throw new InvalidOperationException();
        }
        return card;
    }

    public async Task<string> GetPhoneNumberAsync(string walletNumber)
    {
        var phoneNumber = await _cardRepository
            .GetAll()
            .Where(x => x.RecordStatus == RecordStatus.Active
                     && x.WalletNumber == walletNumber
                     && x.CardId > 0)
            .SingleOrDefaultAsync()
            .Select(x => x.PhoneNumber);

        if (phoneNumber is null)
        {
            throw new NotFoundException(nameof(IWalletCard), walletNumber);
        }

        return phoneNumber;
    }

    public async Task<IWalletCardDto> GetUserCardAsync(GetCardQuery request)
    {
        var cardInfo = await _cardRepository
            .GetAll()
            .Where(x => x.RecordStatus == RecordStatus.Active
                     && x.UserId == request.UserId
                     && x.CardId > 0)
            .SingleOrDefaultAsync()
            .Select(x => new { x.Id, x.CardId, x.CardNumber });

        if (cardInfo is null)
        {
            return new IWalletCardDto
            {
                IsExistingCustomer = false
            };
        }

        return new IWalletCardDto { Id = cardInfo.Id, CardId = cardInfo.CardId, CardNumber = cardInfo.CardNumber, IsExistingCustomer = true };
    }

    private IWalletCard PopulateUserIWalletCard(CreateCardCommand request)
    {
        var fullName = request.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new IWalletCard
        {
            AddressDetail = request.AddressDetail,
            CardApplicationStatus = CardApplicationStatus.Initialize,
            CityId = request.CityId,
            Email = request.Email,
            FullName = string.Join(" ",fullName),
            IdentityNumber = request.IdentityNumber,
            IndividualFrameworkAgreementVersion = request.IndividualFrameworkAgreementVersion,
            IsApprovedIndividualFrameworkAgreement = request.IsApprovedIndividualFrameworkAgreement,
            IsApprovedKvkkAgreement = request.IsApprovedKvkkAgreement,
            IsApprovedPreliminaryInformationAgreement = request.IsApprovedPreliminaryInformationAgreement,
            KvkkAgreementVersion = request.KvkkAgreementVersion,
            PhoneNumber = request.PhoneNumber,
            PreliminaryInformationAgreementVersion = request.PreliminaryInformationAgreementVersion,
            TownId = request.TownId,
            UserId = request.UserId,
            WalletNumber = request.WalletNumber,
            UserType = request.UserType,
            CommercialElectronicCommunicationAggrementVersion = request.CommercialElectronicCommunicationAggrementVersion,
            IsApprovedCommercialElectronicCommunicationAggrement = request.IsApprovedCommercialElectronicCommunicationAggrement,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
        };
    }

}
