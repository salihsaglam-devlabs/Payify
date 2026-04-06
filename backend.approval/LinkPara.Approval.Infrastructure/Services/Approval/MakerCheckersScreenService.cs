using AutoMapper;
using LinkPara.Approval.Application.Features.MakerCheckers.Commands.DeleteMakerChecker;
using LinkPara.Approval.Application.Features.MakerCheckers.Commands.SaveMakerChecker;
using LinkPara.Approval.Application.Features.MakerCheckers.Commands.UpdateMakerChecker;
using LinkPara.Approval.Domain.Entities;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LinkPara.Approval.Infrastructure.Services.Approval;
public class MakerCheckersScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<MakerChecker> _makerCheckerRepository;
    private readonly IGenericRepository<Case> _caseRepository;
    private readonly IStringLocalizer _localizer;
    private readonly IRoleService _roleService;

    public MakerCheckersScreenService(IGenericRepository<MakerChecker> makerCheckerRepository,
        IStringLocalizerFactory factory,
        IGenericRepository<Case> caseRepository,
        IRoleService roleService)
    {
        _makerCheckerRepository = makerCheckerRepository;
        _localizer = factory.Create("ScreenFields", "LinkPara.Approval.API");
        _caseRepository = caseRepository;
        _roleService = roleService;
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!Guid.TryParse(url.LastOrDefault(), out var makerCheckerId))
        {
            throw new InvalidCastException();
        }

        var makerChecker = await _makerCheckerRepository.GetAll().Where(s => s.Id == makerCheckerId).FirstOrDefaultAsync();

        if (makerChecker is null)
        {
            throw new NotFoundException(nameof(MakerChecker), makerCheckerId);
        }

        var approvalCase = await _caseRepository.GetAll().Where(s => s.Id == makerChecker.CaseId
                                                        && s.RecordStatus == RecordStatus.Active).FirstOrDefaultAsync();

        var makerRole = await _roleService.GetRoleAsync(makerChecker.MakerRoleId);
        var checkerRole = await _roleService.GetRoleAsync(makerChecker.CheckerRoleId);


        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("CaseId").Value, makerChecker.CaseId},
            { _localizer.GetString("CaseName").Value, approvalCase.DisplayName},
            { _localizer.GetString("MakerRoleId").Value, makerChecker.MakerRoleId },
            { _localizer.GetString("MakerRoleName").Value, makerRole.Name },
            { _localizer.GetString("CheckerRoleId").Value, makerChecker.CheckerRoleId },
            { _localizer.GetString("CheckerRoleName").Value, checkerRole.Name },
            { _localizer.GetString("ModuleName").Value, approvalCase?.ModuleName },
            { _localizer.GetString("Resource").Value, approvalCase?.Resource },
            { _localizer.GetString("ActionName").Value, approvalCase?.ActionName }
        };

        if (makerChecker.SecondCheckerRoleId != Guid.Empty)
        {
            var secondCheckerRole = await _roleService.GetRoleAsync(makerChecker.SecondCheckerRoleId);
            data.Add(_localizer.GetString("SecondCheckerRoleId").Value, makerChecker.SecondCheckerRoleId);
            data.Add(_localizer.GetString("SecondCheckerRoleName").Value, secondCheckerRole.Name);
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "MakerCheckers"
        };
    }

    public Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<SaveMakerCheckerCommand>(request.Body);

        var approvalCase = await _caseRepository.GetAll().Where(s => s.Id == requestBody.CaseId
                                                                && s.RecordStatus == RecordStatus.Active).FirstOrDefaultAsync();
        if (approvalCase is null)
        {
            throw new NotFoundException(nameof(Case), requestBody.CaseId);
        }

        var makerRole = await _roleService.GetRoleAsync(requestBody.MakerRoleId);
        var checkerRole = await _roleService.GetRoleAsync(requestBody.CheckerRoleId);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("CaseId").Value, requestBody.CaseId},
            { _localizer.GetString("CaseName").Value, approvalCase.DisplayName},
            { _localizer.GetString("MakerRoleId").Value, requestBody.MakerRoleId },
            { _localizer.GetString("MakerRoleName").Value, makerRole.Name },
            { _localizer.GetString("CheckerRoleId").Value, requestBody.CheckerRoleId },
            { _localizer.GetString("CheckerRoleName").Value, checkerRole.Name },
        };

        if (requestBody.SecondCheckerRoleId != Guid.Empty)
        {
            var secondCheckerRole = await _roleService.GetRoleAsync(requestBody.SecondCheckerRoleId);
            data.Add(_localizer.GetString("SecondCheckerRoleId").Value, requestBody.SecondCheckerRoleId);
            data.Add(_localizer.GetString("SecondCheckerRoleName").Value, secondCheckerRole.Name);
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "MakerCheckers"
        };
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateMakerCheckerCommand>(request.Body);

        var approvalCase = await _caseRepository.GetAll().Where(s => s.Id == requestBody.CaseId
                                                                && s.RecordStatus == RecordStatus.Active).FirstOrDefaultAsync();

        if (approvalCase is null)
        {
            throw new NotFoundException(nameof(Case), requestBody.CaseId);
        }

        var makerChecker = await _makerCheckerRepository.GetAll().Where(s => s.Id == requestBody.Id
                                                            && s.RecordStatus == RecordStatus.Active).FirstOrDefaultAsync();


        if (makerChecker is null)
        {
            throw new NotFoundException(nameof(MakerChecker), requestBody.Id);
        }

        var makerRole = await _roleService.GetRoleAsync(makerChecker.MakerRoleId);
        var checkerRole = await _roleService.GetRoleAsync(makerChecker.CheckerRoleId);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("CaseId").Value, makerChecker.CaseId},
            { _localizer.GetString("CaseName").Value, approvalCase.DisplayName},
            { _localizer.GetString("MakerRoleId").Value, makerChecker.MakerRoleId },
            { _localizer.GetString("MakerRoleName").Value, makerRole.Name },
            { _localizer.GetString("CheckerRoleId").Value, makerChecker.CheckerRoleId },
            { _localizer.GetString("CheckerRoleName").Value, checkerRole.Name },
        };


        RoleDetailDto secondCheckerRole = null;
        if (makerChecker.SecondCheckerRoleId != Guid.Empty)
        {
            secondCheckerRole = await _roleService.GetRoleAsync(makerChecker.SecondCheckerRoleId);
            data.Add(_localizer.GetString("SecondCheckerRoleId").Value, makerChecker.SecondCheckerRoleId);
            data.Add(_localizer.GetString("SecondCheckerRoleName").Value, secondCheckerRole.Name);
        }

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(makerChecker, requestBody, _localizer);

        if (updatedFields.Any(x => x.Key == _localizer.GetString("MakerRoleId").Value))
        {
            var changedMakerRole = await _roleService.GetRoleAsync(requestBody.MakerRoleId);

            Dictionary<string, object> value = new Dictionary<string, object>
                {
                    {
                        "OldValue",
                        _localizer.GetString(makerRole.Name).Value
                     },
                    {
                        "NewValue",
                        _localizer.GetString(changedMakerRole.Name).Value
                    }
                };
            updatedFields.Add(_localizer.GetString("MakerRoleName").Value, value);
        }

        if (updatedFields.Any(x => x.Key == _localizer.GetString("CheckerRoleId").Value))
        {
            var changedCheckerRole = await _roleService.GetRoleAsync(requestBody.CheckerRoleId);

            Dictionary<string, object> value = new Dictionary<string, object>
                {
                    {
                        "OldValue",
                        _localizer.GetString(checkerRole.Name).Value
                     },
                    {
                        "NewValue",
                        _localizer.GetString(changedCheckerRole.Name).Value
                    }
                };
            updatedFields.Add(_localizer.GetString("CheckerRoleName").Value, value);
        }

        if (updatedFields.Any(x => x.Key == _localizer.GetString("SecondCheckerRoleId").Value))
        {
            RoleDetailDto changedSecondCheckerRole = null;
            if (requestBody.SecondCheckerRoleId != Guid.Empty)
            {
                changedSecondCheckerRole = await _roleService.GetRoleAsync(requestBody.CheckerRoleId);
            }

            Dictionary<string, object> value = new Dictionary<string, object>
                {
                    {
                        "OldValue",
                        _localizer.GetString(secondCheckerRole != null ? secondCheckerRole.Name : string.Empty).Value
                    },
                    {
                        "NewValue",
                        _localizer.GetString(changedSecondCheckerRole != null ? changedSecondCheckerRole.Name : string.Empty).Value
                    }
                };
            updatedFields.Add(_localizer.GetString("SecondCheckerRoleName").Value, value);

        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }
}

