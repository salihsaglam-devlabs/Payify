using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.PF.Infrastructure.Services;

public class NotificationTemplateParametersService : INotificationTemplateParametersService
{
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IStringLocalizer _localizer;
    public NotificationTemplateParametersService(
        IGenericRepository<Merchant> merchantRepository,
        IStringLocalizerFactory localizerFactory)
    {
        _merchantRepository = merchantRepository;
        _localizer = localizerFactory.Create("NotificationTemplateParameters", "LinkPara.PF.Infrastructure");
    }
    
    public List<string> GetAllNotificationTemplateParameterNamesAsync(string language)
    {
        var model = new PfCustomNotificationParameters();
        return model.GetParameters(language).Keys.ToList();
    }
    private object GetValueByKey(object entity, string key)
    {
        var parts = key.Split('_');
        object current = entity;
        
        if (key.StartsWith("SubMerchants", StringComparison.OrdinalIgnoreCase))
        {
            var merchant = entity is Merchant m ? m : null;
            
            if (merchant is null || merchant.SubMerchants == null || merchant.SubMerchants.Count == 0)
            {
                return "";
            }
            else if (merchant.SubMerchants != null && merchant.SubMerchants.Count > 0)
            {
                return string.Join("\n", merchant.SubMerchants.Select(s => s.Name + "-" + s.Number));
            }
        }
        
        if (key.StartsWith("MerchantUsers", StringComparison.OrdinalIgnoreCase))
        {
            var merchant = entity is Merchant m ? m : null;
            
            if (merchant is null || merchant.MerchantUsers == null || merchant.MerchantUsers.Count == 0)
            {
                return "";
            }
            else if (merchant.MerchantUsers != null && merchant.MerchantUsers.Count > 0)
            {
                return string.Join("\n", merchant.MerchantUsers.Select(s => s.Name + " " + s.Surname + " (" + s.Email + "-" + s.MobilePhoneNumber + ")"));
            }
        }
        
        if (key.StartsWith("MerchantDocuments", StringComparison.OrdinalIgnoreCase))
        {
            var merchant = entity is Merchant m ? m : null;
            
            if (merchant is null || merchant.MerchantDocuments == null || merchant.MerchantDocuments.Count == 0)
            {
                return "";
            }
            else if (merchant.MerchantDocuments != null && merchant.MerchantDocuments.Count > 0)
            {
                return string.Join("\n", merchant.MerchantDocuments.Select(s => _localizer.GetString("DocumentName") + ": " + s.DocumentName ));
            }
        }
        
        if (key.StartsWith("MerchantBankAccounts", StringComparison.OrdinalIgnoreCase))
        {
            var merchant = entity is Merchant m ? m : null;
            
            if (merchant is null || merchant.MerchantBankAccounts == null || merchant.MerchantBankAccounts.Count == 0)
            {
                return "";
            }
            else if (merchant.MerchantBankAccounts != null && merchant.MerchantBankAccounts.Count > 0)
            {
                return string.Join("\n", merchant.MerchantBankAccounts.Select(s => 
                    _localizer.GetString("Iban") + ": " + s.Iban + ", " + _localizer.GetString("BankName") + ": " + s.Bank?.Name));
            }
        }
        
        if (key.StartsWith("MerchantScores", StringComparison.OrdinalIgnoreCase))
        {
            var merchant = entity is Merchant m ? m : null;
            
            if (merchant is null || merchant.MerchantScores == null || merchant.MerchantScores.Count == 0)
            {
                return "";
            }
            else if (merchant.MerchantScores != null && merchant.MerchantScores.Count > 0)
            {
                return string.Join("\n", merchant.MerchantScores.Select(e => "ScoreCard: " + e.ScoreCardScore + ", Findeks: " + e.FindeksScore));
            }
        }
        
        if (key.StartsWith("MerchantLimits", StringComparison.OrdinalIgnoreCase))
        {
            var merchant = entity is Merchant m ? m : null;
            
            if (merchant is null || merchant.MerchantLimits == null || merchant.MerchantLimits.Count == 0)
            {
                return "";
            }
            else if (merchant.MerchantLimits != null && merchant.MerchantLimits.Count > 0)
            {
                return string.Join("\n", merchant.MerchantLimits.Select(s => 
                    _localizer.GetString("LimitType") + ": " + s.LimitType + ", " + _localizer.GetString("MaxAmount") + ": " + s.MaxAmount 
                    + ", " + _localizer.GetString("MaxPiece") + ": " + s.MaxPiece));
            }
        }
        
