# DemoAutWeb - Autenticación Web con Microsoft Entra External ID

Este branch muestra cómo integrar autenticación en una aplicación Blazor Web App utilizando Microsoft Entra External ID como proveedor de identidad.

El objetivo de esta versión es demostrar autenticación web usando OpenID Connect y manejo de sesión autenticada mediante cookies. Esta versión todavía no consume el API protegido ni implementa autorización fina mediante roles o permisos.

## Objetivo de esta versión

En esta versión se habilitó que la aplicación web:

1. Use Microsoft Entra External ID para autenticar usuarios.
2. Permita iniciar y cerrar sesión desde la aplicación Blazor.
3. Mantenga la sesión autenticada mediante cookies.
4. Proteja páginas y componentes Razor usando autorización.
5. Muestre información del usuario autenticado.
6. Utilice OpenID Connect para integrarse con Microsoft Entra.

## Infraestructura

La documentación visual de configuración en Microsoft Entra External ID se encuentra en:

```text
README-04-authentication-web-infrastructure.docx
```

## Componentes principales

La solución usa una aplicación registrada en Microsoft Entra External ID para representar la aplicación web Blazor.

La aplicación web redirige al usuario a Microsoft Entra External ID para realizar el proceso de autenticación.

Después del login:

1. Microsoft Entra autentica al usuario.
2. Microsoft Entra devuelve un ID Token a la aplicación.
3. La aplicación crea una sesión autenticada usando cookies.
4. Los componentes Razor pueden acceder a la información del usuario autenticado.

## Configuración general realizada

### 1. Registro de la aplicación web en Microsoft Entra External ID

Se registró una aplicación para representar la aplicación Blazor.

En esa aplicación se configuró:

```text
Redirect URI:
https://localhost:<puerto>/signin-oidc
```

También se habilitó:

```text
ID Tokens
```

en:

```text
Authentication
→ Implicit grant and hybrid flows
```

Esto es necesario para que Microsoft Entra pueda devolver el ID Token utilizado por la aplicación web para autenticar al usuario.

## Configuración del appsettings.json

La sección `AzureAd` contiene la configuración utilizada por la aplicación web para autenticarse con Microsoft Entra External ID:

```json
"AzureAd": {
  "Instance": "https://themepark.ciamlogin.com/",
  "Domain": "themepark.ciamlogin.com",
  "TenantId": "597dd7d9-2093-4b82-a7be-6c9a2805ae2c",
  "ClientId": "<client-id-app-blazor>",
  "ClientSecret": "<client-secret-app-blazor>",
  "CallbackPath": "/signin-oidc"
}
```

El `ClientId` corresponde a la aplicación registrada para Blazor.

El `ClientSecret` corresponde al secreto configurado en Microsoft Entra External ID para la aplicación web.

El `CallbackPath` indica la ruta donde Microsoft Entra devolverá la respuesta de autenticación después del login.

## OpenID Connect

La autenticación web utiliza OpenID Connect.

OpenID Connect es una capa construida sobre OAuth2 orientada específicamente a autenticación e identidad de usuarios.

En esta implementación:

- La aplicación web autentica usuarios.
- Microsoft Entra valida credenciales.
- Microsoft Entra devuelve un ID Token.
- La aplicación crea una sesión autenticada.

En esta etapa todavía no se utilizan access tokens para consumir el API.

## Configuración de autenticación

La autenticación se configura con:

```csharp
builder
    .Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
    });
```

Esto configura:

- OpenID Connect.
- Integración con Microsoft Entra External ID.
- Manejo de login y logout.
- Validación de identidad del usuario.

## Autorización en la aplicación

La autorización se habilita con:

```csharp
builder.Services.AddAuthorization();
```

Esto permite proteger componentes Razor usando:

```razor
@attribute [Authorize]
```

Por ejemplo:

```razor
@page "/secure"
@attribute [Authorize]
```

Con esto, el usuario debe autenticarse antes de acceder al componente.

## Estado de autenticación en Blazor

La aplicación utiliza:

```csharp
builder.Services.AddCascadingAuthenticationState();
```

Esto permite que los componentes Razor accedan al estado de autenticación del usuario.

Por ejemplo:

```razor
<AuthorizeView>
    <Authorized>
        <span>User: @context.User.Identity?.Name</span>
    </Authorized>
</AuthorizeView>
```

## Login y logout

La aplicación usa los endpoints internos proporcionados por Microsoft Identity:

```razor
<a href="MicrosoftIdentity/Account/SignIn">Login</a>

<a href="MicrosoftIdentity/Account/SignOut">Logout</a>
```

Estos endpoints son registrados mediante:

```csharp
builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

app.MapControllers();
```

## Selección de cuenta y reautenticación

La aplicación modifica los parámetros enviados a Microsoft Entra antes de redirigir al usuario al login:

```csharp
options.Events.OnRedirectToIdentityProvider = context =>
{
    context.ProtocolMessage.Prompt = "select_account";
    return Task.CompletedTask;
};
```

Con:

```text
select_account
```

Microsoft Entra muestra la pantalla de selección de cuenta cuando existen múltiples sesiones o cuentas disponibles en el navegador.

También puede utilizarse:

```text
login
```

para forzar que el usuario vuelva a introducir credenciales aunque ya exista una sesión activa.

## Middleware de autenticación y autorización

La aplicación habilita los middlewares:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

`UseAuthentication()` valida la identidad del usuario y establece el contexto autenticado.

`UseAuthorization()` verifica acceso a componentes o recursos protegidos.

## Qué no incluye esta versión

Esta versión todavía no:

- Consume el API protegido.
- Solicita access tokens.
- Implementa scopes.
- Implementa roles.
- Implementa permisos funcionales.

El objetivo actual es únicamente demostrar autenticación web usando Microsoft Entra External ID.

En una versión posterior:

1. La aplicación solicitará access tokens.
2. La aplicación consumirá el API protegido.
3. El API validará scopes.
4. Se implementarán roles y permisos funcionales.

## Flujo general de ejecución

1. El usuario abre la aplicación Blazor.
2. El usuario presiona Login.
3. La aplicación redirige a Microsoft Entra External ID.
4. El usuario se autentica.
5. Microsoft Entra devuelve un ID Token.
6. La aplicación crea una sesión autenticada.
7. Los componentes protegidos pueden ser utilizados.
8. El usuario puede cerrar sesión mediante Logout.

## Referencias

- Microsoft Entra External ID documentation  
  https://learn.microsoft.com/en-us/entra/external-id/

- Microsoft Entra External ID for external tenants  
  https://learn.microsoft.com/en-us/entra/external-id/customers/

- ASP.NET Core Blazor Web App with OpenID Connect  
  https://learn.microsoft.com/en-us/aspnet/core/blazor/security/blazor-web-app-with-oidc?view=aspnetcore-10.0

- Microsoft Identity Web for ASP.NET Core  
  https://learn.microsoft.com/en-us/entra/msal/dotnet/microsoft-identity-web/

- OpenID Connect protocol  
  https://learn.microsoft.com/en-us/entra/identity-platform/v2-protocols-oidc

- Azure Samples - ASP.NET Core Web App with Azure AD B2C  
  https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-5-B2C
