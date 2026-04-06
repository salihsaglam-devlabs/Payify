using AutoMapper;
using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.Emoney.Application.Features.Accounts;
using LinkPara.Emoney.Application.Features.Accounts.Commands.ValidateIdentity;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.Emoney.Infrastructure.Services.Approval;

public class CustodyAccountScreenService : IApprovalScreenService
{
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _localizer;
    private readonly IGenericRepository<Account> _accountRepository;

    public CustodyAccountScreenService(
        IMapper mapper,
        IStringLocalizerFactory factory,
        IGenericRepository<Account> accountRepository)
    {
        _mapper = mapper;
        _localizer = factory.Create("ScreenFields", "LinkPara.Emoney.API");
        _accountRepository = accountRepository;
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<DeactivateAccountCommand>(request.Body);

        var childAccount = _accountRepository.GetAll()
            .Where(x => x.Id == requestBody.AccountId)
            .FirstOrDefault();

        var parentAccount = _accountRepository.GetAll()
            .Where(x => x.Id == childAccount.ParentAccountId)
            .FirstOrDefault();

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("ChildName").Value, childAccount.Name},
            { _localizer.GetString("ParentName").Value, parentAccount.Name}
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "EmoneyAccounts"
        });
    }
    public Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!Guid.TryParse(url.LastOrDefault(), out var accountId))
        {
            throw new InvalidCastException();
        }

        var account = await _accountRepository.GetAll()
                                               .SingleOrDefaultAsync(x => x.Id == accountId);

        if (account is null)
        {
            throw new NotFoundException(nameof(Account));
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Id"), account.Id.ToString() },
            { _localizer.GetString("KycChangeDate"), account.KycChangeDate.ToString() },
            { _localizer.GetString("AccountStatus"), account.AccountStatus.ToString() },
            { _localizer.GetString("RecordStatus"), account.RecordStatus.ToString() },
            { _localizer.GetString("SuspendedDate"), account.SuspendedDate.ToString() },
        };

        var requestBody = JsonConvert.DeserializeObject<JsonPatchDocument<PatchAccountDto>>(request.Body);

        var requestAccountUserDto = _mapper.Map<PatchAccountDto>(account);

        requestBody.ApplyTo(requestAccountUserDto);

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(account, requestAccountUserDto, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "EmoneyAccounts",
            UpdatedFields = updatedFields
        };
    }
}
