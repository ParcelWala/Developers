using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Parcelwala.DataAccess.Data;
using Parcelwala.DataAccess.Services;
using ParcelwalaAPP.DataAccess.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------
// 1. Configure Services
// ------------------------------------

// Database (SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Services
builder.Services.AddScoped<IOtpAuthService, OtpAuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IReferralService, ReferralService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IGoodsTypeService, GoodsTypeService>();
builder.Services.AddScoped<IRestrictedItemService, RestrictedItemService>();
builder.Services.AddScoped<IAddressService, AddressService>();


// MVC + API Controllers
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

var app = builder.Build();

// ------------------------------------
// 2. Configure Middleware (Correct Order)
// ------------------------------------

// Always enable Swagger (Development + Production)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();

// Map API controllers first
app.MapControllers();

// Map MVC default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

app.Run();
