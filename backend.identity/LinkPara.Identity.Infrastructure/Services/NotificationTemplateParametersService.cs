using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Notification.NotificationModels.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Identity.Infrastructure.Services;

public class NotificationTemplateParametersService : INotificationTemplateParametersService
{
    private readonly IRepository<User> _userRepository;
    private readonly IStringLocalizer _localizer;
    public NotificationTemplateParametersService(
        IRepository<User> userRepository,
        IStringLocalizerFactory localizerFactory)
    {
        _userRepository = userRepository;
        _localizer = localizerFactory.Create("TemplateParameters", "LinkPara.Identity.Infrastructure");
    }
    
    public List<string> GetAllNotificationTemplateParameterNamesAsync(string language)
    {
        var model = new IdentityCustomNotificationParameters();
        return model.GetParameters(language).Keys.ToList();
    }
    
    private object GetValueByKey(object entity, string key)
    {
        var parts = key.Split('_');
        object current = entity;
        
        if (key.StartsWith("Roles", StringComparison.OrdinalIgnoreCase))
        {
            var user = entity is User m ? m : null;
            
            if (user is null)
            {
                return "";
            }
            else if (user.Roles != null && user.Roles.Count > 0)
            {
                return string.Join("\n", user.Roles.Select(s => 
                    _localizer.GetString("RoleName") + ": " + s.Name + " " + _localizer.GetString("RoleScope") + ": " + s.RoleScope));
            }
        }
        
        if (key.StartsWith("LoginActivity", StringComparison.OrdinalIgnoreCase))
        {
            var user = entity is User m ? m : null;
            
            if (user is null)
            {
                return "";
            }
            else if (user.LoginActivity != null && user.LoginActivity.Count > 0)
            {
                return string.Join("\n", user.LoginActivity.Select(s => _localizer.GetString("LoginDate") + ": " + s.Date + " " + _localizer.GetString("LoginResult") + ": " + s.LoginResult));
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
    public async Task<IdentityCustomNotificationParameters> GetNotificationParameterTemplateValuesAsync(Guid userId, string language)
    {
        var user = await _userRepository.GetAll()
            .Include(u => u.Roles)
            .Include(u=> u.LoginActivity)
            .Include(u => u.LoginLastActivity)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var model = new IdentityCustomNotificationParameters();
        
        var fields = model.GetPropertiesWithDisplayNames(language);

        var modelType = typeof(IdentityCustomNotificationParameters);

        foreach (var field in fields)
        {
            try
            {
                var value = GetValueByKey(user, field.Key);
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

        var str = model.RenderTemplate("<p>Eposta içerik deneme 2222 {{Kullanıcı Telefon Numarası}}</p>", "TR");
        
        return model;
    }
}