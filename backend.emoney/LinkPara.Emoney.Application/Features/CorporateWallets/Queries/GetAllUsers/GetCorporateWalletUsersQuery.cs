using AutoMapper;
using LinkPara.Emoney.Application.Features.CompanyPools;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Queries.GetAllUsers;

public class GetCorporateWalletUsersQuery : SearchQueryParams, IRequest<PaginatedList<CorporateWalletUserDto>>
{
    public Guid AccountId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}


public class GetCorporateWalletUsersQueryHandler : IRequestHandler<GetCorporateWalletUsersQuery, PaginatedList<CorporateWalletUserDto>>
{
    private readonly IGenericRepository<AccountUser> _repository;
    private readonly IMapper _mapper;
    public GetCorporateWalletUsersQueryHandler(IGenericRepository<AccountUser> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<CorporateWalletUserDto>> Handle(GetCorporateWalletUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.GetAll()
            .Where(x => x.AccountId == request.AccountId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.FullName))
        {
            query = query.Where(x => x.Firstname.Contains(request.FullName)
                || x.Lastname.Contains(request.FullName)
                || (x.Firstname + " " + x.Lastname).Contains(request.FullName));
        }
        if (!string.IsNullOrEmpty(request.Email))
        {
            query = query.Where(s => s.Email.ToLower().Contains(request.Email.ToLower()));
        }
        if (!string.IsNullOrEmpty(request.PhoneCode))
        {
            query = query.Where(s => s.PhoneCode.ToLower().Contains(request.PhoneCode.ToLower()));
        }
        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            query = query.Where(s => s.PhoneNumber.ToLower().Contains(request.PhoneNumber.ToLower()));
        }
        if (request.RecordStatus.HasValue)
        {
            query = query.Where(s => s.RecordStatus == request.RecordStatus);
        }

        return await query
           .PaginatedListWithMappingAsync<AccountUser, CorporateWalletUserDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
