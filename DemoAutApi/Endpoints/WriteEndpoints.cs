namespace DemoAutApi.Endpoints;

internal static class WriteEndpoints
{
    public static IEndpointRouteBuilder MapWriteEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/write")
            .WithTags("Write Examples")
            // Exige que el usuario autenticado posea el permiso funcional requerido
            // para ejecutar operaciones de escritura sobre empleados.
            .RequirePermission(Permissions.Empleados.Write);

        group.MapPost("/InsertEmpleado", InsertEmpleado);

        return builder;
    }

    static async Task InsertEmpleado([FromServices] EmpresaContext context)
    {
        var empleadoFake = Faker.GenerateEmpleadoFake();

        context.Empleados.Add(empleadoFake);

        await context.SaveChangesAsync();
    }
}
