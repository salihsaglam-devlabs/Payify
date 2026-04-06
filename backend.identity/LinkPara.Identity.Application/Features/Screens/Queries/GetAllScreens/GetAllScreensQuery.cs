using AutoMapper;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Screens.Queries.GetScreensByRoleId;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Application.Features.Screens.Queries.GetAllScreens
{
    public class GetAllScreensQuery : IRequest<List<ScreenDto>>
    {
        public Guid RoleId { get; set; }
    }

    public class GetAllScreensQueryHandler : IRequestHandler<GetAllScreensQuery, List<ScreenDto>>
    {
        private readonly IRepository<Screen> _screenRepository;
        private readonly IMapper _mapper;

        public GetAllScreensQueryHandler(IRepository<Screen> screenRepository,
            IMapper mapper)
        {
            _screenRepository = screenRepository;
            _mapper = mapper;
        }
        public async Task<List<ScreenDto>> Handle(GetAllScreensQuery request, CancellationToken cancellationToken)
        {
            var screens = await _screenRepository.GetAll()
                          .OrderBy(screen => screen.ModulePriority)
                          .ThenBy(screen => screen.Priority)
                          .ToListAsync();


            return _mapper.Map<List<Screen>, List<ScreenDto>>(screens);
        }
    }
}
