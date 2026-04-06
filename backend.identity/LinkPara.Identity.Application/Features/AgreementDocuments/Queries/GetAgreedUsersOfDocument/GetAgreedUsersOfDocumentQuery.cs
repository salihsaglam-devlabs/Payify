using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetAgreedUsersOfDocument;
public class GetAgreedUsersOfDocumentQuery : SearchQueryParams, IRequest<PaginatedList<AgreementUserDto>>
{
    public Guid Id { get; set; }
}

public class GetAgreedUsersOfDocumentQueryHandler : IRequestHandler<GetAgreedUsersOfDocumentQuery, PaginatedList<AgreementUserDto>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<UserAgreementDocument> _userAgreementRepository;
    private readonly IRepository<AgreementDocumentVersion> _agreementDocumentVersionRepository;
    private readonly IContextProvider _contextProvider;

    public GetAgreedUsersOfDocumentQueryHandler(IMapper mapper,
        IRepository<UserAgreementDocument> userAgreementRepository,
        IRepository<AgreementDocumentVersion> agreementDocumentVersionRepository,
        IContextProvider contextProvider)
    {
        _mapper = mapper;
        _userAgreementRepository = userAgreementRepository;
        _agreementDocumentVersionRepository = agreementDocumentVersionRepository;
        _contextProvider = contextProvider;
    }

    public async Task<PaginatedList<AgreementUserDto>> Handle(GetAgreedUsersOfDocumentQuery request, CancellationToken cancellationToken)
    {
        var languageCode = string.IsNullOrEmpty(_contextProvider.CurrentContext.Language)
                            ? "tr"
                            : _contextProvider.CurrentContext.Language.Substring(0, 2);

        var latestVersionDocument = await _agreementDocumentVersionRepository
            .GetAll()
            .Where(f => f.LanguageCode.ToLower() == languageCode.ToLower() 
                                   && f.RecordStatus == RecordStatus.Active
                                   && f.IsLatest
                                   && f.AgreementDocumentId == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestVersionDocument is null)
        {
            throw new NotFoundException(nameof(AgreementDocumentVersion));
        }

        var userList = _userAgreementRepository
            .GetAll()
            .Include(x => x.User)
            .Where(x => x.AgreementDocumentVersionId == latestVersionDocument.Id 
                        && x.RecordStatus == RecordStatus.Active 
                        && x.User.RecordStatus == RecordStatus.Active
                        && x.AgreementDocumentVersionId == latestVersionDocument.Id)
            .Select(x => x.User);

        return await userList.PaginatedListWithMappingAsync<User,AgreementUserDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
