# DemoAutApi - Autorización por permisos

Este branch muestra cómo extender el API autenticado para incorporar **autorización funcional basada en permisos**.

Hasta la versión anterior, el API ya validaba:

1. Que la solicitud incluyera un token JWT válido emitido por Microsoft Entra External ID.
2. Que el token contuviera el scope general `API.Access`.

En esta versión se agrega un tercer nivel de control:

3. Que el usuario autenticado posea el **permiso funcional específico** requerido por cada endpoint.

## Objetivo de esta versión

En esta versión se habilitó que el API:

1. Defina permisos funcionales de forma centralizada.
2. Proteja endpoints mediante permisos específicos.
3. Genere políticas de autorización dinámicas a partir del permiso solicitado.
4. Evalúe los permisos mediante un `AuthorizationHandler` personalizado.
5. Delegue la consulta de permisos a un servicio especializado.
6. Permita evolucionar hacia una autorización basada en base de datos.
7. Mantenga la validación del scope general `API.Access`.

## Infraestructura

La autenticación con Microsoft Entra External ID y la protección general del API mediante el scope `API.Access` ya fueron configuradas en versiones anteriores.

Esta etapa se concentra exclusivamente en la **autorización funcional interna del API**.

## Modelo general de autorización

La estrategia utilizada separa tres responsabilidades:

| Nivel | Propósito |
|---|---|
| Autenticación | Confirmar quién es el usuario |
| Scope | Confirmar que el cliente puede consumir el API |
| Permiso | Confirmar que el usuario puede ejecutar una operación específica |

El flujo completo queda así:

```text
Token válido
    + Scope API.Access
        + Permiso funcional requerido
            = Endpoint autorizado
````

Por ejemplo:

```text
GET  /read/GetEmpleados     → Empleado.Read
POST /write/InsertEmpleado  → Empleado.Write
```

## Roles y permisos

Aunque esta versión se implementa inicialmente con **permisos**, el diseño permite utilizar más adelante un modelo completo de:

```text
Usuarios → Roles → Permisos
```

La recomendación arquitectónica es:

* Los **roles** agrupan permisos.
* Los **permisos** son los que protegen directamente los endpoints.

De esta forma, el código del API no depende de nombres de roles como `Administrador` o `Editor`, sino de capacidades funcionales concretas como:

```text
Empleado.Read
Empleado.Write
```

## Permisos definidos

Los permisos se centralizan en una clase estática:

```csharp
internal static class Permissions
{
    internal static class Empleados
    {
        public const string Read = "Empleado.Read";
        public const string Write = "Empleado.Write";
    }
}
```

Esto evita utilizar strings literales dispersos en el código y reduce errores de escritura al proteger endpoints.

## Protección de endpoints por permiso

Para que los endpoints expresen claramente qué permiso requieren, se creó el método de extensión:

```csharp
.RequirePermission(...)
```

Por ejemplo:

```csharp
.RequirePermission(Permissions.Empleados.Read);
```

y:

```csharp
.RequirePermission(Permissions.Empleados.Write);
```

Esto permite escribir:

```csharp
group.MapGet("/GetEmpleados", GetEmpleados)
    .RequirePermission(Permissions.Empleados.Read);
```

en lugar de:

```csharp
.RequireAuthorization("Permission:Empleado.Read");
```

La extensión oculta el detalle interno de la política dinámica y deja el código de los endpoints más expresivo.

## Convención interna de políticas

Internamente, las políticas de permisos utilizan el formato:

```text
Permission:<nombre-del-permiso>
```

Ejemplos:

```text
Permission:Empleado.Read
Permission:Empleado.Write
```

El método `.RequirePermission(...)` construye automáticamente ese nombre de política.

## `PermissionRequirement`

Se creó un requisito de autorización que representa el permiso solicitado:

```csharp
internal sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
```

Este objeto es utilizado por el sistema de autorización para indicar:

```text
Este endpoint requiere el permiso X.
```

## `PermissionAuthorizationPolicyProvider`

Se implementó un proveedor dinámico de políticas:

```csharp
PermissionAuthorizationPolicyProvider
```

Su objetivo es generar políticas de autorización en tiempo de ejecución a partir de nombres como:

```text
Permission:Empleado.Read
```

Cuando detecta una política con el prefijo `Permission:`, construye una política que exige:

1. Usuario autenticado.
2. Scope general válido para el API.
3. Permiso funcional específico.

Conceptualmente, crea políticas como esta:

```csharp
var policy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .RequireScope(_scopesNames)
    .AddRequirements(new PermissionRequirement(permission))
    .Build();
