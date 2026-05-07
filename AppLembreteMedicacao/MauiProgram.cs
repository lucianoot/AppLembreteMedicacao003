using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using System.Globalization;

namespace AppLembreteMedicacao
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Configuração de Cultura
            var culturaBr = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentCulture = culturaBr;
            CultureInfo.DefaultThreadCurrentUICulture = culturaBr;

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification(config =>
                {
                    config.AddCategory(new NotificationCategory(NotificationCategoryType.Status)
                    {
                        ActionList = new HashSet<NotificationAction>
                        {
                            new NotificationAction(100) { Title = "Dose Tomada" },
                            new NotificationAction(101) { Title = "Pular Dose" }
                        }
                    });
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}