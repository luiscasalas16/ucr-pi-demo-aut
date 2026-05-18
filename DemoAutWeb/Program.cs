global using DemoAutWeb.Components;
global using DemoAutWeb.Models;
global using DemoAutWeb.Utils;
global using Microsoft.AspNetCore.Authentication.OpenIdConnect;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.Identity.Web;
global using Microsoft.Identity.Web.UI;

namespace DemoAutWeb;

static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Registra los servicios necesarios para manejar automáticamente desafíos de autenticación,
        // consentimiento incremental y políticas de acceso condicional solicitadas por Microsoft Entra.
        builder.Services.AddMicrosoftIdentityConsentHandler();

        var apiScopes = builder.Configuration.GetSection("DownstreamApi:Scopes").Get<string[]>()!;

        // Configura autenticación OpenID Connect utilizando Microsoft Entra External ID.
        builder
            .Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(options =>
            {
                // Carga la configuración de autenticación desde la sección AzureAd del appsettings.json.
                builder.Configuration.Bind("AzureAd", options);

                // Permite modificar los parámetros enviados a Microsoft Entra antes de redirigir al usuario al proceso de autenticación.
                options.Events.OnRedirectToIdentityProvider = context =>
                {
                    // Fuerza que Microsoft Entra muestre nuevamente la pantalla de autenticación y solicite credenciales al usuario,
                    // incluso si ya existe una sesión iniciada en el navegador.
                    //context.ProtocolMessage.Prompt = "login";

                    // Fuerza que Microsoft Entra muestre la pantalla de selección de cuenta cuando hay varias sesiones o cuentas disponibles.
                    context.ProtocolMessage.Prompt = "select_account";

                    return Task.CompletedTask;
                };
            })
            // Habilita la adquisición de access tokens para consumir APIs protegidos en nombre del usuario autenticado.
            .EnableTokenAcquisitionToCallDownstreamApi(apiScopes)
            // Configura un cache de tokens en memoria para reutilizar tokens adquiridos previamente durante la sesión de la aplicación.
            .AddInMemoryTokenCaches();

        // Habilita el uso de autorización dentro de la aplicación web.
        builder.Services.AddAuthorization();

        // Registra los controladores necesarios para los endpoints internos de autenticación proporcionados por Microsoft Identity.
        builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

        builder.Services.AddRazorComponents().AddInteractiveServerComponents();

        // Habilita el estado de autenticación en cascada para que los componentes Razor puedan acceder a la información del usuario autenticado.
        builder.Services.AddCascadingAuthenticationState();

        // Registra un HttpClient configurado para consumir el API protegido DemoAutApi.
        builder.Services.AddHttpClient(
            "DemoAutApi",
            client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["DownstreamApi:BaseUrl"]!);
            }
        );

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);

            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        // Ejecuta el middleware de autenticación y autorización.
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapStaticAssets();

        // Registra los controladores utilizados internamente por Microsoft Identity para manejar login, logout y callbacks de autenticación.
        app.MapControllers();

        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        app.Run();
    }
}
