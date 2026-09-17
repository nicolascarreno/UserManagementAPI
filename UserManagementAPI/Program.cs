using System.Collections.Concurrent;
using UserManagementAPI.Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserRepository, InMemoryUserRepository>();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

builder.Services.AddAuthentication();
builder.Services.AddControllers();

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

app.MapControllers();
app.Run();