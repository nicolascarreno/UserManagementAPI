namespace UserManagementAPI.Utilities;
public static class Utils {
    public static bool ValidateUser(UserInput user, out string? error)
    {
        if (string.IsNullOrWhiteSpace(user.Name))
        {
            error = "Name is mandatory.";
            return false;
        }

        if (!user.Name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
        {
            error = "Name can only contains whitespaces and letters.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(user.LastName))
        {
            error = "Last name is mandatory.";
            return false;
        }

        if (!user.LastName.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
        {
            error = "Last name can only contain whitespaces and letters.";
            return false;
        }

        if (!ValidateMail(user.Mail, out var errorMail))
        {
            error = errorMail;
            return false;
        }

        error = null;
        return true;
    }
    
    public static bool ValidateMail(string mail, out string? error)
    {
        if (string.IsNullOrWhiteSpace(mail))
        {
            error = "Mail is mandatory";
            return false;
        }
        if (mail.Count(c => c == '@') != 1)
        {
            error = "Mail must contain exactly one '@'";
            return false;
        }

        var parts = mail.Split('@');
        if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            error = "There must characters before and after the '@'";
            return false;
        }

        if (parts[1].Count(c => c == '.') != 1 || parts[0].Contains('.'))
        {
            error = "Mail must contain exaclty one '.' after the '@' and musn't contain '.' before the '@'";
            return false;
        }

        if (mail.Contains(' '))
        {
            error = "Mail can't contain whitespaces";
            return false;
        }

        error = null;
        return true;
    }

    public class User
    {
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public int Id { get; set; }
    }

    public class UserInput
    {
        public string Mail { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Id { get; set; }
    }

    public class UserResponse
    {
        public string Mail { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Id { get; set; }
    }
}

