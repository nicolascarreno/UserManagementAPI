using System.Collections.Concurrent;
using UserManagementAPI.Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

builder.Services.AddAuthentication();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            Error = "Un enexpected error occurred."
        });
    });
});

app.Use(async(context, next) =>
{
    //Simulate authentication with a query parameteter
    var isAuthenticated = context.Request.Query["authenticated"] == "true";
    if (!isAuthenticated)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Access Denied");    
        }
        return;    
    }
    context.Response.Cookies.Append("SecureCookie", "SecureData", new CookieOptions
    {
        HttpOnly = true,
        Secure = true
    });
    await next();    
});
app.UseHttpLogging();


var users = new ConcurrentDictionary<int, Utils.User>
{
    [1] = new Utils.User { Name = "Ana", LastName = "García", Mail = "ana@example.com" },
    [2] = new Utils.User { Name = "Luis", LastName = "Pérez", Mail = "luis@example.com"}
};

app.MapGet("/users", () =>
{
    return users
        .Select(u => new Utils.UserResponse
        {
            Mail = u.Value.Mail,
            Name = u.Value.Name,
            LastName = u.Value.LastName,
            Id = u.Key
        });
});

app.MapGet("/users/{id}", (int id) =>
{
    if (id <= 0)
    {
        return Results.BadRequest(new { Error = "The ID must be a positive integer" });
    }
    
    if (!users.TryGetValue(id, out var user))
    {
        return Results.NotFound(new { Error = $"User with id {id} doesn't exist." });
    }

    return Results.Ok(new Utils.UserResponse
    {
        Mail = user.Mail,
        Name = user.Name,
        LastName = user.LastName,
        Id = id
    });
});

app.MapPost("/users", (Utils.UserInput newUser) =>
{
    if (!Utils.ValidateUser(newUser, out var error))
    {
        return Results.BadRequest(new { Error = $"{error}" });
    }

    var user = new Utils.User
    {
        Name = newUser.Name,
        LastName = newUser.LastName,
        Mail = newUser.Mail
    };

    if (!users.TryAdd(newUser.Id, user))
    {
        return Results.BadRequest(new { Error = $"The user with ID {newUser.Id} already exists." });
    }

    var createdUser = new Utils.UserResponse
    {
        Mail = newUser.Mail,
        Name = newUser.Name,
        LastName = newUser.LastName,
        Id = newUser.Id
    };

    return Results.Created($"/usuarios/{newUser.Id}", createdUser);
});

app.MapPut("/users/{id}", (int id, Utils.UserInput updatedUser) =>
{
    if (id <= 0)
    {
        return Results.BadRequest(new { Error = "ID must be a positive integer" });
    }

    if (!users.TryGetValue(id, out var existingUser))
    {
        return Results.NotFound(new { Error = $"User with ID {id} doesn't exist." });
    }

    if (!Utils.ValidateUser(updatedUser, out var error))
    {
        return Results.BadRequest(new { Error = $"{error}" });
    }

    var updatedUserObj = new Utils.User
    {
        Name = updatedUser.Name,
        LastName = updatedUser.LastName,
        Mail = updatedUser.Mail
    };

    if (!users.TryUpdate(id, updatedUserObj, existingUser))
    {
        return Results.Conflict(new { Error = $"The user with ID {id} was modified by someone else." });
    }

    return Results.Ok(new Utils.UserResponse
    {
        Mail = updatedUser.Mail,
        Name = updatedUser.Name,
        LastName = updatedUser.LastName,
        Id = id
    });
});

app.MapDelete("/users/{id}", (int id) =>
{
    if (id <= 0)
    {
        return Results.BadRequest(new { Error = "ID must be a positive integer" });
    }

    if (!users.TryRemove(id, out _))
    {
        return Results.NotFound(new { Error = $"User with ID {id} doesn't exist." });
    }

    return Results.NoContent();
});

app.Run();