```

Esto evita registrar manualmente una política distinta por cada permiso.

## `PermissionAuthorizationHandler`

Se implementó un handler personalizado:

```csharp
PermissionAuthorizationHandler
```

Este handler evalúa si el usuario autenticado posee el permiso requerido.

El proceso actual es:

1. Obtener el correo del usuario desde los claims del token.
2. Consultar `IPermissionService`.
3. Autorizar si el servicio confirma que el usuario tiene el permiso.

Ejemplo de extracción del correo:

```csharp
var email =
    context.User.FindFirst("preferred_username")?.Value
    ?? context.User.FindFirst(ClaimTypes.Email)?.Value
    ?? context.User.FindFirst("email")?.Value;
```

Si no se logra identificar al usuario, no se concede la autorización.

### Nota sobre el identificador del usuario

En esta versión se utiliza el correo electrónico porque facilita la demostración y la integración inicial con los datos existentes.

Sin embargo, en una solución más robusta, podría ser preferible identificar al usuario mediante un identificador estable de Microsoft Entra, como `oid`, y mantener esa referencia vinculada en la base de datos local.

## `IPermissionService`

Se definió una interfaz para abstraer la verificación de permisos:

```csharp
internal interface IPermissionService
{
    Task<bool> HasPermissionAsync(
        string user,
        string permission,
        CancellationToken cancellationToken = default
    );
}
```

Esto desacopla el mecanismo de autorización de la forma concreta en que se almacenan o consultan los permisos.

## `PermissionService`

Se creó una implementación inicial:

```csharp
PermissionService
```

En esta etapa, el servicio todavía no consulta la base de datos. Devuelve `true` de forma temporal para permitir probar el flujo completo de autorización por permisos.

```csharp
internal sealed class PermissionService(EmpresaContext context) : IPermissionService
{
    public async Task<bool> HasPermissionAsync(
        string user,
        string permission,
        CancellationToken cancellationToken = default
    )
    {
        Debug.Assert(context != null);

        return true;
    }
}
```

En una versión posterior, este método deberá consultar la base de datos para verificar:

```text
Usuario → Roles → Permisos
```

y determinar si el usuario posee el permiso requerido.

## Registro de servicios de autorización

La infraestructura de permisos se registra en el contenedor de dependencias:

```csharp
builder.Services.AddScoped<IPermissionService, PermissionService>();

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddSingleton<
    IAuthorizationPolicyProvider,
    PermissionAuthorizationPolicyProvider
>();
```

Cada registro cumple una función:

* `IPermissionService`: verifica permisos.
* `PermissionAuthorizationHandler`: evalúa requisitos de autorización.
* `PermissionAuthorizationPolicyProvider`: genera policies dinámicas por permiso.

## Políticas `DefaultPolicy` y `FallbackPolicy`

En esta etapa se analizaron dos alternativas posibles para los endpoints que no declaran permisos funcionales explícitos.

### Alternativa 1: permitir acceso general al API

Con esta opción, si un endpoint no declara permiso específico, se aplica la política `ApiAccess`.

```csharp
// Se aplica cuando un endpoint usa RequireAuthorization() sin política explícita.
//options.DefaultPolicy = options.GetPolicy("ApiAccess")!;

// Se aplica cuando un endpoint no declara ningún requisito de autorización.
//options.FallbackPolicy = options.GetPolicy("ApiAccess");
```

Con esta alternativa:

* Un endpoint sin autorización explícita exige autenticación y scope.
* Un endpoint con `.RequireAuthorization()` también exige autenticación y scope.
* No se obliga al desarrollador a definir un permiso funcional específico.

### Alternativa 2: obligar a tomar una decisión explícita

Con esta opción se define una política que siempre falla:

```csharp
options.AddPolicy(
    "PermissionRequired",
    policy =>
    {
        policy.RequireAssertion(_ => false);
    }
);

