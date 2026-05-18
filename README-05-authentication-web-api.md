# DemoAutWeb + DemoAutApi - Integración Web + API con Microsoft Entra External ID

Este branch muestra cómo integrar una aplicación Blazor Web App autenticada con un API protegido utilizando Microsoft Entra External ID.

El objetivo de esta versión es demostrar cómo una aplicación web autenticada puede solicitar access tokens en nombre del usuario y consumir un API protegido usando OAuth2 y OpenID Connect.

Esta versión conecta los proyectos desarrollados previamente:

* `04-authentication-web`
* `02-authentication-api`

## Objetivo de esta versión

En esta versión se habilitó que la aplicación web:

1. Obtenga access tokens para consumir el API.
2. Consuma endpoints protegidos del API.
3. Envíe Bearer Tokens en solicitudes HTTP.
4. Utilice Microsoft Identity Web para la adquisición de tokens.
5. Maneje automáticamente recuperación de tokens y consentimiento incremental.
6. Utilice Authorization Code Flow para obtener tokens del API.

El API continúa:

1. Validando tokens JWT.
2. Validando scopes.
3. Protegiendo endpoints mediante políticas de autorización.

## Infraestructura

La documentación visual de configuración en Microsoft Entra External ID se encuentra en:

```text
README-05-authentication-web-api-infrastructure.docx
```

## Arquitectura general

En esta versión existen tres componentes principales:

### 1. Aplicación Web Blazor

Representa la aplicación frontend utilizada por el usuario.

Responsabilidades:

* Autenticar usuarios.
* Mantener una sesión autenticada.
* Solicitar access tokens.
* Consumir el API protegido.

### 2. API protegido

Representa el backend protegido mediante JWT Bearer Authentication.

Responsabilidades:

* Validar access tokens.
* Validar scopes.
* Proteger endpoints.

### 3. Microsoft Entra External ID

Proveedor de identidad encargado de:

* Autenticar usuarios.
* Emitir ID Tokens.
* Emitir Access Tokens.
* Validar consentimiento y scopes.

## Flujo general de autenticación y autorización

El flujo completo funciona así:

1. El usuario abre la aplicación Blazor.
2. La aplicación redirige al usuario a Microsoft Entra.
3. El usuario se autentica.
4. Microsoft Entra devuelve un ID Token.
5. La aplicación web crea una sesión autenticada.
6. La aplicación web solicita un Access Token para el API.
7. Microsoft Entra devuelve el Access Token.
8. La aplicación web llama el API usando Bearer Authentication.
9. El API valida el token y el scope.
10. El endpoint protegido se ejecuta.

## Cambio importante: SPA → WEB

Durante la versión anterior, la aplicación Blazor estaba configurada en Microsoft Entra como:

```text
SPA (Single Page Application)
```

Eso funcionaba correctamente mientras la aplicación solamente realizaba login de usuarios.

Sin embargo, en esta versión la aplicación necesita:

1. Solicitar access tokens para consumir un API.
2. Mantener tokens en el servidor.
3. Utilizar Microsoft Identity Web para adquisición de tokens.
4. Utilizar un Client Secret.

Por esa razón, la aplicación tuvo que cambiarse de:

```text
SPA
```

A:

```text
Web
```

En Microsoft Entra External ID.

## ¿Por qué fue necesario el cambio?

Una aplicación SPA se considera un:

```text
Public Client
```

porque el código corre en el navegador y no puede almacenar secretos de forma segura.

En cambio, una aplicación Blazor Web App con Interactive Server ejecuta código del lado servidor.

Eso significa que:

* Puede almacenar secretos de forma segura.
* Puede actuar como cliente confidencial.
* Puede solicitar access tokens usando Client Secret.

Por esa razón, Microsoft Identity Web espera que este tipo de aplicación esté registrada como:

```text
Web Application
```

Y no como SPA.

## Aclaración sobre los tokens utilizados

En este flujo intervienen dos tipos de token con propósitos distintos:

### ID Token

El ID Token se utiliza para autenticar al usuario dentro de la aplicación web.

Sirve para que la Web conozca la identidad del usuario y pueda crear una sesión autenticada.

### Access Token

El Access Token se utiliza para consumir el API protegido.

Este token:

* Está dirigido al API.
* Incluye el scope autorizado.
* Se envía en el encabezado `Authorization` como Bearer Token.

