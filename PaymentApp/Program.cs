using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaymentApp.Data;
using PaymentApp.Models;
using PaymentApp.Services.Auth;
using PaymentApp.Services.Payment;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<PaymentDbContext>(options => options.UseNpgsql("Host=localhost;Database=payapp;Username=payapp;Password=devpass"));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();


var app = builder.Build();

app.MapControllers();

app.Run();
