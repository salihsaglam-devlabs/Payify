using AutoMapper;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Screens.Queries.GetScreensByUserId
{
    public class GetScreensByUserIdQuery : IRequest<List<ScreenDto>>
    {
        public Guid UserId { get; set; }
    }

    public class GetScreensByUserIdQueryHandler : IRequestHandler<GetScreensByUserIdQuery, List<ScreenDto>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IRepository<RoleScreen> _roleScreenRepository;
        private readonly RoleManager<Role> _roleManager; 
        private readonly IMapper _mapper;

        public GetScreensByUserIdQueryHandler(
            IRepository<RoleScreen> roleScreenRepository,
            RoleManager<Role> roleManager,
            UserManager<User> userManager,
            IMapper mapper)
        {
            _roleScreenRepository = roleScreenRepository;
            _roleManager = roleManager;
            _userManager = userManager;
            _mapper = mapper;
        }
        public async Task<List<ScreenDto>> Handle(GetScreensByUserIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
            {
                throw new NotFoundException(nameof(User));
            }

            var roleNames = await _userManager.GetRolesAsync(user);

            var roleIds = new List<string>();

            if (roleNames.Any())
            {
                foreach (var roleName in roleNames)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        roleIds.Add(role.Id.ToString());
                    }
                }
            }
            else
            {
                throw new NotFoundException(nameof(Role), request.UserId);

            }

            var screens = await _roleScreenRepository.GetAll()
                .Where(r => roleIds.Contains(r.RoleId.ToString()))
                .Select(rs => rs.Screen)
                .OrderBy(screen => screen.ModulePriority) 
                .ThenBy(screen => screen.Priority)
                .Distinct()
                .ToListAsync();

            return _mapper.Map<List<Screen>, List<ScreenDto>>(screens);
        }
    }
}
