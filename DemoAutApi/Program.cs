global using DemoAutApi.Database;
global using DemoAutApi.Endpoints;
global using DemoAutApi.Models;
global using DemoAutApi.Utils;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Identity.Web;
global using Microsoft.OpenApi;

namespace DemoAutApi;

static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();

        var domain = builder.Configuration["AzureAd:Domain"]!;
        var tenantId = builder.Configuration["AzureAd:TenantId"]!;
        var apiClientId = builder.Configuration["AzureAd:ClientId"]!;
        var scopesNames = builder.Configuration["AzureAd:Scopes"]!.Split(',').ToList();
        var scopesUris = scopesNames.Select(scope => $"api://{apiClientId}/{scope}").ToList();

        // Configura la autenticación para validar tokens emitidos por Microsoft Entra External ID.
        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        // Configura autorización para el uso de políticas para controlar el acceso a los endpoints.
        builder.Services.AddAuthorization(options =>
        {
            // Define una política de autorización que exige usuario autenticado y uno de los scopes.
            options.AddPolicy(
                "ApiAccess",
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireScope(scopesNames.ToArray());
                }
            );
        });

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });

            // Configura Swagger para usar OAuth2 Authorization Code Flow con Microsoft Entra External ID.
            options.AddSecurityDefinition(
                "oauth2",
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Description = "Microsoft Entra External ID",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            // Registra los endpoints de autorización y token del tenant externo de Microsoft Entra.
                            AuthorizationUrl = new Uri(
                                $"https://{domain}/{tenantId}/oauth2/v2.0/authorize"
                            ),
                            TokenUrl = new Uri($"https://{domain}/{tenantId}/oauth2/v2.0/token"),
                            // Indica a Swagger cuáles scopes puede solicitar al autenticarse contra Microsoft Entra.
                            Scopes = scopesUris.ToDictionary(scope => scope, scope => scope),
                        },
                    },
                }
            );

            // Exige que las operaciones protegidas en Swagger usen el esquema OAuth2 y los scopes configurados.
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("oauth2", document)] = scopesUris,
            });
        });

        builder.Services.AddDbContext<EmpresaContext>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo API v1");

                // Configura Swagger UI como cliente OAuth2 para solicitar tokens mediante Authorization Code Flow con PKCE.

                options.OAuthClientId(builder.Configuration["AzureAd:SwaggerClientId"]);
                options.OAuthUsePkce();
                options.OAuthScopes(scopesUris.ToArray());

                options.OAuthAdditionalQueryStringParams(
                    // Fuerza que Microsoft Entra muestre nuevamente la pantalla de autenticación y solicite credenciales al usuario,
                    // incluso si ya existe una sesión iniciada en el navegador.
                    //new Dictionary<string, string> { ["prompt"] = "login" }

                    // Fuerza que Microsoft Entra muestre la pantalla de selección de cuenta cuando hay varias sesiones o cuentas disponibles.
                    new Dictionary<string, string> { ["prompt"] = "select_account" }
                );
            });
        }

        // Ejecuta el middleware de autenticación y autorización.
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapReadEndpoints();
        app.MapWriteEndpoints();

        app.Run();
    }
}
