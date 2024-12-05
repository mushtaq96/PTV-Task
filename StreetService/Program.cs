using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite;
using StreetService.Data;
using StreetService.Models;
using StreetService.Controllers;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register the StreetContext
builder.Services.AddDbContext<StreetContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
    o => o.UseNetTopologySuite()));

builder.Services.AddFeatureManagement(); // Register the hidden feature manager
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
