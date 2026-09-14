using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var usuarios = new ConcurrentDictionary<string, Usuario>
{
    ["ana@example.com"] = new Usuario { Nombre = "Ana", Apellido = "García" },
    ["luis@example.com"] = new Usuario { Nombre = "Luis", Apellido = "Pérez" }
};

app.MapGet("/usuarios", () =>
{
    return usuarios
        .OrderBy(u => u.Key)
        .Select(u => new UsuarioResponse
        {
            Mail = u.Key,
            Nombre = u.Value.Nombre,
            Apellido = u.Value.Apellido
        })
        .ToList();
});

app.MapGet("/usuarios/{mail}", (string mail) =>
{
    if (string.IsNullOrWhiteSpace(mail))
    {
        return Results.BadRequest(new { mensaje = "El mail es obligatorio" });
    }
    
    if (!usuarios.TryGetValue(mail, out var usuario))
    {
        return Results.NotFound(new { mensaje = $"No existe un usuario con mail {mail}." });
    }

    return Results.Ok(new UsuarioResponse
    {
        Mail = mail,
        Nombre = usuario.Nombre,
        Apellido = usuario.Apellido
    });
});

app.MapPost("/usuarios", (UsuarioInput nuevoUsuario) =>
{
    if (!ValidarUsuario(nuevoUsuario, out var error))
    {
        return Results.BadRequest(new { mensaje = error });
    }

    var usuario = new Usuario
    {
        Nombre = nuevoUsuario.Nombre,
        Apellido = nuevoUsuario.Apellido
    };

    if (!usuarios.TryAdd(nuevoUsuario.Mail, usuario))
    {
        return Results.BadRequest(new { mensaje = $"Ya existe un usuario con el mail {nuevoUsuario.Mail}." });
    }

    var usuarioCreado = new UsuarioResponse
    {
        Mail = nuevoUsuario.Mail,
        Nombre = nuevoUsuario.Nombre,
        Apellido = nuevoUsuario.Apellido
    };

    return Results.Created($"/usuarios/{Uri.EscapeDataString(nuevoUsuario.Mail)}", usuarioCreado);
});

app.MapPut("/usuarios/{mail}", (string mail, UsuarioInput usuarioActualizado) =>
{
    if (string.IsNullOrWhiteSpace(mail))
    {
        return Results.BadRequest(new { mensaje = "El mail es obligatorio" });
    }

    if (!usuarios.TryGetValue(mail, out var usuarioExistente))
    {
        return Results.NotFound(new { mensaje = $"No existe un usuario con mail {mail}." });
    }

    if (!ValidarUsuario(usuarioActualizado, out var error))
    {
        return Results.BadRequest(new { mensaje = error });
    }

    var usuarioActualizadoObj = new Usuario
    {
        Nombre = usuarioActualizado.Nombre,
        Apellido = usuarioActualizado.Apellido
    };

    if (!usuarios.TryUpdate(mail, usuarioActualizadoObj, usuarioExistente))
    {
        return Results.Conflict(new { mensaje = $"El usuario con mail {mail} fue modificado por otra operación." });
    }

    return Results.Ok(new UsuarioResponse
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
        return Results.BadRequest(new { mensaje = "El mail es obligatorio" });
    }

    if (!usuarios.TryRemove(mail, out _))
    {
        return Results.NotFound(new { mensaje = $"No existe un usuario con mail {mail}." });
    }

    return Results.NoContent();
});

app.Run();

static bool ValidarUsuario(UsuarioInput usuario, out string? error)
{
    if (string.IsNullOrWhiteSpace(usuario.Nombre))
    {
        error = "El nombre es obligatorio.";
        return false;
    }

    if (string.IsNullOrWhiteSpace(usuario.Apellido))
    {
        error = "El apellido es obligatorio.";
        return false;
    }

    if (!ValidarMail(usuario.Mail, out var errorMail))
    {
        error = errorMail;
        return false;
    }

    error = null;
    return true;
}

static bool ValidarMail(string mail, out string? error)
{
    if (string.IsNullOrWhiteSpace(mail))
    {
        error = "El mail es obligatorio";
        return false;
    }
    if (mail.Count(c => c == '@') != 1)
    {
        error = "El mail debe contener exactamente un '@'";
        return false;
    }

    var partes = mail.Split('@');
    if (string.IsNullOrWhiteSpace(partes[0]) || string.IsNullOrWhiteSpace(partes[1]))
    {
        error = "Deben haber caracteres antes y despues del '@'";
        return false;
    }

    if (partes[1].Count(c => c == '.') != 1 || partes[0].Contains('.'))
    {
        error = "El mail debe contener un '.' despues del '@' y no debe contener '.' antes del '@'";
        return false;
    }

    if (mail.Contains(' '))
    {
        error = "El mail no debe contener espacios";
        return false;
    }

    error = null;
    return true;
}

public class Usuario
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
}

public class UsuarioInput
{
    public string Mail { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
}

public class UsuarioResponse
{
    public string Mail { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
}

