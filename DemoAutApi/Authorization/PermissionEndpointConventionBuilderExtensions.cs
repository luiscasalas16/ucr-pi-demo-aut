namespace DemoAutApi.Authorization;

// Define un método de extensión para aplicar autorización por permisos de forma más expresiva en los endpoints.
// Permite usar:
//     .RequirePermission("Empleado.Read")
// en lugar de:
//     .RequireAuthorization("Permission:Empleado.Read")
internal static class PermissionEndpointConventionBuilderExtensions
{
    // Prefijo utilizado por el proveedor dinámico de políticas para identificar políticas basadas en permisos.
    private const string PolicyPrefix = "Permission:";

    // Aplica al endpoint una política de autorización dinámica asociada al permiso indicado.
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, string permission)
        where TBuilder : IEndpointConventionBuilder
    {
        // Evita registrar políticas inválidas cuando el permiso es nulo, vacío o contiene solo espacios.
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        // Construye el nombre de la política dinámica y la aplica al endpoint.
        return builder.RequireAuthorization($"{PolicyPrefix}{permission}");
    }
}
