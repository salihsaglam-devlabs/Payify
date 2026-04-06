using AutoMapper;
using LinkPara.Audit;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Chargebacks;
using LinkPara.Emoney.Application.Features.Chargebacks.Commands;
using LinkPara.Emoney.Application.Features.Chargebacks.Queries;
using LinkPara.Emoney.Application.Features.Provisions.Commands.ProvisionCashback;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Documents;
using LinkPara.HttpProviders.Documents.Models;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.PF;
using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OnUsPaymentRequest = LinkPara.Emoney.Domain.Entities.OnUsPaymentRequest;
using OnUsPaymentStatus = LinkPara.Emoney.Domain.Enums.OnUsPaymentStatus;

namespace LinkPara.Emoney.Infrastructure.Services;

public class ChargebackService : IChargebackService
{
    private const string GeneralErrorCode = "500";
    private readonly IGenericRepository<Chargeback> _chargebackRepository;
    private readonly IGenericRepository<ChargebackDocument> _chargebackDocumentRepository;
    private readonly IGenericRepository<Transaction> _transactionRepository;
    private readonly IGenericRepository<Provision> _provisionRepository;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IGenericRepository<OnUsPaymentRequest> _onUsPaymentRequestRepository;
    private readonly IProvisionService _provisionService;
    private readonly IDocumentService _documentService;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IPfOnUsService _pfOnUsService;
    private readonly IMapper _mapper;

    public ChargebackService(IGenericRepository<Chargeback> chargebackRepository,
        IGenericRepository<Wallet> walletRepository,
        IProvisionService provisionService,
        IGenericRepository<AccountUser> accountUserRepository,
        IContextProvider contextProvider,
        IPfOnUsService pfOnUsService,
        IGenericRepository<Transaction> transactionRepository,
        IGenericRepository<OnUsPaymentRequest> onUsPaymentRequestRepository,
        IDocumentService documentService,
        IGenericRepository<Provision> provisionRepository,
        IMapper mapper,
        IGenericRepository<ChargebackDocument> chargebackDocumentRepository)
    {
        _chargebackRepository = chargebackRepository;
        _walletRepository = walletRepository;
        _provisionService = provisionService;
        _accountUserRepository = accountUserRepository;
        _contextProvider = contextProvider;
        _pfOnUsService = pfOnUsService;
        _transactionRepository = transactionRepository;
        _onUsPaymentRequestRepository = onUsPaymentRequestRepository;
        _documentService = documentService;
        _provisionRepository = provisionRepository;
        _mapper = mapper;
        _chargebackDocumentRepository = chargebackDocumentRepository;
    }

