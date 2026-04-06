using LinkPara.Identity.Domain.Entities;
using MediatR;
using LinkPara.Identity.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Application.Features.Account.Queries.GetUserPicture;

public class GetUserPictureQuery : IRequest<UserPictureDto>
{
    public Guid UserId { get; set; }
}

public class GetUserPictureQueryHandler : IRequestHandler<GetUserPictureQuery, UserPictureDto>
{
    private readonly IRepository<UserPicture> _userPictureRepository;

    public GetUserPictureQueryHandler(IRepository<UserPicture> userPictureRepository)
    {
        _userPictureRepository = userPictureRepository;
    }

    public async Task<UserPictureDto> Handle(GetUserPictureQuery query, CancellationToken cancellationToken)
    {
        var userPicture = await _userPictureRepository.GetAll()
            .Select(x => new
            {
                x.UserId,
                x.Bytes,
                x.ContentType
            })
            .FirstOrDefaultAsync(x => x.UserId == query.UserId);

        return new UserPictureDto()
        {
            Bytes = userPicture?.Bytes,
            ContentType = userPicture?.ContentType
        };
    }
}