using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using paoecirco.org_server;
using paoecirco.org_server.Domain;
using paoecirco.org_server.Responses.Attendence;
using paoecirco.org_server.Responses.Councilor;
using paoecirco.org_server.Responses.OfficeSpending;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontPolicy", policy =>
    {
        policy
            .WithOrigins("https://www.paoecirco.org")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<PostgresDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("DatabaseCredentials"), npgsql =>
    {
        npgsql.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null
        );
    }
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

builder.Services.AddMemoryCache();

var app = builder.Build();
app.UseCors("FrontPolicy");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
    db.Database.EnsureCreated();
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

app.MapGet("/health", () => Results.Ok("Healthy!!! :)"))
.WithTags("Health");


#region Councilors Domain
app.MapGet("/councilors", async (PostgresDbContext context) =>
{
    var councilours = await context.Councilours.ToListAsync();
    return Results.Ok(councilours.Select(x => x.ToResponse()));
})
.WithTags("Vereadores")
.WithSummary("Consulta todos os vereadores cadastrados")
.WithDescription("""
- Retorna 204 se não houver registros.
""");

app.MapGet("/councilors/{id}", async (PostgresDbContext context, [FromRoute] Guid id) =>
{
    var councilour = await context.Councilours.Where(x => x.Id == id).FirstOrDefaultAsync();

    if (councilour is null)
        return Results.NoContent();

    return Results.Ok(councilour.ToResponse());
})
.WithTags("Vereadores")
.WithSummary("Consulta o vereador especificado")
.WithDescription("""
- Retorna 204 se não houver registros.
""");
#endregion

#region OfficeSpending Domain
app.MapGet("/office-spendings/{councilor_id}", async (PostgresDbContext context,
    [FromRoute(Name = "councilor_id")] Guid councilorId,
    [FromQuery] int? year,
    [FromQuery] int? month) =>
{
    OfficeSpendingByIdResponse? response;

    if (month is not null && year is not null)
    {
        response = await context.OfficeSpendings
            .Where(x => x.CouncilorId == councilorId && x.Month.Month == month && x.Month.Year == year)
            .Select(x => x.ToOfficeByIdResponse())
            .FirstOrDefaultAsync();
    }
    else
    {
        response = await context.OfficeSpendings.
            Where(x => x.CouncilorId == councilorId)
            .OrderByDescending(x => x.Month)
            .Select(x => x.ToOfficeByIdResponse())
            .FirstOrDefaultAsync();
    }

    if (response is null)
        return Results.NoContent();

    return Results.Ok(response);
})
.WithTags("Despesas de Gabinete")
.WithSummary("Consulta o gasto de gabinete do vereador especificado")
.WithDescription("""
- Retorna 204 se não houver registros.
""");

app.MapGet("/office-spendings", async (PostgresDbContext context, [FromQuery] int? year, [FromQuery] int? month) =>
{
    IList<OfficeSpending> officeSpending = [];

    if (month is not null && year is not null)
    {
        officeSpending = await context.OfficeSpendings
            .Where(x => x.Month.Month == month && x.Month.Year == year)
            .Include(x => x.Councilor)
            .ToListAsync();
    }
    else
    {
        int lastMonth = await context.OfficeSpendings
            .OrderByDescending(x => x.Month)
            .Select(x => x.Month.Month)
            .FirstOrDefaultAsync();

        officeSpending = await context.OfficeSpendings
            .Where(x => x.Month.Month == lastMonth)
            .Include(x => x.Councilor)
            .ToListAsync();
    }

    if (officeSpending is null)
        return Results.NoContent();

    return Results.Ok(officeSpending
        .OrderBy(x => x.TotalSpent())
        .Select(x => x.ToAllOfficeSpendingsResponse())
    );
})
.WithTags("Despesas de Gabinete")
.WithSummary("Consulta o gasto de gabinete de todos os vereadores")
.WithDescription("""
Retorna os gastos de gabinete de todos os vereadores, ordenado pelos vereadores que mais menos gastaram.
- Retorna 204 se não houver registros.
""");
#endregion

#region Attendences Domain
app.MapGet("attendences/{councilor_id}", async (PostgresDbContext context, 
    [FromRoute(Name = "councilor_id")] Guid councilorId,
    [FromQuery] int? year,
    [FromQuery] int? month) =>
{
    IEnumerable<AttendenceResponse> attendences = [];

    if (month is not null && year is not null)
    {
        attendences = await context.Attendences
            .Where(x => x.CouncilorId == councilorId && x.Month.Month == month && x.Month.Year == year)
            .OrderBy(x => x.Month.Day)
            .Select(x => x.ToAttendenceResponse())
            .ToListAsync();
    }
    else
    {
        int lastMonth = await context.Attendences
            .OrderByDescending(x => x.Month)
            .Select(x => x.Month.Month)
            .FirstOrDefaultAsync();

        attendences = await context.Attendences.
            Where(x => x.CouncilorId == councilorId && x.Month.Month == lastMonth)
            .OrderBy(x => x.Month.Day)
            .Select(x => x.ToAttendenceResponse())
            .ToListAsync();
    }

    if (!attendences.Any())
        return Results.NoContent();

    return Results.Ok(attendences);
})
.WithTags("Presenças de sessões extraordinárias e ordinárias")
.WithSummary("Consulta presenças do vereador específicado")
.WithDescription("""
- Retorna 204 se não houver registros.
""");

app.MapGet("attendences", async (PostgresDbContext context, [FromQuery] int? year, [FromQuery] int? month) =>
{
    IEnumerable<Attendence> attendences = [];

    if (month is not null && year is not null)
    {
        attendences = await context.Attendences
            .Where(x => x.Month.Month == month && x.Month.Year == year)
            .OrderBy(x => x.Month.Day)
            .Include(x => x.Councilor)
            .ToListAsync();
    }
    else
    {
        int lastMonth = await context.Attendences
            .OrderByDescending(x => x.Month)
            .Select(x => x.Month.Month)
            .FirstOrDefaultAsync();

        attendences = await context.Attendences.
            Where(x => x.Month.Month == lastMonth)
            .OrderBy(x => x.Month.Day)
            .Include(x => x.Councilor)
            .ToListAsync();
    }

    if (!attendences.Any())
        return Results.NoContent();

    var grouped = attendences.GroupBy(x => x.CouncilorId)
        .Select(g => g.ToList())
        .ToList();

    IList<AllAttendencesResponse> response = [];

    foreach (var x in grouped)
    {
        AllAttendencesResponse attendenceResponse = x.First().ToAllAttendencesResponse(
            totalAttendences: x.Count(a => a.Status == "PRESENTE"),
            totalAbsences: x.Count(a => a.Status == "AUSENTE"),
            totalJustified: x.Count(a => a.Status == "Justificado")
        );
        response.Add(attendenceResponse);
    }

    return Results.Ok(response.OrderByDescending(x => x.TotalAttendences));
})
.WithTags("Presenças de sessões extraordinárias e ordinárias")
.WithSummary("Consulta o total de presenças, ausências e justificados de um vereador de um determinado mês.")
.WithDescription("""
Retorna as presenças de todos os vereadores, ordenado pelos vereadores que mais possuem presenças.
- Retorna 204 se não houver registros.
""");
#endregion

app.Run();
