using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace LinkPara.SharedModels.Localization
{
    public static class LocalizationConfiguration
    {
        public static void ConfigureLocalization(this IServiceCollection services)
        {
            services.AddLocalization(x => x.ResourcesPath = "Resources");
        }

        public static void ConfigureLocalization(this IApplicationBuilder app)
        {
            var supportedCultures = new[] { "tr", "en" };

            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            app.UseRequestLocalization(localizationOptions);
        }
    }
}
