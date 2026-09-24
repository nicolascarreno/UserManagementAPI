# UserManagementAPI

API REST para gestión de usuarios desarrollada con ASP.NET Core y .NET 10. Permite registrar, consultar, actualizar y eliminar usuarios, además de autenticar clientes mediante JWT. La solución está pensada para ser simple, extensible y fácil de probar con Swagger.

---

## 1. Título y descripción breve

### UserManagementAPI

Es una API de gestión de usuarios que ofrece operaciones CRUD y flujo de autenticación basado en JWT. El proyecto está orientado a un caso de uso realista de una aplicación backend donde los usuarios pueden autenticarse y acceder a recursos protegidos.

La API cubre estas necesidades principales:

- Registrar usuarios con validación de datos.
- Iniciar sesión con correo y contraseña.
- Generar un token JWT seguro.
- Consultar, actualizar y eliminar usuarios autenticados.
- Persistir la información en SQLite con Entity Framework Core.

---

## 2. Tech stack

La aplicación utiliza las siguientes tecnologías y librerías:

- .NET 10
- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQLite
- JWT (JSON Web Tokens)
- Swagger / OpenAPI
- BCrypt.Net-Next para hashing de contraseñas
- Serilog para logging
- Inyección de dependencias nativa de ASP.NET Core

### Dependencias principales

- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.EntityFrameworkCore.Design
- Swashbuckle.AspNetCore
- BCrypt.Net-Next
- Serilog.AspNetCore
- Serilog.Sinks.Console

---

## 3. Arquitectura y patrones de diseño utilizados

La API sigue una arquitectura sencilla pero organizada, con separación clara entre la capa de entrada, la lógica de autenticación y el acceso a datos.

### 3.1 Inyección de dependencias

El proyecto utiliza la inyección de dependencias integrada de ASP.NET Core para registrar servicios y repositorios en el contenedor de la aplicación.

Ejemplo de registro:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<TokenService>();
```

Esto permite:

- mayor testabilidad,
- desacoplamiento entre controladores y acceso a datos,
- reutilización de servicios y configuración centralizada.

### 3.2 Patrón Repository

La lógica de acceso a datos está encapsulada en la interfaz `IUserRepository` y su implementación `EfUserRepository`.

Esto evita que los controladores conozcan directamente los detalles de Entity Framework y mantiene la lógica de negocio separada de la capa de persistencia.

### 3.3 Controladores con ASP.NET Core MVC

Los endpoints están definidos en controladores:

- `AuthController` para autenticación.
- `UserController` para CRUD de usuarios.

Cada acción devuelve respuestas HTTP con códigos específicos como `200 OK`, `201 Created`, `400 Bad Request`, `401 Unauthorized`, `404 Not Found`, etc.

### 3.4 Validación de entrada

Las validaciones están centralizadas en `Utils.cs` para garantizar reglas consistentes sobre:

- nombre,
- apellido,
- correo,
- longitud de contraseña,
- formato del mail.

### 3.5 Middleware de logging y manejo global de errores

El proyecto incluye:

- `UseHttpLogging()` para registrar requests/responses,
- `UseExceptionHandler()` para controlar errores inesperados globalmente,
- configuración de autenticación y autorización en el pipeline.

---

## 4. Requisitos previos

Para ejecutar este proyecto, necesitas:

- .NET SDK 10.0 o superior
- Un editor como Visual Studio 2025 o VS Code
- Git para clonar el repositorio
- Acceso a una terminal/PowerShell o consola del sistema

### Configuración adicional importante

La app usa JWT y requiere una clave secreta configurada en `Jwt:Key`.

Debes definirla en `appsettings.Development.json` o, mejor aún, con User Secrets:

```bash
dotnet user-secrets set "Jwt:Key" "TuClaveSuperSecretaDeAlMenos32Caracteres" --project UserManagementAPI
```

Esto es necesario porque el programa accede a:

```csharp
builder.Configuration["Jwt:Key"]
```

sin definirlo en `appsettings.json` se produce un problema de configuración al generar los tokens.

---

## 5. Instalación y ejecución

### 5.1 Clonar el repositorio

```bash
git clone https://github.com/tu-usuario/UserManagementAPI.git
cd UserManagementAPI
```

### 5.2 Restaurar dependencias

```bash
dotnet restore
```

### 5.3 Compilar la solución

```bash
dotnet build
```

### 5.4 Ejecutar la API

```bash
dotnet run --project UserManagementAPI
```

### 5.5 Acceder a Swagger

Una vez levantada la aplicación, normalmente podrás abrir:

```text
https://localhost:{puerto}/swagger
```

Esto te permitirá probar los endpoints directamente desde la interfaz UI de Swagger.

### 5.6 Ejecutar en modo de desarrollo

```bash
dotnet watch run --project UserManagementAPI
```

---

## 6. Autenticación

La autenticación del proyecto se implementó con JWT y ASP.NET Core Authentication.

### 6.1 Cómo se agregó

En `Program.cs` se registran los servicios de autenticación y autorización:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

app.UseAuthentication();
app.UseAuthorization();
```

### 6.2 Flujo de autenticación

1. El cliente envía un `POST` a `/api/auth/login` con:
   - `mail`
   - `password`
2. El controlador busca el usuario por correo.
3. Se verifica la contraseña con `BCrypt.Net.BCrypt.Verify(...)`.
4. Si las credenciales son válidas, se genera un JWT con `TokenService`.
5. El cliente recibe el token y lo envía en la cabecera:

