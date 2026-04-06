using AutoMapper;
using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Users.Commands.CreateUser;
using LinkPara.Identity.Application.Features.Users.Queries;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MassTransit.Initializers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Data;

namespace LinkPara.Identity.Infrastructure.Services.Approval;

public class UserScreenService : IApprovalScreenService
{

    private readonly IRepository<User> _userRepository;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IStringLocalizer _localizer;

    public UserScreenService(IRepository<User> userRepository,
        IMapper mapper,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IStringLocalizerFactory factory)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _userManager = userManager;
        _roleManager = roleManager;
        _localizer = factory.Create("ScreenFields", "LinkPara.Identity.API");
    }

    public Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        ///v1/Users/894b23e6-985d-4eb5-89a0-08da9ff646bb
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!Guid.TryParse(url.LastOrDefault(), out var userId))
        {
            throw new InvalidCastException();
        }

        var dbCurrentUser = await _userRepository.GetAll()
            .Include(s => s.Roles)
            .Where(q => q.Id == userId)
            .FirstOrDefaultAsync();

        if (dbCurrentUser is null)
        {
            throw new NotFoundException(nameof(User), userId!);
        }

        var displayScreenFields = new Dictionary<string, object>
        {
            { _localizer.GetString("UserFullName").Value, $"{dbCurrentUser.FirstName} {dbCurrentUser.LastName}"}
        };

        var requestBody = JsonConvert.DeserializeObject<JsonPatchDocument<PatchUserDto>>(request.Body);

        var requestUserDto = _mapper.Map<PatchUserDto>(dbCurrentUser);

        var deletedRoles = requestBody.Operations
            .Where(s => s.path.Contains("roles") && s.value is null)
            .ToList();

        if (deletedRoles.Count > 0)
        {
            deletedRoles.ForEach(s => s.op = Microsoft.AspNetCore.JsonPatch.Operations.OperationType.Remove.ToString());
        }

        requestBody.ApplyTo(requestUserDto);

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(dbCurrentUser, requestUserDto, _localizer);

        if (requestUserDto.Roles.Any())
        {
            var userRoles = await _userManager.GetRolesAsync(dbCurrentUser);

            if (userRoles is null || userRoles.Count == 0)
            {
                throw new NotFoundException(nameof(Role));
            }

            var newRoles = await _roleManager.Roles
                    .Where(s => requestUserDto.Roles.Contains(s.Id))
                    .Select(s => s.Name)
                    .ToListAsync();

            if (updatedFields.Any(x => x.Key == "Roles"))
            {
                updatedFields.Remove("Roles");

                var updatedField = new Dictionary<string, object>
                {
                    {"OldValue", string.Join("-" , userRoles) },
                    {"NewValue", string.Join("-", newRoles)  }
                };

                updatedFields.Add("RoleName", updatedField);
            }
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = displayScreenFields,
            Resource = "Users",
            UpdatedFields = updatedFields
        };
    }

    public async Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<CreateUserCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("UserFullName").Value, $"{requestBody.FirstName} {requestBody.LastName}"},
            { _localizer.GetString("Email").Value,  requestBody.Email},
            { _localizer.GetString("PhoneCode").Value,  requestBody.PhoneCode},
            { _localizer.GetString("PhoneNumber").Value,  requestBody.PhoneNumber},
            { _localizer.GetString("UserType").Value,  _localizer.GetString(requestBody.UserType.ToString()).Value}
        };

        if (requestBody.Roles.Any())
        {
            var roles = await _roleManager.Roles
                    .Where(s => requestBody.Roles.Contains(s.Id))
                    .Select(s => s.Name)
                    .ToListAsync();

            data.Add(_localizer.GetString("RoleName").Value, string.Join("-", roles));
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Users"
        };
    }

    public Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }
}
