using System.Text;
using System.Diagnostics;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Application.Services;
using BusBooking.Api.Infrastructure.Bootstrap;
using BusBooking.Api.Infrastructure.Email;
using BusBooking.Api.Infrastructure.Persistence;
using BusBooking.Api.Infrastructure.Repositories;
using BusBooking.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
var secret = builder.Configuration["JWT_SECRET"] ?? jwtOptions.Secret;
var issuer = builder.Configuration["JWT_ISSUER"] ?? jwtOptions.Issuer;
var audience = builder.Configuration["JWT_AUDIENCE"] ?? jwtOptions.Audience;

builder.Services.Configure<JwtOptions>(options =>
{
    options.Secret = secret;
    options.Issuer = issuer;
    options.Audience = audience;
    options.ExpiryMinutes = jwtOptions.ExpiryMinutes;
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? builder.Configuration["CONNECTIONSTRINGS__DEFAULTCONNECTION"];
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBusRepository, BusRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IOperatorService, OperatorService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddHostedService<AdminSeederHostedService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
startupLogger.LogInformation("Starting BusBooking.Api. Environment={EnvironmentName}", app.Environment.EnvironmentName);

app.UseRouting();
app.UseCors("AllowFrontend");

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Http");
    var sw = Stopwatch.StartNew();

    using var scope = logger.BeginScope(new Dictionary<string, object?>
    {
        ["TraceId"] = context.TraceIdentifier
    });

    try
    {
        await next();
        sw.Stop();
        logger.LogInformation("HTTP {Method} {Path} => {StatusCode} in {ElapsedMs}ms", context.Request.Method, context.Request.Path.Value, context.Response.StatusCode, sw.ElapsedMilliseconds);
    }
    catch (InvalidOperationException ex)
    {
        sw.Stop();
        logger.LogWarning(ex, "HTTP {Method} {Path} invalid operation after {ElapsedMs}ms: {Message}", context.Request.Method, context.Request.Path.Value, sw.ElapsedMilliseconds, ex.Message);
        
        context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        sw.Stop();
        logger.LogError(ex, "HTTP {Method} {Path} failed after {ElapsedMs}ms", context.Request.Method, context.Request.Path.Value, sw.ElapsedMilliseconds);
        
        context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { message = ex.Message, stack = ex.StackTrace });
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Avoid redirecting local HTTP preflight requests (e.g. Angular on localhost:4200 -> API on localhost:5000).
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