```http
Authorization: Bearer <token>
```

6. El middleware valida el token para permitir el acceso a rutas protegidas.

### 6.3 Token generado

El servicio `TokenService` crea un token que incluye claims como:

- `sub`: id del usuario
- `email`: correo del usuario
- `name`: nombre y apellido

Además, el token incluye:

- emisor (`Issuer`)
- audiencia (`Audience`)
- expiración configurada en `Jwt:ExpiresInMinutes`

### 6.4 Endpoint de login

```http
POST /api/auth/login
Content-Type: application/json

{
  "mail": "ana@example.com",
  "password": "Ana12345"
}
```

Respuesta esperada:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

---

## 7. Documentación de endpoints

La documentación interactiva de Swagger se habilita con `UseSwagger()` y `UseSwaggerUI()` en modo desarrollo.

### 7.1 Autenticación

| Método | Endpoint | Requiere autenticación | Descripción |
|---|---|---:|---|
| POST | `/api/auth/login` | No | Inicia sesión y devuelve JWT |

### 7.2 Usuarios

| Método | Endpoint | Requiere autenticación | Descripción |
|---|---|---:|---|
| GET | `/api/user` | Sí | Obtiene todos los usuarios |
| GET | `/api/user/{id}` | Sí | Obtiene un usuario por ID |
| POST | `/api/user` | Sí | Crea un usuario |
| PUT | `/api/user/{id}` | Sí | Actualiza un usuario |
| DELETE | `/api/user/{id}` | Sí | Elimina un usuario |

### 7.3 Ejemplo de creación de usuario

```http
POST /api/user
Authorization: Bearer <token>
Content-Type: application/json

{
  "id": 10,
  "name": "Carlos",
  "lastName": "Méndez",
  "mail": "carlos@example.com",
  "password": "Carlos12345"
}
```

### 7.4 Ejemplo de consulta de usuarios

```http
GET /api/user
Authorization: Bearer <token>
```

### 7.5 Códigos de respuesta

- `200 OK`: operación exitosa de consulta o actualización.
- `201 Created`: creación correcta del recurso.
- `204 No Content`: eliminación exitosa.
- `400 Bad Request`: datos inválidos o errores de validación.
- `401 Unauthorized`: credenciales inválidas o token no válido.
- `404 Not Found`: usuario no encontrado.
- `409 Conflict`: conflicto de actualización (por ejemplo, al modificar otro registro concurrente).

---

## 8. Estructura del proyecto

```text
UserManagementAPI/
├── Controllers/
│   ├── AuthController.cs
│   └── UserController.cs
├── Data/
│   └── UserDbContext.cs
├── Migrations/
│   ├── 20260918042816_InitialCreate.cs
│   ├── 20260918042816_InitialCreate.Designer.cs
│   └── AppDbContextModelSnapshot.cs
├── Repositories/
│   ├── EfUserRepository.cs
│   └── IUserRepository.cs
├── Services/
│   └── TokenService.cs
├── appsettings.json
├── appsettings.Development.json
├── Doc.md
├── Program.cs
├── UserManagementAPI.csproj
├── UserManagementAPI.http
├── Utils.cs
├── Properties/
│   └── launchSettings.json
├── bin/
├── obj/
├── logs/
│   └── UserManagementAPI20260914.txt
└── README.md
```

### Descripción de carpetas y archivos principales

- `Controllers/`: define los endpoints HTTP de la API.
- `Data/`: contiene el `DbContext` para Entity Framework Core.
- `Repositories/`: encapsula el acceso a datos y define la interfaz del repositorio.
- `Services/`: incluye lógica transversal, como la generación de tokens JWT.
- `Migrations/`: historial de cambios de base de datos generado por EF Core.
- `Properties/`: configuración de lanzamiento local.
- `logs/`: archivos de logging del proyecto.
- `Program.cs`: punto de entrada de la aplicación y configuración del pipeline.
- `Utils.cs`: validaciones y modelos compartidos.
- `UserManagementAPI.http`: ejemplos de requests HTTP para testing.

---

## 9. Decisiones técnicas

### 9.1 ¿Por qué SQLite?

Se eligió SQLite por ser una base de datos ligera, fácil de configurar y adecuada para proyectos pequeños o de aprendizaje. Además:

- no requiere un servidor de base de datos externo,
- es ideal para pruebas locales,
- se integra muy bien con EF Core,
- mantiene la API sencilla de ejecutar en cualquier entorno.

### 9.2 ¿Por qué patrón Repository?

El patrón Repository se utilizó para separar la lógica de acceso a datos de los controladores. Esto ofrece varias ventajas:

- reduce la dependencia directa de `DbContext` en los controladores,
- facilita cambios futuros a otra fuente de datos,
- centraliza consultas y lógica de persistencia,
- mejora la mantenibilidad del código.

### 9.3 ¿Por qué JWT?

Se optó por JWT por ser una solución estándar para autenticación en APIs REST:

- permite autenticación stateless,
- evita mantener sesiones en el servidor,
- es compatible con clientes web, móviles y terceros,
- se integra fácilmente con ASP.NET Core mediante middleware de validación.

### 9.4 ¿Por qué BCrypt?

Se usó `BCrypt.Net-Next` para almacenar las contraseñas con hashing. Esto evita guardar contraseñas en texto plano y ayuda a proteger la información sensible del usuario.

---