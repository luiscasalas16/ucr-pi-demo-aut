namespace DemoAutApi.Endpoints;

internal static class WriteEndpoints
{
    public static IEndpointRouteBuilder MapWriteEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/write")
            .WithTags("Write Examples")
            // Aplica explícitamente la política ApiAccess a este grupo de endpoints.
            // No es requerido si ya existe una política de autorización por defecto (FallbackPolicy),
            // pero puede usarse para hacer la intención más explícita en el código.
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
