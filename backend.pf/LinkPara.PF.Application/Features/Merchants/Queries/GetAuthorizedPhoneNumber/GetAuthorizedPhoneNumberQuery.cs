using LinkPara.PF.Application.Features.Merchants.Queries.GetMerchantById;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.Merchants.Queries.GetAuthorizedPhoneNumber;

public class GetAuthorizedPhoneNumberQuery : IRequest<string>
{
    public Guid Id { get; set; }
}

public class GetAuthorizedPhoneNumberQueryHandler : IRequestHandler<GetAuthorizedPhoneNumberQuery, string>
{
    private readonly IGenericRepository<Merchant> _repository;

    public GetAuthorizedPhoneNumberQueryHandler(IGenericRepository<Merchant> repository)
    {
        _repository = repository;
    }
    public async Task<string> Handle(GetAuthorizedPhoneNumberQuery request, CancellationToken cancellationToken)
    {
        var merchant = await _repository.GetAll().Include(b => b.Customer)
            .ThenInclude(b => b.AuthorizedPerson)
            .FirstOrDefaultAsync(b => b.Id == request.Id);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), request.Id);
        }

        var phoneNumber = merchant.Customer.AuthorizedPerson.MobilePhoneNumber;
        var phoneCode = merchant.PhoneCode;
        return $"+{phoneCode}{phoneNumber}";
    }
}