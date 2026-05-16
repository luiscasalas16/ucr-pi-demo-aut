namespace DemoAutApi.Endpoints;

internal static class ReadEndpoints
{
    public static IEndpointRouteBuilder MapReadEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/read")
            .WithTags("Read Examples")
            // Protege este grupo de endpoints usando la política ApiAccess.
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
