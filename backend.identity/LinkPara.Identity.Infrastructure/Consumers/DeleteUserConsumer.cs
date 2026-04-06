using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;
using Microsoft.Extensions.Logging;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Infrastructure.Consumers
{
    public class DeleteUserConsumer : IConsumer<DeleteUser>
    {
        private readonly ILogger<DeleteUserConsumer> _logger;
        private readonly IRepository<User> _userRepository;

        public DeleteUserConsumer(ILogger<DeleteUserConsumer> logger,
            IRepository<User> userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task Consume(ConsumeContext<DeleteUser> context)
        {
            var dbCurrentUser = await _userRepository.GetAll()
            .Include(s => s.Roles)
                .Where(q => q.Id == context.Message.UserId)
                .FirstOrDefaultAsync();

            if (dbCurrentUser is null)
            {
                return;
            }

            if (dbCurrentUser.UserStatus is UserStatus.Inactive)
            {
                return;
            }

            try
            {
                DateTime timestamp = DateTime.Now;
                dbCurrentUser.UserName = $"d_{timestamp.Ticks}_{dbCurrentUser.UserName}";
                dbCurrentUser.NormalizedUserName = $"D_{timestamp.Ticks}_{dbCurrentUser.NormalizedUserName}";
                dbCurrentUser.RecordStatus = RecordStatus.Passive;
                dbCurrentUser.UserStatus = UserStatus.Inactive;

                await _userRepository.UpdateAsync(dbCurrentUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteUserConsumer Error : ", ex);
            }


        }
    }
}
