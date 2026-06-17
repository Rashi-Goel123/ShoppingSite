// using System.Text;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.IdentityModel.Tokens;
// using EcommerceApi.Data;
// using EcommerceApi.Hubs;
// using EcommerceApi.Seeds;
// using EcommerceApi.Services;

// var builder = WebApplication.CreateBuilder(args);

// // === DATABASE ===
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// // === AUTHENTICATION ===
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuerSigningKey = true,
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
//             ValidateIssuer = true,
//             ValidIssuer = builder.Configuration["Jwt:Issuer"],
//             ValidateAudience = true,
//             ValidAudience = builder.Configuration["Jwt:Audience"],
//             ValidateLifetime = true
//         };
//         // Allow SignalR to use JWT from query string
//         options.Events = new JwtBearerEvents
//         {
//             OnMessageReceived = context =>
//             {
//                 var accessToken = context.Request.Query["access_token"];
//                 var path = context.HttpContext.Request.Path;
//                 if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
//                 {
//                     context.Token = accessToken;
//                 }
//                 return Task.CompletedTask;
//             }
//         };
//     });
// builder.Services.AddAuthorization();

// // === SERVICES ===
// builder.Services.AddScoped<IJwtService, JwtService>();
// builder.Services.AddScoped<IOtpService, OtpService>();
// builder.Services.AddScoped<IRazorpayService, RazorpayService>();
// builder.Services.AddScoped<IEmailService, EmailService>();
// builder.Services.AddScoped<IGeminiChatService, GeminiChatService>();
// builder.Services.AddHttpClient();

// // === SIGNALR ===
// builder.Services.AddSignalR();

// // === CORS ===
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAngular", policy =>
//     {
//         policy.WithOrigins(
//                 "http://localhost:4200",
//                 "http://localhost:4300",
//                 "https://shoppingsite-weeb.onrender.com"
//               )
//               .AllowAnyHeader()
//               .AllowAnyMethod()
//               .AllowCredentials();
//     });
// });

// // === SWAGGER (using built-in OpenAPI) ===
// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(options =>
// {
//     options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Description = "JWT Authorization header. Enter ONLY your token (without 'Bearer ' prefix). Example: eyJhbGci...",
//         In = Microsoft.OpenApi.ParameterLocation.Header,
//         Type = Microsoft.OpenApi.SecuritySchemeType.Http,
//         Scheme = "bearer"
//     });

//     options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
//     {
//         {
//             new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer"),
//             new System.Collections.Generic.List<string>()
//         }
//     });

//     options.OperationFilter<CustomAuthHeaderOperationFilter>();
// });

// var app = builder.Build();

// // === MIDDLEWARE ===
// app.UseSwagger();
// app.UseSwaggerUI();

// app.UseCors("AllowAngular");

// // Serve static files (uploads)
// app.UseStaticFiles();
// var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");
// Directory.CreateDirectory(uploadsPath);
// app.UseStaticFiles(new StaticFileOptions
// {
//     FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
//     RequestPath = "/uploads"
// });

// app.Use(async (context, next) =>
// {
//     if (context.Request.Headers.TryGetValue("X-Authorization", out var token))
//     {
//         context.Request.Headers["Authorization"] = token;
//     }
//     await next();
// });

// app.UseAuthentication();
// app.UseAuthorization();

// app.MapControllers();

// // SignalR Hubs
// app.MapHub<EcommerceApi.Hubs.ChatHub>("/hubs/chat");
// app.MapHub<EcommerceApi.Hubs.OrderTrackingHub>("/hubs/orders");
// app.MapHub<EcommerceApi.Hubs.NotificationHub>("/hubs/notifications");

// // === DATABASE MIGRATION & SEED ===
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<EcommerceApi.Data.AppDbContext>();
//     await db.Database.MigrateAsync();
//     await EcommerceApi.Seeds.DatabaseSeeder.SeedAsync(db);
// }

// app.Run();

// public class CustomAuthHeaderOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
// {
//     public void Apply(Microsoft.OpenApi.OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
//     {
//         var hasAuthorize = System.Linq.Enumerable.Any(System.Linq.Enumerable.OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>(context.MethodInfo.DeclaringType!.GetCustomAttributes(true))) ||
//                            System.Linq.Enumerable.Any(System.Linq.Enumerable.OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>(context.MethodInfo.GetCustomAttributes(true)));