La aplicación web no reutiliza el ID Token para llamar el API. Debe solicitar un Access Token específico para ese recurso.

## Configuración realizada en Microsoft Entra

### 1. Plataforma Web

En la aplicación registrada para Blazor se configuró:

```text
Authentication
→ Add platform
→ Web
```

Y se registró:

```text
https://localhost:<puerto>/signin-oidc
```

### 2. Client Secret

Se creó un secret en:

```text
Certificates & secrets
```

Ese valor se configuró en:

```json
"ClientSecret"
```

Del `appsettings.json`.

### 3. Permisos del API

La aplicación Web recibió permisos delegados sobre el API:

```text
api://<api-client-id>/API.Access
```

## Configuración del `appsettings.json`

La aplicación Web mantiene su configuración de autenticación con Microsoft Entra External ID y ahora incluye la sección `DownstreamApi`.

Ejemplo:

```json
"AzureAd": {
  "Instance": "https://themepark.ciamlogin.com/",
  "Domain": "themepark.ciamlogin.com",
  "TenantId": "597dd7d9-2093-4b82-a7be-6c9a2805ae2c",
  "ClientId": "<client-id-app-blazor>",
  "ClientSecret": "<client-secret-app-blazor>",
  "CallbackPath": "/signin-oidc"
},
"DownstreamApi": {
  "BaseUrl": "http://localhost:5047/",
  "Scopes": [
    "api://969cc413-10db-4b81-8c46-c1684fb1a796/API.Access"
  ]
}
```

La sección `DownstreamApi` contiene:

* La URL base del API.
* Los scopes que la aplicación Web solicitará para consumirlo.

## Habilitación de adquisición de tokens

La aplicación Web ahora utiliza:

```csharp
.EnableTokenAcquisitionToCallDownstreamApi(apiScopes)
.AddInMemoryTokenCaches();
```

Esto habilita:

1. Solicitud de access tokens.
2. Manejo automático de tokens.
3. Cache de tokens en memoria.
4. Integración con Microsoft Identity Web.

## Registro de `HttpClient` para consumir el API

Se registró un `HttpClient` específico:

```csharp
builder.Services.AddHttpClient(
    "DemoAutApi",
    client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["DownstreamApi:BaseUrl"]!);
    }
);
```

Esto centraliza la configuración del cliente HTTP utilizado para consumir el API.

## Servicios inyectados en el componente Razor

Para obtener el usuario autenticado, adquirir tokens y consumir el API, se inyectaron los siguientes servicios:

```razor
@inject AuthenticationStateProvider AuthenticationStateProvider
@inject ITokenAcquisition TokenAcquisition
@inject IHttpClientFactory HttpClientFactory
@inject IConfiguration Configuration
@inject MicrosoftIdentityConsentAndConditionalAccessHandler ConsentHandler
```

Cada servicio cumple un propósito específico:

* `AuthenticationStateProvider`: obtiene el estado de autenticación actual del usuario.
* `ITokenAcquisition`: solicita access tokens en nombre del usuario autenticado.
* `IHttpClientFactory`: crea el cliente HTTP configurado para el API.
* `IConfiguration`: accede a los scopes y a la URL del API desde configuración.
* `MicrosoftIdentityConsentAndConditionalAccessHandler`: maneja desafíos de autenticación, recuperación de tokens y consentimiento incremental.

## Obtención del usuario autenticado

Dentro del componente Razor se obtiene el usuario autenticado:

```csharp
var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

var user = authenticationState.User;

if (user.Identity?.IsAuthenticated != true)
    return;
```

Esto permite asegurar que solo se intente obtener un token cuando exista un usuario autenticado.

## Obtención de los scopes del API

Los scopes se leen desde configuración:

```csharp
var scopes = Configuration
    .GetSection("DownstreamApi:Scopes")
    .Get<string[]>()!;
```

En esta demo, el scope configurado es:

```text
api://969cc413-10db-4b81-8c46-c1684fb1a796/API.Access
```

## Solicitud de Access Token

La aplicación solicita el access token usando:

```csharp
var accessToken = await TokenAcquisition.GetAccessTokenForUserAsync(
    scopes,
    user: user
);
```

Microsoft Identity Web:

