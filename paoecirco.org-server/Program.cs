using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using paoecirco.org_server;
using paoecirco.org_server.Domain;
using paoecirco.org_server.Responses;

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

#region Councilors Domain
app.MapGet("/councilors", async (PostgresDbContext context) =>
{
    var councilours = await context.Councilours.ToListAsync();
    return Results.Ok(councilours.Select(x => x.ToResponse()));
})
.WithTags("Vereadores");

app.MapGet("/councilors/{id}", async (PostgresDbContext context, [FromRoute] Guid id) =>
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

#region Other
app.MapGet("home", async (PostgresDbContext context) =>
{
    string Attendence = "PRESENTE";
    int lastMonth = DateTime.Now.Month - 2; // TODO alterar pra pegar sempre o último mês na base, pois pode acontecer de não ter dados do mês anterior.

    var allAttendences = await context.Attendences
        .Where(x => x.Month.Month == lastMonth).Include(x => x.Councilor).ToListAsync();

    var attendencesGroupedByCouncilors = allAttendences
        .GroupBy(x => x.CouncilorId)
        .Select(x => x.ToArray());

    var officeSpendingsThisMonth = (await context.OfficeSpendings
        .Where(x => x.Month.Month == lastMonth).ToListAsync())
        .GroupBy(x => x.CouncilorId)
        .Select(x => x.ToArray());

    Dictionary<Guid, (int Attendences, int Absences, decimal OfficeSpending)> variablesToCalculate = [];

    foreach (var i in attendencesGroupedByCouncilors)
    {
        if (variablesToCalculate.ContainsKey(i.First().CouncilorId))
            continue;

        Guid councilorId = i.First().CouncilorId;
        int attendences = i.Where(x => x.Status == Attendence).Count();
        int absences = i.Where(x => x.Status != Attendence).Count();

        decimal officeSpending = 0;

        foreach (var u in officeSpendingsThisMonth)
        {
            if (u.First().CouncilorId != councilorId)
                continue;

            officeSpending = u.Sum(x => x.TotalSpent());
            break;
        }

        variablesToCalculate.Add(councilorId, (attendences, absences, officeSpending));
    }

    var councilors = attendencesGroupedByCouncilors.Select(x => x.First().Councilor);

    IEnumerable<CouncilorHome> ranked = variablesToCalculate
        .Rank(councilors)
        .Select(x => x.ToCouncilorHome(x.OfficeSpendings.Sum(x => x.TotalSpent())));

    HomeResponse response = new()
    {
        MonthHighlight = new()
        {
            TheColeest = ranked.First(),
            TheSpender = ranked.ElementAt(ranked.Count() - 1)
        },
        Rank = ranked
    };

    return Results.Ok(response);
})
.WithTags("Destaques do mês");
#endregion

app.Run();
