namespace UserManagementAPI.Utilidades;
public static class Utils {
    public static bool ValidarUsuario(UsuarioInput usuario, out string? error)
    {
        if (string.IsNullOrWhiteSpace(usuario.Nombre))
        {
            error = "El nombre es obligatorio.";
            return false;
        }

        if (!usuario.Nombre.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
        {
            error = "El nombre solo puede contener letras y espacios.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(usuario.Apellido))
        {
            error = "El apellido es obligatorio.";
            return false;
        }

        if (!usuario.Apellido.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
        {
            error = "El apellido solo puede contener letras y espacios.";
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
    
    public static bool ValidarMail(string mail, out string? error)
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
}

