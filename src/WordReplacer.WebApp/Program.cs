using System.Globalization;
using System.Reflection;
using Blazored.LocalStorage;
using MatBlazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WordReplacer.Models;
using WordReplacer.Services;

namespace WordReplacer.WebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddMatBlazor();
            builder.Services.AddMatToaster(config
                =>
            {
                config.Position = MatToastPosition.TopRight;
                config.PreventDuplicates = true;
                config.NewestOnTop = false;
                config.ShowCloseButton = true;
                config.MaximumOpacity = 100;
                config.VisibleStateDuration = 5000;
            });

            builder.Services.AddBlazoredLocalStorage();
            
            builder.Services.AddTransient<IDocumentService, DocumentService>();

            var appSettings = new AppSettings()
            {
                LanguageStoreKey = "choose_language",
                AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)!
            };

            builder.Services.AddSingleton(appSettings);
            
            builder.Services.AddLocalization();

            var host = builder.Build();

            var localStorage = host.Services.GetRequiredService<ILocalStorageService>();
            var languageStoreKey = appSettings.LanguageStoreKey;

            if (await localStorage.ContainKeyAsync(languageStoreKey).ConfigureAwait(false))
            {
                var selectedLanguage = await localStorage
                    .GetItemAsStringAsync(languageStoreKey)
                    .ConfigureAwait(false);

                var newCulture = new CultureInfo(selectedLanguage);
                Thread.CurrentThread.CurrentCulture = newCulture;
                Thread.CurrentThread.CurrentUICulture = newCulture;
                CultureInfo.DefaultThreadCurrentCulture = newCulture;
                CultureInfo.DefaultThreadCurrentUICulture = newCulture;
            }
            
            await host.RunAsync().ConfigureAwait(false);
        }
    }
}