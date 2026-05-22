using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.API.Exceptions;
using ReservationSystem.API.Validators;
using ReservationSystem.Application.Interfaces;
using ReservationSystem.Application.Services;
using ReservationSystem.Domain.Services;
using ReservationSystem.Infrastructure.Persistence;
using ReservationSystem.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// Database
// ======================================================

builder.Services.AddDbContext<ReservationDbContext>(options =>
{
    options.UseInMemoryDatabase("ReservationDb");
});

// ======================================================
// Infrastructure
// ======================================================

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();