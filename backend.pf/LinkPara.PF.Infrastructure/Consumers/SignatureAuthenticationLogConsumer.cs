using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;
using System.Net;
using LinkPara.SystemUser;

namespace LinkPara.PF.Infrastructure.Consumers
{
    public class SignatureAuthenticationLogConsumer : IConsumer<AuthenticationErrorLog>
    {
        private readonly PfDbContext _dbContext;
        private readonly IApplicationUserService _applicationUserService;

        public SignatureAuthenticationLogConsumer(PfDbContext dbContext, 
            IApplicationUserService applicationUserService)
        {
            _dbContext = dbContext;
            _applicationUserService = applicationUserService;
        }

        public async Task Consume(ConsumeContext<AuthenticationErrorLog> context)
        {
            var message = context.Message;

            await LogAuthenticationFailAsync(message);
        }

        private async Task LogAuthenticationFailAsync(AuthenticationErrorLog apiValidationLog)
        {
            var log = new MerchantApiValidationLog
            {
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                MerchantId = apiValidationLog.MerchantId,
                ErrorCode = ((int)(HttpStatusCode.Unauthorized)).ToString(),
                ErrorMessage = "Hmac Signature Authentication Failed",
                ClientIpAddress = apiValidationLog.ClientIpAddress
            };

            if (apiValidationLog.TransactionType.ToLower().Contains("provision"))
            {
                log.TransactionType = TransactionType.PreAuth;
                log.ApiName = "PreAuth";
            }
            else if (apiValidationLog.TransactionType.ToLower().Contains("reverse"))
            {
                log.TransactionType = TransactionType.Reverse;
                log.ApiName = "Reverse";
            }
            else if (apiValidationLog.TransactionType.ToLower().Contains("return"))
            {
                log.TransactionType = TransactionType.Return;
            }
            else if (apiValidationLog.TransactionType.ToLower().Contains("getthreedsession"))
            {
                log.ApiName = "getthreedsession";
            }
            else if (apiValidationLog.TransactionType.ToLower().Contains("getthreedsessionresult"))
            {
                log.ApiName = "getthreedsessionresult";
            }
            else if (apiValidationLog.TransactionType.ToLower().Contains("inquire"))
            {
                log.ApiName = "inquire";
            }
            else if (apiValidationLog.TransactionType.ToLower().Contains("bin-information"))
            {
                log.ApiName = "bin-information";
            }

            await _dbContext.MerchantApiValidationLog.AddAsync(log);
            await _dbContext.SaveChangesAsync();
        }
    }
}