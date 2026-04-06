using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs
{
    public class PricingProfileConsumer : IConsumer<PricingProfileStatus>
    {
        private readonly ILogger<PricingProfileConsumer> _logger;
        private readonly IGenericRepository<PricingProfile> _repository;
        public PricingProfileConsumer(ILogger<PricingProfileConsumer> logger, IGenericRepository<PricingProfile> repository)
        {
            _logger = logger;
            _repository = repository;
        }
        public async Task Consume(ConsumeContext<PricingProfileStatus> context)
        {
            await PricingProfileAsync();
        }
        private async Task PricingProfileAsync()
        {
            try
            {
                var pricingProfileList = await _repository.GetAll()
                .Where(b => b.RecordStatus == RecordStatus.Active).ToListAsync();

                var pricingProfileListGroupBy = pricingProfileList.GroupBy(x => x.PricingProfileNumber);

                foreach (var grp in pricingProfileListGroupBy)
                {
                    var pricingProfileListByProfileNumber = pricingProfileList
                    .Where(b => b.PricingProfileNumber == grp.Key).ToList();

                    var activePricingProfile = pricingProfileListByProfileNumber.Where(x => DateTime.Now >= x.ActivationDate)
                        .OrderByDescending(x => x.ActivationDate).FirstOrDefault();

                    var newPricingProfileList = new List<PricingProfile>();

                    if (activePricingProfile != null)
                    {
                        if (activePricingProfile.ProfileStatus != ProfileStatus.InUse)
                        {
                            activePricingProfile.ProfileStatus = ProfileStatus.InUse;
                            await _repository.UpdateAsync(activePricingProfile);
                        }
                        newPricingProfileList = pricingProfileListByProfileNumber.Where(x => x.Id != activePricingProfile?.Id).OrderByDescending(x => x.ActivationDate).ToList();
                    }
                    else
                    {
                        //yeni bir profile yoksa;kullanımda olan profile statusunu degistirme
                        newPricingProfileList = pricingProfileListByProfileNumber.OrderByDescending(x => x.ActivationDate).ToList();
                        var oldActivePricingProfile = newPricingProfileList.FirstOrDefault(x => x.ProfileStatus == ProfileStatus.InUse);
                        if (oldActivePricingProfile != null)
                        {
                            newPricingProfileList.Remove(oldActivePricingProfile);
                        }
                    }
                    foreach (var pricingProfile in newPricingProfileList)
                    {
                        if (pricingProfile.ProfileStatus != ProfileStatus.Finished && pricingProfile.ActivationDate < DateTime.Now)
                        {
                            pricingProfile.ProfileStatus = ProfileStatus.Finished;
                            await _repository.UpdateAsync(pricingProfile);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"PricingProfile Consumer Error {exception}");
            }
        }
    }
}
