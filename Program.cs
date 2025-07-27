using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:8080", "https://your-frontend-domain.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
// Register AppDbContext with SQL Server
builder.Services.AddDbContext<CyberCloudDriveAPI.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SupportNonNullableReferenceTypes();
    c.SwaggerDoc("v1", new() { Title = "CyberCloudDriveAPI", Version = "v1" });
    c.OperationFilter<CyberCloudDriveAPI.Swagger.AddFileUploadParamTypesOperationFilter>();
});
builder.Services.AddScoped<CyberCloudDriveAPI.Services.IAuthService, CyberCloudDriveAPI.Services.AuthService>();
builder.Services.AddScoped<CyberCloudDriveAPI.Services.IUserService, CyberCloudDriveAPI.Services.UserService>();
builder.Services.AddScoped<CyberCloudDriveAPI.Services.IFolderService, CyberCloudDriveAPI.Services.FolderService>();
builder.Services.AddScoped<CyberCloudDriveAPI.Services.IPricingService, CyberCloudDriveAPI.Services.PricingService>();
builder.Services.AddScoped<CyberCloudDriveAPI.Services.IActivityService, CyberCloudDriveAPI.Services.ActivityService>();
builder.Services.AddScoped<CyberCloudDriveAPI.Services.ISupportService, CyberCloudDriveAPI.Services.SupportService>();
builder.Services.AddScoped<CyberCloudDriveAPI.Services.IFileService, CyberCloudDriveAPI.Services.FileService>();
builder.Services.AddIdentity<CyberCloudDriveAPI.Models.User, Microsoft.AspNetCore.Identity.IdentityRole>()
    .AddEntityFrameworkStores<CyberCloudDriveAPI.Data.AppDbContext>();

var app = builder.Build();


// Seed initial data (single scope, async)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CyberCloudDriveAPI.Data.AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<CyberCloudDriveAPI.Models.User>>();
    CyberCloudDriveAPI.Data.SeedData.InitializeAsync(context, userManager).GetAwaiter().GetResult();
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
app.UseAuthorization();
app.MapControllers();

app.Run();

