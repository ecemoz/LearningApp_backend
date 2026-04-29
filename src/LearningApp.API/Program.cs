using System.Text;
using LearningApp.Infrastructure.Authentication;
using LearningApp.Infrastructure.Persistence;
using LearningApp.Infrastructure.Persistence.Seed;
using LearningApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var environmentFile = FindEnvironmentFile(".env");
if (environmentFile is not null)
{
    LoadEnvironmentFile(environmentFile);
}

var builder = WebApplication.CreateBuilder(args);
var enableSwagger = builder.Configuration.GetValue("Swagger:Enabled", builder.Environment.IsDevelopment());

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                return uri.Host is "localhost" or "127.0.0.1" or "vercel.app";
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "LearningApp API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Lütfen Bearer token'ınızı girin."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            new List<string>()
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<AchievementService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = jwtSettings["Key"]!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await SeedData.InitializeAsync(context);
}

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();
app.MapMethods("/health", new[] { "GET", "HEAD" }, () => Results.Ok("OK")).AllowAnonymous();

app.MapControllers();

app.Run();

static void LoadEnvironmentFile(string filePath)
{
    if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
    {
        return;
    }

    foreach (var rawLine in File.ReadAllLines(filePath))
    {
        var line = rawLine.Trim();
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
        {
            continue;
        }

        var equalsIndex = line.IndexOf('=');
        if (equalsIndex <= 0)
        {
            continue;
        }

        var key = line[..equalsIndex].Trim();
        var value = line[(equalsIndex + 1)..].Trim();

        if (value.Length >= 2 &&
            ((value.StartsWith('"') && value.EndsWith('"')) ||
             (value.StartsWith('\'') && value.EndsWith('\''))))
        {
            value = value[1..^1];
        }

        Environment.SetEnvironmentVariable(key, value);
    }
}

static string? FindEnvironmentFile(string fileName)
{
    var currentDirectory = Directory.GetCurrentDirectory();
    var directory = new DirectoryInfo(currentDirectory);

    while (directory is not null)
    {
        var candidatePath = Path.Combine(directory.FullName, fileName);
        if (File.Exists(candidatePath))
        {
            return candidatePath;
        }

        directory = directory.Parent;
    }

    return null;
}