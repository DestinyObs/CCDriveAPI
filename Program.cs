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


// Seed initial data (single scope, async)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TheDriveAPI.Data.AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TheDriveAPI.Models.User>>();
    TheDriveAPI.Data.SeedData.InitializeAsync(context, userManager).GetAwaiter().GetResult();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

