namespace DemoAutApi.Authorization;

// Define el contrato del servicio encargado de verificar si un usuario posee un permiso específico.
internal interface IPermissionService
{
    // Determina si el usuario indicado cuenta con el permiso requerido para ejecutar una operación.
    Task<bool> HasPermissionAsync(
        string user,
        string permission,
        CancellationToken cancellationToken = default
    );
}
