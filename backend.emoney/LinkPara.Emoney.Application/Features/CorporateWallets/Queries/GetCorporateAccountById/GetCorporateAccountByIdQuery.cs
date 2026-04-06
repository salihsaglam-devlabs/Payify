using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Documents;
using LinkPara.HttpProviders.Documents.Models;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Queries.GetCorporateAccountById;

public class GetCorporateAccountByIdQuery : IRequest<CorporateAccountDto>
{
    public Guid Id { get; set; }
}

public class GetCorporateAccountByIdQueryHandler : IRequestHandler<GetCorporateAccountByIdQuery, CorporateAccountDto>
{
    private readonly IGenericRepository<Account> _repository;
    private readonly IGenericRepository<AccountUser> _userRepository;
    private readonly IMapper _mapper;
    private readonly IDocumentService _documentService;
    public GetCorporateAccountByIdQueryHandler(IGenericRepository<Account> repository, 
        IMapper mapper, 
        IGenericRepository<AccountUser> userRepository, 
        IDocumentService documentService)
    {
        _repository = repository;
        _mapper = mapper;
        _userRepository = userRepository;
        _documentService = documentService;
    }
    public async Task<CorporateAccountDto> Handle(GetCorporateAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetAll()
            .Where(x => x.AccountType == AccountType.Corporate && x.Id == request.Id)
            .Include(x => x.CompanyPool)
            .FirstOrDefaultAsync();

        if(account is null)
        {
            throw new NotFoundException(nameof(Account), request.Id);
        }

        var users = await _userRepository.GetAll()
            .Where(x => x.AccountId == account.Id)
            .ToListAsync();

        var result = _mapper.Map<CorporateAccountDto>(account);

        result.Users = _mapper.Map<List<CorporateWalletUserDto>>(users);

        var documents = await _documentService.GetDocuments(new GetDocumentListRequest { AccountId = account.Id });
        result.Documents = documents.Items;

        return result;
    }
}
