using System.Diagnostics;

namespace DemoAutApi.Authorization;

// Implementa el servicio encargado de verificar si un usuario posee un permiso específico.
// En esta versión inicial, la validación todavía no consulta la base de datos y devuelve true
// para permitir probar el flujo completo de autorización por permisos.
internal sealed class PermissionService(EmpresaContext context) : IPermissionService
{
    public async Task<bool> HasPermissionAsync(
        string user,
        string permission,
        CancellationToken cancellationToken = default
    )
    {
        // Verifica durante depuración que el contexto de base de datos haya sido inyectado correctamente.
        Debug.Assert(context != null);

        // Implementación temporal.
        // En una etapa posterior, este método consultará la base de datos para verificar
        // si el usuario indicado posee el permiso requerido, ya sea directamente
        // o a través de los roles que tenga asignados.
        return true;
    }
}
