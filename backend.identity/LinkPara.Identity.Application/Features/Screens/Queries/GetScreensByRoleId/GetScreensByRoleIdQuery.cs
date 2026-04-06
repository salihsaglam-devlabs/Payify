using AutoMapper;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Roles.Queries.GetUsersByRoleId;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Application.Features.Screens.Queries.GetScreensByRoleId
{
    public class GetScreensByRoleIdQuery : IRequest<RoleScreenDto>
    {
        public Guid RoleId { get; set; }
    }

    public class GetScreensByRoleIdQueryHandler : IRequestHandler<GetScreensByRoleIdQuery, RoleScreenDto>
    {
        private readonly IRepository<RoleScreen> _roleScreenRepository;
        private readonly RoleManager<Role> _roleManager;

        private readonly IMapper _mapper;

        public GetScreensByRoleIdQueryHandler(
            IRepository<RoleScreen> roleScreenRepository,
            RoleManager<Role> roleManager,
            IMapper mapper)
        {
            _roleScreenRepository = roleScreenRepository;
            _roleManager = roleManager;
            _mapper = mapper;
        }
        public async Task<RoleScreenDto> Handle(GetScreensByRoleIdQuery request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());

            if (role == null)
            {
                throw new NotFoundException(nameof(Role), request.RoleId);
            }

            var screens = await _roleScreenRepository.GetAll()
                .Where(r => r.RoleId == request.RoleId)
                .Select(rs => rs.Screen)
                .OrderBy(screen => screen.ModulePriority)
                .ThenBy(screen => screen.Priority)
                .ToListAsync();

            return new RoleScreenDto()
            { 
             Id =role.Id,
             RoleName = role.Name,
             RoleScope = role.RoleScope,
             CanSeeSensitiveData =role.CanSeeSensitiveData,
             Screens = _mapper.Map<List<Screen>, List<ScreenDto>>(screens)
        };
        }
    }
}
