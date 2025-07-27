var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<CyberCloudDriveAPI.Services.IAuthService, CyberCloudDriveAPI.Services.AuthService>();
builder.Services.AddScoped<CyberCloudDriveAPI.Services.IUserService, CyberCloudDriveAPI.Services.UserService>();
builder.Services.AddIdentity<CyberCloudDriveAPI.Models.User, Microsoft.AspNetCore.Identity.IdentityRole>()
    .AddEntityFrameworkStores<CyberCloudDriveAPI.Data.AppDbContext>();

var app = builder.Build();

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

