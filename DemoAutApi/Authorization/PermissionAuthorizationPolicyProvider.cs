using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DemoAutApi.Authorization;

// Proveedor dinámico de políticas de autorización basadas en permisos.
// Permite declarar políticas con el formato "Permission:<nombre-del-permiso>"
// sin tener que registrar manualmente una política por cada permiso existente.
internal sealed class PermissionAuthorizationPolicyProvider(
    IOptions<AuthorizationOptions> options,
    IConfiguration configuration
) : IAuthorizationPolicyProvider
{
    // Prefijo utilizado para identificar las políticas dinámicas de permisos.
    private const string PolicyPrefix = "Permission:";

    // Proveedor por defecto de ASP.NET Core.
    // Se utiliza para delegar la resolución de políticas que no correspondan a permisos,
    // como políticas nombradas tradicionales, DefaultPolicy o FallbackPolicy.
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider = new(options);

    // Obtiene desde configuración los scopes generales requeridos para consumir el API.
    // Estos scopes se seguirán validando aun cuando la autorización específica se haga por permisos.
    private readonly string[] _scopesNames = configuration["AzureAd:Scopes"]!.Split(
        ',',
        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
    );

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Si la política solicitada no corresponde a una política de permisos,
        // se delega su resolución al proveedor por defecto de ASP.NET Core.
        if (!policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        // Extrae el nombre del permiso a partir del nombre completo de la política.
        // Por ejemplo:
        // "Permission:Empleado.Read" -> "Empleado.Read"
        var permission = policyName[PolicyPrefix.Length..];

        // Construye dinámicamente una política que exige:
        // 1. Usuario autenticado.
        // 2. Scope válido para acceder al API.
        // 3. Permiso funcional específico evaluado por PermissionAuthorizationHandler.
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireScope(_scopesNames)
            .AddRequirements(new PermissionRequirement(permission))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }

    // Devuelve la política por defecto configurada en ASP.NET Core.
    // Se utiliza cuando un endpoint invoca RequireAuthorization() sin indicar una política explícita.
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    // Devuelve la política fallback configurada en ASP.NET Core.
    // Se utiliza cuando un endpoint no declara ningún requisito de autorización.
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }
}
