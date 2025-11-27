using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using paoecirco.org_server;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PostgresDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("DatabaseCredentials")
));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PizzaStore API",
        Description = "Making the Pizzas you love",
        Version = "v1"
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
    });
}

app.MapGet("/councilours", async (PostgresDbContext context) =>
{
    var councilours = await context.Councilours.ToListAsync();
    return Results.Ok(councilours);
});


app.Run();