//         if (!hasAuthorize) return;

//         if (operation.Parameters == null)
//             operation.Parameters = new System.Collections.Generic.List<Microsoft.OpenApi.IOpenApiParameter>();

//         operation.Parameters.Add(new Microsoft.OpenApi.OpenApiParameter
//         {
//             Name = "X-Authorization",
//             In = Microsoft.OpenApi.ParameterLocation.Header,
//             Description = "Swagger UI blocks 'Authorization'. Enter 'Bearer {token}' here instead.",
//             Required = false
//         });
//     }
// }
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EcommerceApi.Data;
using EcommerceApi.Hubs;
using EcommerceApi.Seeds;
using EcommerceApi.Services;

var builder = WebApplication.CreateBuilder(args);

// === DATABASE ===
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:4300",
                "https://shoppingsite-weeb.onrender.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// === AUTHENTICATION ===
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)
            ),

            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Allow SignalR to use JWT from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// === SERVICES ===
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IRazorpayService, RazorpayService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGeminiChatService, GeminiChatService>();
builder.Services.AddHttpClient();

// === SIGNALR ===
builder.Services.AddSignalR();

// === CONTROLLERS + SWAGGER ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header. Enter ONLY your token without 'Bearer ' prefix. Example: eyJhbGci...",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer"),
            new System.Collections.Generic.List<string>()
        }
    });

    options.OperationFilter<CustomAuthHeaderOperationFilter>();
});

var app = builder.Build();

// === SWAGGER ===
app.UseSwagger();
app.UseSwaggerUI();

// === STATIC FILES ===
app.UseStaticFiles();

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// === CORS MUST BE BEFORE AUTHENTICATION / AUTHORIZATION ===
app.UseCors("AllowAngular");

// === CUSTOM X-AUTHORIZATION HEADER FOR SWAGGER ===
app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-Authorization", out var token))
    {
        context.Request.Headers["Authorization"] = token;
    }

    await next();
});

// === AUTH MIDDLEWARE ===
app.UseAuthentication();
app.UseAuthorization();

// === CORS TEST ENDPOINT ===
// Test from browser console:
// fetch("https://ecommerce-api-ugym.onrender.com/cors-test")
//   .then(res => res.json())
//   .then(console.log)
//   .catch(console.error);
app.MapGet("/cors-test", () => Results.Ok(new
{
    message = "CORS working successfully",
    time = DateTime.UtcNow
}))
.RequireCors("AllowAngular");

// === CONTROLLERS ===
app.MapControllers()
   .RequireCors("AllowAngular");

// === SIGNALR HUBS ===
app.MapHub<ChatHub>("/hubs/chat")
   .RequireCors("AllowAngular");

app.MapHub<OrderTrackingHub>("/hubs/orders")
   .RequireCors("AllowAngular");

app.MapHub<NotificationHub>("/hubs/notifications")
   .RequireCors("AllowAngular");

// === DATABASE MIGRATION & SEED ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

    await DatabaseSeeder.SeedAsync(db);
}

app.Run();

public class CustomAuthHeaderOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(
        Microsoft.OpenApi.OpenApiOperation operation,
        Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        var hasAuthorize =
            System.Linq.Enumerable.Any(
                System.Linq.Enumerable.OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>(
                    context.MethodInfo.DeclaringType!.GetCustomAttributes(true)
                )
            )
            ||
            System.Linq.Enumerable.Any(
                System.Linq.Enumerable.OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>(
                    context.MethodInfo.GetCustomAttributes(true)
                )
            );

        if (!hasAuthorize)
            return;

        if (operation.Parameters == null)
        {
            operation.Parameters = new System.Collections.Generic.List<Microsoft.OpenApi.IOpenApiParameter>();
        }

        operation.Parameters.Add(new Microsoft.OpenApi.OpenApiParameter
        {
            Name = "X-Authorization",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Description = "Swagger UI blocks 'Authorization'. Enter 'Bearer {token}' here instead.",
            Required = false
        });
    }
}

