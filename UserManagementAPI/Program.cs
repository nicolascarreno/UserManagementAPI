using System.Collections.Concurrent;
using UserManagementAPI.Utilidades;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            Error = "Ocurrió un error inesperado."
        });
    });
});

var usuarios = new ConcurrentDictionary<string, Utils.Usuario>
{
    ["ana@example.com"] = new Utils.Usuario { Nombre = "Ana", Apellido = "García" },
    ["luis@example.com"] = new Utils.Usuario { Nombre = "Luis", Apellido = "Pérez" }
};

app.MapGet("/usuarios", () =>
{
    return usuarios
        .Select(u => new Utils.UsuarioResponse
        {
            Mail = u.Key,
            Nombre = u.Value.Nombre,
            Apellido = u.Value.Apellido
        });
});

app.MapGet("/usuarios/{mail}", (string mail) =>
{
    if (string.IsNullOrWhiteSpace(mail))
    {
        return Results.BadRequest(new { Error = "El mail es obligatorio" });
    }
    
    if (!usuarios.TryGetValue(mail, out var usuario))
    {
        return Results.NotFound(new { Error = $"No existe un usuario con mail {mail}." });
    }

    return Results.Ok(new Utils.UsuarioResponse
    {
        Mail = mail,
        Nombre = usuario.Nombre,
        Apellido = usuario.Apellido
    });
});

app.MapPost("/usuarios", (Utils.UsuarioInput nuevoUsuario) =>
{
    if (!Utils.ValidarUsuario(nuevoUsuario, out var error))
    {
        return Results.BadRequest(new { Error = $"{error}" });
    }

    var usuario = new Utils.Usuario
    {
        Nombre = nuevoUsuario.Nombre,
        Apellido = nuevoUsuario.Apellido
    };

    if (!usuarios.TryAdd(nuevoUsuario.Mail, usuario))
    {
        return Results.BadRequest(new { Error = $"Ya existe un usuario con el mail {nuevoUsuario.Mail}." });
    }

    var usuarioCreado = new Utils.UsuarioResponse
    {
        Mail = nuevoUsuario.Mail,
        Nombre = nuevoUsuario.Nombre,
        Apellido = nuevoUsuario.Apellido
    };

    return Results.Created($"/usuarios/{Uri.EscapeDataString(nuevoUsuario.Mail)}", usuarioCreado);
});

app.MapPut("/usuarios/{mail}", (string mail, Utils.UsuarioInput usuarioActualizado) =>
{
    if (string.IsNullOrWhiteSpace(mail))
    {
        return Results.BadRequest(new { Error = "El mail es obligatorio" });
    }

    if (!usuarios.TryGetValue(mail, out var usuarioExistente))
    {
        return Results.NotFound(new { Error = $"No existe un usuario con mail {mail}." });
    }

    if (!Utils.ValidarUsuario(usuarioActualizado, out var error))
    {
        return Results.BadRequest(new { Error = $"{error}" });
    }

    var usuarioActualizadoObj = new Utils.Usuario
    {
        Nombre = usuarioActualizado.Nombre,
        Apellido = usuarioActualizado.Apellido
    };

    if (!usuarios.TryUpdate(mail, usuarioActualizadoObj, usuarioExistente))
    {
        return Results.Conflict(new { Error = $"El usuario con mail {mail} fue modificado por otra operación." });
    }

    return Results.Ok(new Utils.UsuarioResponse
    {
        Mail = mail,
        Nombre = usuarioActualizado.Nombre,
        Apellido = usuarioActualizado.Apellido
    });
});

app.MapDelete("/usuarios/{mail}", (string mail) =>
{
    if (string.IsNullOrWhiteSpace(mail))
    {
        return Results.BadRequest(new { Error = "El mail es obligatorio" });
    }

    if (!usuarios.TryRemove(mail, out _))
    {
        return Results.NotFound(new { Error = $"No existe un usuario con mail {mail}." });
    }

    return Results.NoContent();
});

app.Run();