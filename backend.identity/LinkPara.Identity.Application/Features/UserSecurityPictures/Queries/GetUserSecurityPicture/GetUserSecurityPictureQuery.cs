using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.SecurityPictures.Queries;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.UserSecurityPictures.Queries.GetUserSecurityPicture;

public class GetUserSecurityPictureQuery : IRequest<SecurityPictureDto>
{
    public Guid UserId { get; set; }
}

public class GetUserSecurityPictureQueryHandler : IRequestHandler<GetUserSecurityPictureQuery, SecurityPictureDto>
{
    private readonly IRepository<UserSecurityPicture> _userSecurityPictureRepository;

    public GetUserSecurityPictureQueryHandler(IRepository<UserSecurityPicture> userSecurityPictureRepository)
    {
        _userSecurityPictureRepository = userSecurityPictureRepository;
    }

    public async Task<SecurityPictureDto> Handle(GetUserSecurityPictureQuery request, CancellationToken cancellationToken)
    {
        var result = await _userSecurityPictureRepository.GetAll()
            .Where(x => x.UserId == request.UserId && x.RecordStatus == RecordStatus.Active)
            .Select(x => new SecurityPictureDto
            {
                Id = x.SecurityPicture.Id,
                Name = x.SecurityPicture.Name,
                Bytes = x.SecurityPicture.Bytes,
                ContentType = x.SecurityPicture.ContentType,
                RecordStatus = x.SecurityPicture.RecordStatus
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }
}
