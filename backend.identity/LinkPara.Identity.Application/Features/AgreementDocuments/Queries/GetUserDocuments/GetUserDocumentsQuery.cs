using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetUserDocuments;

public class GetUserDocumentsQuery : IRequest<List<UserAgreementDocumentsStatusDto>>
{
    [FromRoute]
    public Guid UserId { get; set; }
    public bool GetOptionalDocuments { get; set; }
}

public class GetUserDocumentsQueryHandler : IRequestHandler<GetUserDocumentsQuery, List<UserAgreementDocumentsStatusDto>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<UserAgreementDocument> _userAgreementDocumentRepository;
    private readonly IRepository<AgreementDocumentVersion> _documentVersionRepository;
    private readonly UserManager<User> _userManager;
    private readonly IContextProvider _contextProvider;

    public GetUserDocumentsQueryHandler(IMapper mapper,
        IRepository<UserAgreementDocument> userAgreementDocumentRepository,
        IRepository<AgreementDocumentVersion> documentVersionRepository,
        UserManager<User> userManager
,
        IContextProvider contextProvider)
    {
        _userAgreementDocumentRepository = userAgreementDocumentRepository;
        _documentVersionRepository = documentVersionRepository;
        _mapper = mapper;
        _userManager = userManager;
        _contextProvider = contextProvider;
    }

    public async Task<List<UserAgreementDocumentsStatusDto>> Handle(GetUserDocumentsQuery query, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(query.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), query.UserId);
        }
        var productType = user.UserType switch
        {
            UserType.CorporateWallet => ProductType.CorporateWallet,
            UserType.Corporate => ProductType.PF,
            UserType.CorporateSubMerchant => ProductType.PF,
            _ => ProductType.Emoney,
        };

        var languageCode = string.IsNullOrEmpty(_contextProvider.CurrentContext.Language)
                            ? "tr"
                            : _contextProvider.CurrentContext.Language.Substring(0, 2);

        var userDocumentList = await _userAgreementDocumentRepository.GetAll()
            .Where(q => q.UserId == query.UserId)?
            .ToListAsync(cancellationToken);

        var userDocumentListVersionIds = userDocumentList
            .Where(q => q.UserId == query.UserId)?
            .Select(q => q.AgreementDocumentVersionId)
            .ToList();

        var productsWithJoin = _documentVersionRepository.GetAll()
            .Include(q => q.AgreementDocument)
            .Where(q => q.IsLatest && q.LanguageCode == languageCode && q.AgreementDocument.ProductType == productType);

        if (!query.GetOptionalDocuments)
        {
            productsWithJoin = productsWithJoin.Where(q => q.IsOptional == false);
        }

        var productsWithJoinList = await productsWithJoin.ToListAsync(cancellationToken);

        var result = productsWithJoinList.Select(q =>
        {
            var userDoc = userDocumentList.FirstOrDefault(x => x.AgreementDocumentVersionId == q.Id);
            return new UserAgreementDocumentsStatusDto
            {
                AgreementDocumentId = q.AgreementDocumentId,
                Title = q.Title,
                IsForceUpdate = q.IsForceUpdate,
                IsLatest = q.IsLatest,
                Version = q.AgreementDocument.LastVersion,
                IsSigned = userDocumentListVersionIds.Contains(q.Id),
                Content = q.Content,
                SignedAt = userDoc?.CreateDate,
                ApprovalChannel = userDoc?.ApprovalChannel
            };
        }).ToList();

        return _mapper.Map<List<UserAgreementDocumentsStatusDto>>(result);
    }
}
