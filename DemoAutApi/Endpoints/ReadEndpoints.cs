namespace DemoAutApi.Endpoints;

internal static class ReadEndpoints
{
    public static IEndpointRouteBuilder MapReadEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/read")
            .WithTags("Read Examples")
            // Aplica explícitamente la política ApiAccess a este grupo de endpoints.
            // No es requerido si ya existe una política de autorización por defecto (FallbackPolicy),
            // pero puede usarse para hacer la intención más explícita en el código.
            .RequireAuthorization("ApiAccess");

        group.MapGet("/GetEmpleados", GetEmpleados);

        return builder;
    }

    public record EmpleadoDto(
        string Cedula,
        string Nombre,
        string Apellidos,
        string Correo,
        int Salario
    );

    static Task<List<EmpleadoDto>> GetEmpleados([FromServices] EmpresaContext context)
    {
        return context
            .Empleados.OrderBy(e => e.Nombre)
            .Select(e => new EmpleadoDto(e.Cedula, e.Nombre, e.Apellidos, e.Correo, e.Salario))
            .ToListAsync();
    }
}
