using AutoMapper;
using LinkPara.SharedModels.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Application.Features.Address.Queries.GetAddressByUserId;

public class GetAddressByUserIdQuery : IRequest<List<UserAddressDto>>
{
    public Guid UserId { get; set; }
}
public class GetAddressByUserIdQueryHandler : IRequestHandler<GetAddressByUserIdQuery, List<UserAddressDto>>
{
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly IRepository<UserAddress> _repository;

    public GetAddressByUserIdQueryHandler(IMapper mapper,
        UserManager<User> userManager,
        IRepository<UserAddress> repository)
    {
        _mapper = mapper;
        _userManager = userManager;
        _repository = repository;
    }

    public async Task<List<UserAddressDto>> Handle(GetAddressByUserIdQuery query, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(query.UserId.ToString());

        var userAddresses = await _repository.GetAll()
            .Where(q => q.UserId == query.UserId)
            .ToListAsync();

        if (userAddresses.Count == 0)
        {
            throw new NotFoundException(nameof(UserAddress),query.UserId);
        }

        return _mapper.Map<List<UserAddress>, List<UserAddressDto>>(userAddresses);
    }
}