    public async Task<PaginatedList<ChargebackDto>> GetChargebackAsync(GetChargebackQuery request)
    {
        var list = _chargebackRepository.GetAll()
            .Where(x => x.RecordStatus == RecordStatus.Active);

        if (request.TransactionId != null)
        {
            list = list.Where(s => s.TransactionId == new Guid(request.TransactionId.ToString()));
        }

        if (request.TransactionType != null)
        {
            list = list.Where(s => s.TransactionType == request.TransactionType);
        }

        if (request.Status != null)
        {
            list = list.Where(s => s.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.OrderId))
        {
            list = list.Where(s => (s.OrderId).Contains(request.OrderId));
        }

        if (!string.IsNullOrEmpty(request.WalletNumber))
        {
            list = list.Where(s => (s.WalletNumber).Contains(request.WalletNumber));
        }

        if (!string.IsNullOrEmpty(request.MerchantName))
        {
            list = list.Where(s => (s.MerchantName).Contains(request.MerchantName));
        }

        if (!string.IsNullOrEmpty(request.UserName))
        {
            list = list.Where(s => (s.UserName).Contains(request.UserName));
        }

        if (request.TransactionDate != null)
        {
            list = list.Where(s => s.TransactionDate.Date == Convert.ToDateTime(request.TransactionDate).Date);
        }

        return await list
                    .PaginatedListWithMappingAsync<Chargeback, ChargebackDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<ChargebackDto> InitChargebackAsync(InitChargebackCommand request)
    {
        var onUsPayment = new OnUsPaymentRequest();
        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);

            if (transaction is null)
            {
                throw new NotFoundException("TransactionId : ", request.TransactionId);
            }

            var wallet = await _walletRepository.GetAll()
                .FirstOrDefaultAsync(s => s.WalletNumber == request.WalletNumber);

            if (wallet is null)
            {
                throw new NotFoundException("WalletNumber : ", request.WalletNumber);
            }

            var accountUser = await _accountUserRepository.GetAll()
                .FirstOrDefaultAsync(s => s.AccountId == wallet.AccountId);

            if (accountUser == null)
            {
                throw new NotFoundException(nameof(Account), wallet.AccountId);
            }

            var userId = accountUser.UserId;
            
            if (transaction.TransactionType == TransactionType.OnUs)
            {
                onUsPayment = await _onUsPaymentRequestRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.TransactionId == request.TransactionId
                                           && x.Status == OnUsPaymentStatus.Success);
                var holdMerchantTransaciton = new JsonPatchDocument<UpdateMerchantTransactionRequest>();
                holdMerchantTransaciton.Replace(x => x.IsSuspecious, false);
                holdMerchantTransaciton.Replace(x => x.IsChargeback, true);
                holdMerchantTransaciton.Replace(x => x.SuspeciousDescription, request.Description);

                var pfHoldResponse = await _pfOnUsService.ChargebackOnUsPayment(new Guid(onUsPayment.MerchantTransactionId),
                                                                                holdMerchantTransaciton);

                onUsPayment.Status = OnUsPaymentStatus.Suspecious;
                onUsPayment.ChargebackDescription = request.Description;
                onUsPayment.UpdateDate = DateTime.Now;
                onUsPayment.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? onUsPayment.LastModifiedBy;
            }

            var chargeback = new Chargeback
            {
                TransactionId = request.TransactionId,
                UserName = string.Concat(accountUser.Firstname, " ", accountUser.Lastname),
                Amount = transaction.Amount,
                Currency = transaction.CurrencyCode,
                WalletNumber = request.WalletNumber,
                Status = ChargebackStatus.Pending,
                TransactionType = transaction.TransactionType,
                Description = request.Description,
                UserId = accountUser.UserId,
                MerchantId = request.MerchantId,
                MerchantName = transaction.Tag ?? string.Empty,
                TransactionDate = transaction.TransactionDate,
                OrderId = onUsPayment.OrderId
            };
            await _chargebackRepository.AddAsync(chargeback);
            await _onUsPaymentRequestRepository.UpdateAsync(onUsPayment);

            return _mapper.Map<ChargebackDto>(chargeback);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<ChargebackDto> ApproveChargebackAsync(ApproveChargebackCommand request)
    {
        try
        {
            if (request.Status == ChargebackStatus.Pending)
            {
                throw new InvalidParameterException(nameof(ChargebackStatus.Pending));
            }

            var chargeback = await _chargebackRepository.GetAll()
                .FirstOrDefaultAsync(x => x.TransactionId == request.TransactionId
                                            && x.Status == ChargebackStatus.Pending);
            

            if (chargeback is null)
            {
                throw new NotFoundException("TransactionId : ", request.TransactionId);
            }

            var onUsPayment = await _onUsPaymentRequestRepository.GetAll()
                        .FirstOrDefaultAsync(x => x.TransactionId == chargeback.TransactionId
                                               && x.Status == OnUsPaymentStatus.Suspecious);
            if (onUsPayment is null)
            {
                throw new NotFoundException(nameof(onUsPayment));
            }

            if (request.Status == ChargebackStatus.Chargeback)
            {
                var provision = await _provisionRepository.GetAll().
                    FirstOrDefaultAsync(x => x.TransactionId == chargeback.TransactionId);

                if (provision is null)
                {
                    throw new NotFoundException(nameof(provision));
                }

                await _provisionService.ProvisionChargebackAsync(new ProvisionChargebackCommand
                {
                    UserId = provision.UserId,
                    WalletNumber = provision.WalletNumber,
                    Amount = chargeback.Amount,
                    CurrencyCode = provision.CurrencyCode,
                    ProvisionSource = ProvisionSource.Onus,
                    Description = chargeback.Description,
                    ConversationId = provision.ConversationId,
                    ClientIpAddress = provision.ClientIpAddress,
                    ProvisionReference = provision.ProvisionReference
                }, new CancellationToken());

                chargeback.Status = ChargebackStatus.Chargeback;
                onUsPayment.Status = OnUsPaymentStatus.Chargeback;
            }

            if (request.Status == ChargebackStatus.Cancel)
            {
                var cancelMerchantTransaciton = new JsonPatchDocument<UpdateMerchantTransactionRequest>();
                cancelMerchantTransaciton.Replace(x => x.IsSuspecious, false);
                cancelMerchantTransaciton.Replace(x => x.IsChargeback, false);
                cancelMerchantTransaciton.Replace(x => x.SuspeciousDescription, request.Description);

                var pfCancelResponse = await _pfOnUsService.ChargebackOnUsPayment(new Guid(onUsPayment.MerchantTransactionId),
                                                                                cancelMerchantTransaciton);

                chargeback.Status = ChargebackStatus.Cancel;
                onUsPayment.Status = OnUsPaymentStatus.Success;
                onUsPayment.ChargebackDescription = string.Empty;
            }

            chargeback.UpdateDate = DateTime.Now;
            chargeback.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? chargeback.LastModifiedBy;
            await _chargebackRepository.UpdateAsync(chargeback);

            onUsPayment.UpdateDate = DateTime.Now;
            onUsPayment.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? onUsPayment.LastModifiedBy;
            await _onUsPaymentRequestRepository.UpdateAsync(onUsPayment);

            return _mapper.Map<ChargebackDto>(chargeback);
        }
        catch(Exception ex)
        { 
            throw;
        }
    }

