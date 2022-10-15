using Blazored.LocalStorage;
using MatBlazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
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

            builder.Services.AddLocalization();

            await builder.Build().RunAsync().ConfigureAwait(false);
        }
    }
}