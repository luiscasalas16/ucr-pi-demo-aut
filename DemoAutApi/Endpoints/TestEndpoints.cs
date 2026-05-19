namespace DemoAutApi.Endpoints;

internal static class TestEndpoints
{
    public static IEndpointRouteBuilder MapTestEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/test").WithTags("Test Examples");

        // No declara autorización explícita.
        // Si existe una FallbackPolicy, esta será aplicada automáticamente.
        group.MapGet("/FallbackPolicy", FallbackPolicyExample);

        // Declara que requiere autorización, pero no especifica una política.
        // En este caso se aplica la DefaultPolicy.
        group.MapGet("/DefaultPolicy", DefaultPolicyExample).RequireAuthorization();

        // Declara explícitamente la política ApiAccess.
        // No utiliza ni FallbackPolicy ni DefaultPolicy.
        group
            .MapGet("/ExplicitApiAccessPolicy", ExplicitApiAccessPolicyExample)
            .RequireAuthorization("ApiAccess");

        // Permite acceso anónimo incluso si existe una política global por defecto.
        group.MapGet("/Anonymous", AnonymousExample).AllowAnonymous();

        return builder;
    }

    static IResult FallbackPolicyExample()
    {
        return Results.Ok(
            new
            {
                Message = "Endpoint protegido mediante FallbackPolicy.",
                Authorization = "No declara autorización explícita; se aplica FallbackPolicy.",
            }
        );
    }

    static IResult DefaultPolicyExample()
    {
        return Results.Ok(
            new
            {
                Message = "Endpoint protegido mediante DefaultPolicy.",
                Authorization = "Usa RequireAuthorization() sin indicar política.",
            }
        );
    }

    static IResult ExplicitApiAccessPolicyExample()
    {
        return Results.Ok(
            new
            {
                Message = "Endpoint protegido mediante la política explícita ApiAccess.",
                Authorization = "Usa RequireAuthorization(\"ApiAccess\").",
            }
        );
    }

    static IResult AnonymousExample()
    {
        return Results.Ok(
            new { Message = "Endpoint público.", Authorization = "Usa AllowAnonymous()." }
        );
    }
}
