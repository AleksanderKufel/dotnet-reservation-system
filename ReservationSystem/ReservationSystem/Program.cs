using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Validators;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.API.Exceptions;
using ReservationSystem.API.Validators;
using ReservationSystem.Application.Interfaces;
using ReservationSystem.Application.Services;
using ReservationSystem.Domain.Services;
using ReservationSystem.Infrastructure.Identity;
using ReservationSystem.Infrastructure.Persistence;
using ReservationSystem.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// Database
// ======================================================

builder.Services.AddDbContext<ReservationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// ======================================================
// Infrastructure
// ======================================================

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

builder.Services
    .AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<ReservationDbContext>();

// ======================================================
// Domain
// ======================================================

builder.Services.AddScoped<ReservationConflictChecker>();

// ======================================================
// Application
// ======================================================

builder.Services.AddScoped<ReservationService>();

// ======================================================
// API / Framework
// ======================================================

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

ValidatorOptions.Global.LanguageManager.Enabled = false;

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// ======================================================
// Middleware pipeline
// ======================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapIdentityApi<User>();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();