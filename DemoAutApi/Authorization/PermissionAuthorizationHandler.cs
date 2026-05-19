using System.Security.Claims;

namespace DemoAutApi.Authorization;

// Evalúa los requisitos de autorización basados en permisos.
// Para cada solicitud protegida por una política de permiso, verifica si el usuario autenticado posee el permiso requerido.
internal sealed class PermissionAuthorizationHandler(IPermissionService permissionService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        // Obtiene el correo electrónico del usuario autenticado desde los claims disponibles en el token.
        // Se prueban varios nombres de claim para soportar distintas representaciones posibles del correo.
        var email =
            context.User.FindFirst("preferred_username")?.Value
            ?? context.User.FindFirst(ClaimTypes.Email)?.Value
            ?? context.User.FindFirst("email")?.Value;

        // Si no se logra identificar al usuario mediante su correo, no se concede la autorización.
        if (string.IsNullOrWhiteSpace(email))
            return;

        // Consulta el servicio de permisos para verificar si el usuario posee el permiso requerido por la política.
        var hasPermission = await permissionService.HasPermissionAsync(
            email,
            requirement.Permission
        );

        // Si el usuario posee el permiso, se marca el requisito como satisfecho.
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
