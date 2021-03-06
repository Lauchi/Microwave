using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microwave.UI
{
    [ExcludeFromCodeCoverage]
    public static class MicrowaveUiExtensions
    {
        public static void AddMicrowaveUi(this IServiceCollection services)
        {
            services.ConfigureOptions(typeof(MicrowaveUiConfigureOptions));
        }

        public static IApplicationBuilder UseMicrowaveUi(this IApplicationBuilder builder)
        {
            builder.UseRouting();
            builder.UseEndpoints(endpoints => {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
            builder.UseStaticFiles();
            return builder;
        }
    }
}