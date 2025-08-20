using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;


var builder = WebApplication.CreateBuilder(args);
// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JWT");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var secret = jwtSettings["Secret"] ?? "supersecret";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };
});

// Add CORS policy for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:8080", "https://thedrive.netlify.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
// Register AppDbContext with SQL Server
builder.Services.AddDbContext<TheDriveAPI.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SupportNonNullableReferenceTypes();
    c.SwaggerDoc("v1", new() { Title = "TheDriveAPI", Version = "v1" });
    c.OperationFilter<TheDriveAPI.Swagger.AddFileUploadParamTypesOperationFilter>();

    // Add JWT Bearer support to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddScoped<TheDriveAPI.Services.IAuthService, TheDriveAPI.Services.AuthService>();
builder.Services.AddScoped<TheDriveAPI.Services.IUserService, TheDriveAPI.Services.UserService>();
builder.Services.AddScoped<TheDriveAPI.Services.IFolderService, TheDriveAPI.Services.FolderService>();
builder.Services.AddScoped<TheDriveAPI.Services.IPricingService, TheDriveAPI.Services.PricingService>();
builder.Services.AddScoped<TheDriveAPI.Services.IActivityService, TheDriveAPI.Services.ActivityService>();
builder.Services.AddScoped<TheDriveAPI.Services.ISupportService, TheDriveAPI.Services.SupportService>();
builder.Services.AddScoped<TheDriveAPI.Services.IFileService, TheDriveAPI.Services.FileService>();
builder.Services.AddIdentity<TheDriveAPI.Models.User, Microsoft.AspNetCore.Identity.IdentityRole>()
    .AddEntityFrameworkStores<TheDriveAPI.Data.AppDbContext>();

var app = builder.Build();

// Check if this is migration-only mode
var commandArgs = Environment.GetCommandLineArgs();
bool migrationMode = commandArgs.Contains("--migrate-only");

// Always run migrations first
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TheDriveAPI.Data.AppDbContext>();
    
    // Run migrations
    Console.WriteLine("Running database migrations...");
    await context.Database.MigrateAsync();
    Console.WriteLine("Database migrations completed successfully");
    
    // If migration-only mode, exit after migrations
    if (migrationMode)
    {
        Console.WriteLine("Migration-only mode completed. Exiting...");
        return;
    }
    
    // Seed initial data
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TheDriveAPI.Models.User>>();
    await TheDriveAPI.Data.SeedData.InitializeAsync(context, userManager);
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// Only use HTTPS redirection if HTTPS is properly configured
// app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

