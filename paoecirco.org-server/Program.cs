using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using paoecirco.org_server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PostgresDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("DatabaseCredentials")
));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "paoecirco.org Web API",
        Description = "Ranking de vereadores da Câmara Municipal de Blumenau.",
        Version = "v1"
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
    db.Database.Migrate();
}

app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "paoecirco.org API V1");
    c.RoutePrefix = "swagger";
});

#region Councilours Domain
app.MapGet("/councilours", async (PostgresDbContext context) =>
{
    var councilours = await context.Councilours.ToListAsync();
    return Results.Ok(councilours.Select(x => x.ToResponse()));
})
.WithTags("Vereadores");

app.MapGet("/councilours/{id}", async (PostgresDbContext context, [FromRoute] Guid id) =>
{
    var councilour = await context.Councilours.Where(x => x.Id == id).FirstOrDefaultAsync();

    if (councilour is null)
        return Results.NoContent();

    return Results.Ok(councilour.ToResponse());
})
.WithTags("Vereadores");
#endregion

#region OfficeSpending Domain
app.MapGet("/office-spendings", async (PostgresDbContext context) =>
{
    var officeSpendings = await context.OfficeSpendings.ToListAsync();
    return Results.Ok(officeSpendings.Select(x => x.ToResponse()));
})
.WithTags("Despesas de Gabinete");

app.MapGet("/office-spendings/{councilor_id}", async (PostgresDbContext context, [FromRoute(Name = "councilor_id")] Guid councilorId) =>
{
    var officeSpendingsForCouncilor = await context.OfficeSpendings.Where(x => x.CouncilorId == councilorId).FirstOrDefaultAsync();

    if (officeSpendingsForCouncilor is null)
        return Results.NoContent();

    return Results.Ok(officeSpendingsForCouncilor.ToResponse());
})
.WithTags("Despesas de Gabinete");
#endregion

#region Attendences Domain
app.MapGet("attendences/{councilor_id}", async (PostgresDbContext context, [FromRoute(Name = "councilor_id")] Guid councilorId) =>
{
    var attendencesForCouncilor = await context.Attendences.Where(x => x.CouncilorId == councilorId).FirstOrDefaultAsync();

    if (attendencesForCouncilor is null)
        return Results.NoContent();

    return Results.Ok(attendencesForCouncilor.ToResponse());
})
.WithTags("Presenças de sessões extraordinárias e ordinárias");
#endregion

app.Run();
