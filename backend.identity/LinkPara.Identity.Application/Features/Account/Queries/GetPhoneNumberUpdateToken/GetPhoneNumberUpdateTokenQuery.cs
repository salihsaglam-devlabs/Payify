using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LinkPara.Identity.Application.Features.Account.Queries.GetPhoneNumberUpdateToken;
public class GetPhoneNumberUpdateTokenQuery : IRequest<UpdatePhoneNumberTokenDto>
{
    public string UserId { get; set; }
    public string NewPhoneNumber { get; set; }
}
public class GetPhoneNumberUpdateTokenQueryHandler : IRequestHandler<GetPhoneNumberUpdateTokenQuery, UpdatePhoneNumberTokenDto>
{
    private readonly UserManager<User> _userManager;
    private readonly IRepository<User> _userRepository;
    public GetPhoneNumberUpdateTokenQueryHandler(
        UserManager<User> userManager,
        IRepository<User> userRepository)
    {
        _userManager = userManager;
        _userRepository = userRepository;
    }
    public async Task<UpdatePhoneNumberTokenDto> Handle(GetPhoneNumberUpdateTokenQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId) ?? throw new UserNotFoundException();

        var dbChecker = await _userRepository
            .GetAll()
            .FirstOrDefaultAsync(x =>
                                 x.PhoneNumber == request.NewPhoneNumber &&
                                 x.UserStatus != UserStatus.Inactive &&
                                 x.UserType == UserType.Individual, cancellationToken);
        if (dbChecker is not null)
        {
            throw new AlreadyInUseException(nameof(request.NewPhoneNumber));
        }

        var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, request.NewPhoneNumber);

        return new UpdatePhoneNumberTokenDto
        {
            Token = Base64UrlEncoder.Encode(token)
        };
    }
}
