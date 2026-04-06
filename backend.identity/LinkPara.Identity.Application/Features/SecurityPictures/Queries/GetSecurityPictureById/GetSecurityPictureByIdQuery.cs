using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.SecurityPictures.Queries.GetSecurityPictureById;

public class GetSecurityPictureByIdQuery : IRequest<SecurityPictureDto>
{
    public Guid Id { get; set; }
}

public class GetSecurityPictureByIdQueryHandler : IRequestHandler<GetSecurityPictureByIdQuery, SecurityPictureDto>
{
    private readonly IRepository<SecurityPicture> _repository;
    private readonly IMapper _mapper;

    public GetSecurityPictureByIdQueryHandler(IRepository<SecurityPicture> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SecurityPictureDto> Handle(GetSecurityPictureByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _repository.GetAll()
            .ProjectTo<SecurityPictureDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dto is null)
            throw new NotFoundException(nameof(SecurityPicture));

        return dto;
    }
}
