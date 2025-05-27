using InboxService.Data;
using InboxService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ?? 1. CORS-policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "https://localhost:7154",
            "https://ventixe.azurewebsites.net"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// ?? 2. Lägg till globalt [Authorize] om du vill
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
});

builder.Services.AddOpenApi();
builder.Services.AddDbContext<InboxDbContext>(x =>
    x.UseSqlServer(builder.Configuration.GetConnectionString("LocalDB")));

builder.Services.AddHttpClient("UserProfileService", client =>
{
    client.BaseAddress = new Uri("https://userprofileservice20250526122221-hucuazd2dbgqhee6.swedencentral-01.azurewebsites.net/api/");
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserProfileServiceClient>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };

        // ?? Aktivera token via cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["jwt"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// ?? 3. Middleware
app.UseCors("AllowFrontend");
app.MapOpenApi();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();