1. Obtiene el token desde cache si existe.
2. Solicita un nuevo token a Microsoft Entra si es necesario.
3. Devuelve el access token para consumir el API.

## Envío del Bearer Token

El token se envía usando:

```csharp
httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", accessToken);
```

Luego el API recibe:

```http
Authorization: Bearer <access_token>
```

## Consumo del API protegido

Finalmente la aplicación consume el endpoint protegido:

```csharp
_empleados = (await httpClient.GetFromJsonAsync<List<Empleado>>("read/GetEmpleados"))?.ToArray();
```

El API valida:

1. Firma del token.
2. Audience.
3. Scope `API.Access`.

## Manejo de pérdida de tokens en memoria

La aplicación utiliza:

```csharp
.AddInMemoryTokenCaches();
```

Esto significa que:

* Los tokens se almacenan en memoria.
* Los tokens se pierden cuando la aplicación se reinicia.

En ese caso:

* La cookie de autenticación todavía existe.
* El usuario parece autenticado.
* Pero el access token ya no está disponible.

Esto puede producir:

```text
MicrosoftIdentityWebChallengeUserException
```

## Manejo automático de recuperación de tokens

Para manejar ese escenario se agregó:

```csharp
builder.Services.AddMicrosoftIdentityConsentHandler();
```

Y en el componente Razor:

```razor
@inject MicrosoftIdentityConsentAndConditionalAccessHandler ConsentHandler
```

Luego, cuando ocurre una excepción de desafío:

```csharp
ConsentHandler.HandleException(ex);
```

Esto permite:

* Recuperar tokens perdidos.
* Ejecutar nuevamente el flujo de autenticación cuando sea necesario.
* Manejar consentimiento incremental.
* Manejar Conditional Access.

## Ejemplo de manejo de excepción al solicitar el token

```csharp
try
{
    var accessToken = await TokenAcquisition.GetAccessTokenForUserAsync(
        scopes,
        user: user
    );

    var httpClient = HttpClientFactory.CreateClient("DemoAutApi");

    httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", accessToken);

    _empleados = (await httpClient.GetFromJsonAsync<List<Empleado>>("read/GetEmpleados"))?.ToArray();
}
catch (MicrosoftIdentityWebChallengeUserException ex)
{
    ConsentHandler.HandleException(ex);
}
```

## Qué no incluye esta versión

Esta versión todavía no implementa:

* Roles.
* Permisos funcionales.
* Claims personalizados.
* Autorización fina dentro del API.

Actualmente el acceso se controla únicamente mediante:

```text
API.Access
```

Ese scope habilita acceso general al API, pero todavía no distingue entre operaciones de lectura, escritura o permisos de negocio.

## Flujo final de ejecución

1. El usuario inicia sesión en la aplicación Web.
2. La Web obtiene un ID Token.
3. La Web mantiene una sesión autenticada.
4. La Web solicita un Access Token para el API.
5. Microsoft Entra devuelve el Access Token.
6. La Web consume el API usando Bearer Authentication.
7. El API valida el token y el scope.
8. El endpoint protegido se ejecuta.

## Referencias

* Microsoft Identity Web
  [https://learn.microsoft.com/en-us/entra/msal/dotnet/microsoft-identity-web/](https://learn.microsoft.com/en-us/entra/msal/dotnet/microsoft-identity-web/)

* ASP.NET Core Blazor Web App with OpenID Connect
  [https://learn.microsoft.com/en-us/aspnet/core/blazor/security/blazor-web-app-with-oidc?view=aspnetcore-10.0](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/blazor-web-app-with-oidc?view=aspnetcore-10.0)

* Web app calling downstream APIs
  [https://learn.microsoft.com/en-us/entra/identity-platform/scenario-web-app-call-api-overview](https://learn.microsoft.com/en-us/entra/identity-platform/scenario-web-app-call-api-overview)

* OAuth 2.0 authorization code flow
  [https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow](https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow)

* Microsoft Entra External ID
  [https://learn.microsoft.com/en-us/entra/external-id/](https://learn.microsoft.com/en-us/entra/external-id/)

* Microsoft Identity Web token acquisition
  [https://learn.microsoft.com/en-us/dotnet/api/microsoft.identity.web.itokenacquisition](https://learn.microsoft.com/en-us/dotnet/api/microsoft.identity.web.itokenacquisition)
