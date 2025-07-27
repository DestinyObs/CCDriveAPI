builder.Services.AddScoped<CyberCloudDriveAPI.Services.IFileService, CyberCloudDriveAPI.Services.FileService>();
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
// Register AppDbContext with SQL Server
builder.Services.AddDbContext<CyberCloudDriveAPI.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<CyberCloudDriveAPI.Services.IAuthService, CyberCloudDriveAPI.Services.AuthService>();
builder.Services.AddScoped<CyberCloudDriveAPI.Services.IUserService, CyberCloudDriveAPI.Services.UserService>();
builder.Services.AddIdentity<CyberCloudDriveAPI.Models.User, Microsoft.AspNetCore.Identity.IdentityRole>()
    .AddEntityFrameworkStores<CyberCloudDriveAPI.Data.AppDbContext>();

var app = builder.Build();
// Seed initial data
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
app.UseAuthorization();
app.MapControllers();

app.Run();

