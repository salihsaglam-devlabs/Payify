using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace LinkPara.PF.Infrastructure.Consumers.CronJobs
{
    public class CostProfileConsumer : IConsumer<CostProfileStatus>
    {
        private readonly ILogger<CostProfileConsumer> _logger;
        private readonly IGenericRepository<CostProfile> _repository;
        public CostProfileConsumer(ILogger<CostProfileConsumer> logger, IGenericRepository<CostProfile> repository)
        {
            _logger = logger;
            _repository = repository;
        }
        public async Task Consume(ConsumeContext<CostProfileStatus> context)
        {
            await CostProfileAsync();
        }
        private async Task CostProfileAsync()
        {
            try
            {
                var costProfileList = await _repository.GetAll()
                .Where(b => b.RecordStatus == RecordStatus.Active).ToListAsync();

                var posGroup = costProfileList.GroupBy(x => new {x.VposId, x.PhysicalPosId});

                foreach (var grp in posGroup)
                {
                    var costProfiles = grp.ToList();

                    var activeCostProfile = costProfiles.Where(x => DateTime.Now >= x.ActivationDate)
                        .OrderByDescending(x => x.ActivationDate).FirstOrDefault();

                    var allCostProfile = new List<CostProfile>();

                    if (activeCostProfile is not null)
                    {
                        if (activeCostProfile.ProfileStatus != ProfileStatus.InUse)
                        {
                            activeCostProfile.ProfileStatus = ProfileStatus.InUse;
                            await _repository.UpdateAsync(activeCostProfile);
                        }
                        allCostProfile = costProfiles.Where(x => x.Id != activeCostProfile?.Id).OrderByDescending(x => x.ActivationDate).ToList();
                    }
                    else
                    {
                        allCostProfile = costProfiles.OrderByDescending(x => x.ActivationDate).ToList();
                        var oldActivePricingProfile = allCostProfile.FirstOrDefault(x => x.ProfileStatus == ProfileStatus.InUse);
                        if (oldActivePricingProfile is not null)
                        {
                            allCostProfile.Remove(oldActivePricingProfile);
                        }
                    }
                    foreach (var costProfile in 
                             allCostProfile
                                 .Where(
                                     costProfile => costProfile.ProfileStatus != ProfileStatus.Finished && 
                                                    costProfile.ActivationDate < DateTime.Now))
                    {
                        costProfile.ProfileStatus = ProfileStatus.Finished;
                        costProfile.RecordStatus = RecordStatus.Passive;
                        await _repository.UpdateAsync(costProfile);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"CostProfile Consumer Error {exception}");
            }
        }
    }
}