    public async Task<ChargebackDocumentDto> AddChargebackDocumentAsync(AddChargebackDocumentCommand request)
    {
        try
        {
            var chargeback = await _chargebackRepository.GetByIdAsync(request.ChargebackId);

            if (chargeback == null)
            {
                throw new NotFoundException("ChargebackId : ", request.ChargebackId);
            }

            var document = new CreateDocumentRequest()
            {
                OriginalFileName = request.OriginalFileName,
                ContentType = request.ContentType,
                Bytes = request.Bytes,
                UserId = chargeback.UserId,
                MerchantId = string.IsNullOrEmpty(chargeback.MerchantId) ? null : new Guid(chargeback.MerchantId),
                DocumentTypeId = request.DocumentTypeId
            };

            var documentResponse = await _documentService.CreateDocument(document);

            var chargebackDocument = new ChargebackDocument
            {
                ChargebackId = chargeback.Id,
                TransactionId = chargeback.TransactionId,
                DocumentId = documentResponse.Id,
                DocumentTypeId = documentResponse.DocumentTypeId,
                FileName = request.OriginalFileName,
                Description = request.DocumentDescription
            };
            await _chargebackDocumentRepository.AddAsync(chargebackDocument);

            return _mapper.Map<ChargebackDocumentDto>(chargebackDocument);
        }
        catch(Exception ex) 
        { 
            throw; 
        }
    }

    public async Task<bool> DeleteChargebackDocumentAsync(DeleteChargebackDocumentCommand request)
    {
        var chargebackDocument = await _chargebackDocumentRepository.GetByIdAsync(request.ChargebackDocumentId);

        if (chargebackDocument == null)
        {
            throw new NotFoundException("ChargebackDocumentId : ", request.ChargebackDocumentId);
        }

        chargebackDocument.RecordStatus = RecordStatus.Passive;
        chargebackDocument.UpdateDate = DateTime.Now;
        chargebackDocument.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? chargebackDocument.LastModifiedBy;
        await _chargebackDocumentRepository.UpdateAsync(chargebackDocument);

        return true;
    }

    public async Task<List<ChargebackDocumentDto>> GetChargebackDocumentsAsync(GetChargebackDocumentQuery request)
    {
        var chargebackDocumentList = await _chargebackDocumentRepository.GetAll()
                                    .Where(x => x.ChargebackId == request.ChargebackId
                                    && x.RecordStatus == RecordStatus.Active)
                                    .ToListAsync();

        if (chargebackDocumentList == null)
        {
            throw new NotFoundException("ChargebackId : ", request.ChargebackId);
        }

        return _mapper.Map<List<ChargebackDocumentDto>>(chargebackDocumentList);
    }
}