        if (key.StartsWith("MerchantBlockageList", StringComparison.OrdinalIgnoreCase))
        {
            var merchant = entity is Merchant m ? m : null;
            
            if (merchant is null || merchant.MerchantBlockageList == null || merchant.MerchantBlockageList.Count == 0)
            {
                return "";
            }
            else if (merchant.MerchantBlockageList != null && merchant.MerchantBlockageList.Count > 0)
            {
                return string.Join("\n", merchant.MerchantBlockageList.Select(s => 
                    _localizer.GetString("BlockageStatus") + ": " + s.MerchantBlockageStatus + ", " + _localizer.GetString("TotalAmount") + ": " + s.TotalAmount 
                    + ", " + _localizer.GetString("BlockageAmount") + ": " + s.BlockageAmount + ", " + _localizer.GetString("RemainingAmount") + ": " + s.RemainingAmount));
            }
        }
        
        if (key.StartsWith("MerchantEmails", StringComparison.OrdinalIgnoreCase))
        {
            var merchant = entity is Merchant m ? m : null;
            
            if (merchant is null)
            {
                return "";
            }
            else if (merchant.MerchantEmails != null && merchant.MerchantEmails.Count > 0)
            {
                return string.Join("\n", merchant.MerchantEmails.Select(e => e.Email));
            }
            else
            {
                return "";
            }
        }
        
        if (key.StartsWith("MerchantBusinessPartner", StringComparison.OrdinalIgnoreCase))
        {
            var merchant = entity is Merchant m ? m : null;
            
            if (merchant is null)
            {
                return "";
            }
            else if (merchant.MerchantBusinessPartner != null && merchant.MerchantBusinessPartner.Count > 0)
            {
                return string.Join("\n", merchant.MerchantBusinessPartner.Select(s => s.FirstName + " " + s.LastName + " (" + s.Email + "-" + s.PhoneNumber + ")"));
            }
            else
            {
                return "";
            }
        }


        foreach (var part in parts)
        {
            if (current == null) return null;

            var type = current.GetType();
            var prop = type.GetProperty(part);
            if (prop == null) return null;

            current = prop.GetValue(current);
        }

        return current;
    }
    public async Task<PfCustomNotificationParameters> GetNotificationParameterTemplateValuesAsync(Guid merchantId, string language)
    {
        var merchant = await _merchantRepository.GetAll()
            .Include(m => m.Customer)
                .ThenInclude(c => c.AuthorizedPerson)
            .Include(m => m.Mcc)
            .Include(m => m.TechnicalContact)
            .Include(m => m.MerchantIntegrator)
            .Include(m => m.MerchantUsers)
            .Include(m => m.MerchantDocuments)
            .Include(m => m.MerchantVposList)
                .ThenInclude(v => v.Vpos)
            .Include(m => m.MerchantEmails)
            .Include(m => m.MerchantScores)
            .Include(m => m.MerchantBankAccounts)
                .ThenInclude(b => b.Bank)
            .Include(m => m.MerchantLimits)
            .Include(m => m.MerchantBlockageList)
            .Include(m => m.MerchantBusinessPartner)
            .Include(m => m.SubMerchants)
            .FirstOrDefaultAsync(m => m.Id == merchantId);
        
        var model = new PfCustomNotificationParameters();
        
        var fields = model.GetPropertiesWithDisplayNames(language);

        var modelType = typeof(PfCustomNotificationParameters);

        foreach (var field in fields)
        {
            try
            {
                var value = GetValueByKey(merchant, field.Key);
                var property = modelType.GetProperty(field.Key);

                if (property != null && property.CanWrite)
                {
                    var convertedValue = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(model, convertedValue);
                }
            }
            catch
            {
                var property = modelType.GetProperty(field.Key);
                if (property != null && property.CanWrite)
                {
                    var defaultValue = property.PropertyType == typeof(string) ? "" : Activator.CreateInstance(property.PropertyType);
                    property.SetValue(model, defaultValue);
                }
            }
        }

        return model;
    }
}