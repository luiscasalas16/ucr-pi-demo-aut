# DemoAutApi - Autenticación con Microsoft Entra External ID

Este branch muestra cómo proteger un API desarrollado con .NET Minimal APIs utilizando Microsoft Entra External ID como proveedor de identidad.

El objetivo de esta versión es demostrar autenticación mediante tokens JWT y autorización básica mediante scopes. Esta versión todavía no implementa roles ni permisos funcionales de la aplicación.

## Objetivo de esta versión

En esta versión se habilitó que el API:

1. Use Microsoft Entra External ID para autenticar usuarios.
2. Reciba y valide tokens JWT emitidos por Microsoft Entra.
3. Proteja endpoints usando una política de autorización.
4. Use Swagger UI como cliente OAuth2 para probar el API.
5. Solicite tokens usando Authorization Code Flow con PKCE.
6. Valide que el token recibido contenga el scope configurado.

## Infraestructura

La documentación visual de creación de infraestructura en Microsoft Entra External ID se encuentra en:

```text
README-Infrastructure.md
```

## Componentes principales

La solución usa dos registros de aplicación en Microsoft Entra External ID:

- Una aplicación que representa el API.
- Una aplicación que representa el cliente Swagger.

La aplicación del API expone el scope:

```text
API.Access
```

Ese scope representa el permiso general para acceder al API.

Swagger solicita ese scope al autenticarse. Luego, cuando Swagger llama los endpoints protegidos, envía el access token en el encabezado HTTP:

```http
Authorization: Bearer <access_token>
```

El API valida ese token antes de permitir el acceso.

## Configuración general realizada

### 1. Registro del API en Microsoft Entra External ID

Se registró una aplicación para representar el API.

En esa aplicación se expuso un scope llamado:

```text
API.Access
```

Ese scope representa el permiso general para acceder al API.

### 2. Registro de Swagger como cliente

Se registró otra aplicación para representar Swagger UI como cliente OAuth2.

A esta aplicación se le concedió permiso para solicitar el scope del API:

```text
api://<api-client-id>/API.Access
```

También se configuró la URL de redirección usada por Swagger:

```text
https://localhost:<puerto>/swagger/oauth2-redirect.html
```

### 3. Configuración del API en appsettings.json

La sección `AzureAd` contiene los valores necesarios para validar tokens emitidos por Microsoft Entra External ID:

```json
"AzureAd": {
  "Instance": "https://themepark.ciamlogin.com/",
  "Domain": "themepark.ciamlogin.com",
  "TenantId": "597dd7d9-2093-4b82-a7be-6c9a2805ae2c",
  "ClientId": "969cc413-10db-4b81-8c46-c1684fb1a796",
  "Scopes": "API.Access",
  "SwaggerClientId": "ee3ea1a6-5800-47eb-a399-9db0d706fa70"
}
```

El `ClientId` corresponde a la aplicación registrada para el API.

El `SwaggerClientId` corresponde a la aplicación registrada para Swagger UI.

El valor `Scopes` contiene el nombre lógico del scope que debe aparecer dentro del token.

## Scopes

Un scope representa un permiso que una aplicación cliente solicita para poder llamar un API.

En esta demo se usa:

```text
API.Access
```

Ese scope no representa todavía permisos específicos como crear, editar o eliminar empleados. Solo representa permiso general para acceder al API.

Hay dos formas importantes de usar el scope en el código:

### Scope completo

Swagger necesita solicitar el scope usando el URI completo:

```text
api://<api-client-id>/API.Access
```

Por eso el código construye los scopes así:

```csharp
var scopesUris = scopesNames.Select(scope => $"api://{apiClientId}/{scope}").ToList();
```

### Nombre lógico del scope

El API valida el scope usando solo el nombre lógico que aparece dentro del claim del token:

```text
API.Access
```

Por eso la política de autorización usa:

```csharp
policy.RequireScope(scopesNames.ToArray());
```

## Autenticación y autorización en el API

La autenticación se configura con:

```csharp
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
```

Esto permite que el API valide tokens JWT emitidos por Microsoft Entra External ID.

La autorización se configura con una política llamada:

```text
ApiAccess
```

Esta política exige dos cosas:

1. Que el usuario esté autenticado.
2. Que el token tenga el scope configurado.

```csharp
options.AddPolicy(
    "ApiAccess",
    policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireScope(scopesNames.ToArray());
    }
);
```

Luego los endpoints se protegen con:

```csharp
.RequireAuthorization("ApiAccess");
```

## Swagger y PKCE

Swagger UI se configuró como cliente OAuth2 para que pueda autenticar al usuario y obtener un access token.

Se usa Authorization Code Flow con PKCE:

```csharp
options.OAuthUsePkce();
```

PKCE agrega una protección adicional al flujo OAuth2 y es el enfoque recomendado para clientes públicos o interactivos como Swagger UI.

También se configuró:

```csharp
new Dictionary<string, string> { ["prompt"] = "select_account" }
```

Esto fuerza la pantalla de selección de cuenta cuando hay varias sesiones o cuentas disponibles en el navegador.

Si se quisiera forzar que el usuario vuelva a introducir credenciales, se podría usar:

```csharp
new Dictionary<string, string> { ["prompt"] = "login" }
```

## Qué no incluye esta versión

Esta versión todavía no implementa autorización fina por roles o permisos.

Actualmente, si un usuario autenticado obtiene un token con el scope `API.Access`, puede acceder a los endpoints protegidos por la política `ApiAccess`.

En una versión posterior se puede agregar autorización más específica.

La idea recomendada es mantener los scopes como permisos generales de acceso al API y manejar los permisos funcionales dentro de la aplicación.

## Flujo general de ejecución

1. El usuario abre Swagger.
2. Presiona `Authorize`.
3. Swagger redirige al usuario a Microsoft Entra External ID.
4. El usuario se autentica.
5. Microsoft Entra devuelve un access token a Swagger.
6. Swagger llama al API usando el token.
7. El API valida el token.
8. El API verifica que el token tenga el scope `API.Access`.
9. Si la validación es correcta, el endpoint se ejecuta.

## Referencias

- Microsoft Entra External ID documentation  
  https://learn.microsoft.com/en-us/entra/external-id/

- Microsoft Entra External ID for external tenants  
  https://learn.microsoft.com/en-us/entra/external-id/customers/

- Protected web API: verify scopes and app roles  
  https://learn.microsoft.com/en-us/entra/identity-platform/scenario-protected-web-api-verification-scope-app-roles

- OAuth 2.0 authorization code flow  
  https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow

- Swagger OAuth2 configuration  
  https://swagger.io/docs/open-source-tools/swagger-ui/usage/oauth2/

- Azure Samples - ASP.NET Core Web App with Azure AD B2C  
  https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-5-B2C

- Azure Samples - Protecting a Web API with Azure AD B2C  
  https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/master/4-WebApp-your-API/4-2-B2C/README.md

- Microsoft Docs - Enable authentication in a web API using Azure AD B2C  
  https://docs.azure.cn/en-us/active-directory-b2c/enable-authentication-web-api?tabs=csharpclient

- Joseph Guadagno - Enabling User Authentication in Swagger using Microsoft Identity  
  https://www.josephguadagno.net/2022/06/03/enabling-user-authentication-in-swagger-using-microsoft-identity

- Joseph Guadagno - Protecting an ASP.NET Core API with Microsoft Identity Platform  
  https://www.josephguadagno.net/2020/06/12/protecting-an-asp-net-core-api-with-microsoft-identity
