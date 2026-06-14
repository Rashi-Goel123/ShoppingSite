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

// === AUTHENTICATION ===
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
        // Allow SignalR to use JWT from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
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

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:4300",
                "https://shoppingsite.weeb.onrender.com"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// === SWAGGER (using built-in OpenAPI) ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header. Enter ONLY your token (without 'Bearer ' prefix). Example: eyJhbGci...",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

var app = builder.Build();

// === MIDDLEWARE ===
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAngular");

// Serve static files (uploads)
app.UseStaticFiles();
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR Hubs
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<OrderTrackingHub>("/hubs/orders");
app.MapHub<NotificationHub>("/hubs/notifications");

// === DATABASE MIGRATION & SEED ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(db);
}

app.Run();

public class SecurityRequirementsOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(Microsoft.OpenApi.OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        var hasAuthorize = System.Linq.Enumerable.Any(System.Linq.Enumerable.OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>(context.MethodInfo.DeclaringType!.GetCustomAttributes(true))) ||
                           System.Linq.Enumerable.Any(System.Linq.Enumerable.OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>(context.MethodInfo.GetCustomAttributes(true)));

        if (hasAuthorize)
        {
            if (operation.Security == null)
                operation.Security = new System.Collections.Generic.List<Microsoft.OpenApi.OpenApiSecurityRequirement>();

            var scheme = new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer");
            var requirement = new Microsoft.OpenApi.OpenApiSecurityRequirement
            {
                { scheme, new System.Collections.Generic.List<string>() }
            };
            operation.Security.Add(requirement);
        }
    }
}

