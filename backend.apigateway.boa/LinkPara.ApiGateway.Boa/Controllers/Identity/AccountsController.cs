using LinkPara.ApiGateway.Boa.Commons.IdentityModels;
using LinkPara.ApiGateway.Boa.Filters.CustomerContext;
using LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Identity.Models.Responses;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.Identity;

public class AccountsController : ApiControllerBase
{
    private readonly IAccountHttpClient _accountHttpClient;
    private readonly IEmoneyAccountHttpClient _emoneyAccountHttpClient;
    private readonly ILogger<AccountsController> _logger;
    private readonly IBus _bus;

    public AccountsController(IAccountHttpClient accountHttpClient,
        IEmoneyAccountHttpClient emoneyAccountHttpClient,
        ILogger<AccountsController> logger,
        IBus bus)
    {
        _accountHttpClient = accountHttpClient;
        _emoneyAccountHttpClient = emoneyAccountHttpClient;
        _logger = logger;
        _bus = bus;
    }

    /// <summary>
    /// Creates a new individual customer.
    /// </summary>
    /// <param name="request">The details of the new customer to be created.</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("create-individual-customer")]
    public async Task<CreateIndividualCustomerResponse> CreateIndividualCustomerAsync(CreateIndividualCustomerRequest request)
    {
        var serviceRequest = new CreateIndividualCustomerRequestWithUsername
        {
            UserName = string.Concat(UserTypePrefix.Individual, request.PhoneCode, request.PhoneNumber).Replace("+", ""),
            PhoneNumber = request.PhoneNumber,
            PhoneCode = request.PhoneCode,
            Email = request.Email,
            ExternalCustomerId = request.ExternalCustomerId,
            ExternalPersonId = request.ExternalPersonId,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };

        var register = await _accountHttpClient.CreateIndividualCustomerAsync(serviceRequest);

        if (register is not null && !string.IsNullOrWhiteSpace(register.UserId.ToString()))
        {
            try
            {
                await _emoneyAccountHttpClient.CreateAccountAsync(new CreateEmoneyAccountRequest
                {
                    AccountType = AccountType.Individual,
                    AccountKycLevel = AccountKycLevel.NoneKyc,
                    Email = request.Email,
                    PhoneCode = request.PhoneCode,
                    PhoneNumber = request.PhoneNumber,
                    IdentityUserId = Guid.Parse(register.UserId.ToString()),
                    Firstname = request.FirstName,
                    Lastname = request.LastName                    
                });
            }
            catch (Exception exception)
            {
                _logger.LogError($"Account Creating Error : {exception} - UserId : {register.UserId}");

                var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Identity.DeleteUser"));
                await endpoint.Send(new DeleteUser
                {
                    UserId = Guid.Parse(register.UserId.ToString())
                }, tokenSource.Token);

                throw;
            }
        }
        return register;
    }

    /// <summary>
    /// STEP-1: Returns an email update token.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("update-email/token")]
    [CustomerContextRequired]
    public async Task<GetEmailUpdateTokenResponse> GetEmailUpdateTokenAsync([FromQuery] GetEmailUpdateTokenRequest request)
    {
        return await _accountHttpClient.GetEmailUpdateTokenAsync(request);
    }

    /// <summary>
    /// STEP-2: After verifying the sent otp code to the new email via EmailOtp, 
    /// finalizes updating email process with the last-given-token and new email parameters.
    /// </summary>
    /// <param name="request"></param>
    [HttpPost("update-email")]
    [CustomerContextRequired]
    public async Task UpdateEmailAsync(UpdateEmailRequest request)
    {
        await _accountHttpClient.UpdateEmailAsync(request);

        var account = await _emoneyAccountHttpClient.GetAccountByUserIdAsync(Guid.Parse(UserId));

        if (account is not null)
        {
            var patchRequest = new JsonPatchDocument<UpdateAccountRequest>();
            patchRequest.Replace(s => s.Email, request.NewEmail);

            await _emoneyAccountHttpClient.PatchAccountAsync(account.Id, patchRequest);
        }
    }

    /// <summary>
    /// STEP-1: Returns an phone update token.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("update-phone/token")]
    [CustomerContextRequired]
    public async Task<ActionResult<GetPhoneNumberTokenResponse>> GetPhoneNumberUpdateTokenAsync([FromQuery] GetPhoneNumberUpdateTokenRequest request)
    {
        return await _accountHttpClient.GetPhoneNumberUpdateTokenAsync(request);
    }

    /// <summary>
    /// STEP-2: After verifying the sent otp code to the new email via EmailOtp, 
    /// finalizes updating email process with the last-given-token and new email parameters.
    /// </summary>
    /// <param name="request"></param>
    [HttpPost("update-phone")]
    [CustomerContextRequired]
    public async Task UpdatePhoneNumberAsync(UpdatePhoneNumberRequest request)
    {
        await _accountHttpClient.UpdatePhoneNumberAsync(request);

        var account = await _emoneyAccountHttpClient.GetAccountByUserIdAsync(Guid.Parse(UserId));

        if (account is not null)
        {
            var patchRequest = new JsonPatchDocument<UpdateAccountRequest>();
            patchRequest.Replace(s => s.PhoneNumber, request.NewPhoneNumber);
            patchRequest.Replace(s => s.PhoneCode, request.NewPhoneCode);

            await _emoneyAccountHttpClient.PatchAccountAsync(account.Id, patchRequest);
        }
    }
}
