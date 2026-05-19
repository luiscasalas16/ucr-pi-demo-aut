namespace DemoAutApi.Authorization;

// Representa un requisito de autorización basado en un permiso funcional específico.
// Este requisito será evaluado por PermissionAuthorizationHandler para determinar
// si el usuario autenticado posee el permiso requerido.
internal sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    // Nombre del permiso que debe poseer el usuario para satisfacer este requisito.
    public string Permission { get; } = permission;
}