options.DefaultPolicy = options.GetPolicy("PermissionRequired")!;

options.FallbackPolicy = options.GetPolicy("PermissionRequired")!;
```

Con esta alternativa:

* Un endpoint sin autorización explícita queda bloqueado.
* Un endpoint con `.RequireAuthorization()` sin política también queda bloqueado.
* El desarrollador debe elegir explícitamente una de estas opciones:

  * `.RequirePermission(...)`
  * `.RequireAuthorization("ApiAccess")`
  * `.AllowAnonymous()`

Esta segunda alternativa es más estricta y ayuda a evitar que un endpoint quede expuesto por omisión sin una decisión consciente de autorización.

## Endpoints de prueba para políticas

Se incorporaron endpoints de prueba para mostrar el comportamiento de distintos escenarios:

| Caso                                 | Comportamiento                     |
| ------------------------------------ | ---------------------------------- |
| Sin autorización explícita           | Usa `FallbackPolicy`               |
| `.RequireAuthorization()`            | Usa `DefaultPolicy`                |
| `.RequireAuthorization("ApiAccess")` | Usa la política nombrada           |
| `.AllowAnonymous()`                  | Permite acceso público             |
| `.RequirePermission(...)`            | Exige permiso funcional específico |

Estos ejemplos permiten observar claramente la diferencia entre:

* Autorización general.
* Autorización por defecto.
* Autorización explícita.
* Acceso anónimo.
* Autorización funcional por permisos.

## Comportamiento esperado

Una vez configurada la autorización por permisos, pueden presentarse estos resultados:

| Escenario                                               | Resultado esperado            |
| ------------------------------------------------------- | ----------------------------- |
| Token inválido o ausente                                | `401 Unauthorized`            |
| Token válido, pero sin scope requerido                  | `403 Forbidden`               |
| Token válido y scope válido, pero sin permiso funcional | `403 Forbidden`               |
| Token válido, scope válido y permiso correcto           | Ejecución normal del endpoint |

## Qué incluye esta versión

Esta versión incluye:

* Definición centralizada de permisos.
* Extensión `.RequirePermission(...)`.
* Policies dinámicas basadas en permisos.
* Requisitos personalizados de autorización.
* Handler de autorización por permisos.
* Servicio abstraído para verificar permisos.
* Registro de toda la infraestructura en DI.
* Protección real de endpoints por permiso.
* Estrategias configurables para `DefaultPolicy` y `FallbackPolicy`.

## Qué no incluye todavía

Esta versión todavía no implementa:

* Consulta real de permisos en base de datos.
* Asignación de permisos a roles.
* Asignación de roles a usuarios.
* Administración dinámica de permisos.
* Cache de permisos.
* Autorización por roles en endpoints.

El objetivo de esta etapa es dejar construida la **infraestructura de autorización por permisos**, lista para conectar con el modelo de datos real en una siguiente versión.

## Flujo final de autorización

1. El usuario llama un endpoint protegido.
2. El API valida el JWT.
3. El API valida el scope `API.Access`.
4. El endpoint requiere un permiso específico.
5. La policy dinámica construye el requisito correspondiente.
6. El `PermissionAuthorizationHandler` obtiene el usuario autenticado.
7. El handler consulta `IPermissionService`.
8. Si el usuario posee el permiso, la solicitud continúa.
9. Si no posee el permiso, se responde `403 Forbidden`.

## Referencias

* ASP.NET Core authorization overview
  [https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction)

* Policy-based authorization in ASP.NET Core
  [https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)

* Custom authorization policy providers
  [https://learn.microsoft.com/en-us/aspnet/core/security/authorization/iauthorizationpolicyprovider](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/iauthorizationpolicyprovider)

* Role-based authorization in ASP.NET Core
  [https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles)

* Microsoft Identity Web
  [https://learn.microsoft.com/en-us/entra/msal/dotnet/microsoft-identity-web/](https://learn.microsoft.com/en-us/entra/msal/dotnet/microsoft-identity-web/)