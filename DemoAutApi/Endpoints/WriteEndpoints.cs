namespace DemoAutApi.Endpoints;

internal static class WriteEndpoints
{
    public static IEndpointRouteBuilder MapWriteEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/write")
            .WithTags("Write Examples")
            // Protege este grupo de endpoints usando la política ApiAccess.
            .RequireAuthorization("ApiAccess");

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
