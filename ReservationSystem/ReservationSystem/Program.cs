using Microsoft.EntityFrameworkCore;
using ReservationSystem.Application.Interfaces;
using ReservationSystem.Application.Services;
using ReservationSystem.Domain.Services;
using ReservationSystem.Infrastructure.Persistence;
using ReservationSystem.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Services
// =======================

// DbContext (InMemory na dev)
builder.Services.AddDbContext<ReservationDbContext>(options =>
    options.UseInMemoryDatabase("ReservationDb"));

// Infrastructure
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

// Application
builder.Services.AddScoped<ReservationService>();

// Domain
builder.Services.AddScoped<ReservationConflictChecker>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =======================
// Middleware
// =======================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();