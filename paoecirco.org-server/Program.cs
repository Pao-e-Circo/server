using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi;
using paoecirco.org_server;
using paoecirco.org_server.Responses;

var builder = WebApplication.CreateBuilder(args);

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
.WithTags("Vereadores");

app.MapGet("/councilors/{id}", async (PostgresDbContext context, [FromRoute] Guid id) =>
{
    var councilour = await context.Councilours.Where(x => x.Id == id).FirstOrDefaultAsync();

    if (councilour is null)
        return Results.NoContent();

    return Results.Ok(councilour.ToResponse());
})
.WithTags("Vereadores");

app.MapGet("/councilors:spending-rank", async (PostgresDbContext context) =>
{
    int lastMonthProcessed = await context.Attendences
        .OrderByDescending(x => x.Month)
        .Select(x => x.Month.Month)
        .FirstOrDefaultAsync();

    var lastMonthSpending = await context.OfficeSpendings
        .Where(x => x.Month.Month == lastMonthProcessed)
        .Include(x => x.Councilor)
        .ToListAsync();

    var first = lastMonthSpending.OrderBy(x => x.TotalSpent()).First();

    var councilorsWithSameFirstPlaceValue = lastMonthSpending.Where(x => x.TotalSpent() == first.TotalSpent());

    var last = lastMonthSpending.OrderByDescending(x => x.TotalSpent()).First();

    List<CouncilorsSpendingRankResponse> response = [];

    foreach (var i in councilorsWithSameFirstPlaceValue)
    {
        response.Add(new(i.Councilor.Id,
            i.Councilor.Name,
            i.Councilor.Phone,
            i.Councilor.Email,
            i.Councilor.PhotoUrl,
            i.Councilor.Party,
            i.TotalSpent(),
            Winner: true));
    }

    response.Add(new(last.Councilor.Id,
            last.Councilor.Name,
            last.Councilor.Phone,
            last.Councilor.Email,
            last.Councilor.PhotoUrl,
            last.Councilor.Party,
            last.TotalSpent(),
            Winner: false));

    return Results.Ok(response);
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
app.MapGet("home", async (PostgresDbContext context,
    IMemoryCache cache,
    [FromQuery] int? requestYear,
    [FromQuery] int? requestMonth) =>
{
    const string CachingKey = "HomeResponseCache";
    int month = default;

    if (requestYear is not null && requestMonth is not null)
        month = await context.Attendences
            .Where(x => x.Month.Month == requestMonth && x.Month.Year == requestYear)
            .Select(x => x.Month.Month)
            .FirstOrDefaultAsync();
    else
    {
        cache.TryGetValue(CachingKey, out HomeResponse? cached);

        if (cached is not null)
            return Results.Ok(cached);

        month = (await GetMonthForRankingProcessing(context, cache)).OrderByDescending(x => x.Month).Select(x => x.Month).First();
    }

    if (month == default)
        return Results.NoContent();

    var allAttendences = await context.Attendences
        .Where(x => x.Month.Month == month).Include(x => x.Councilor).ToListAsync();

    var attendencesGroupedByCouncilors = allAttendences
        .GroupBy(x => x.CouncilorId)
        .Select(x => x.ToArray());

    var officeSpendingsThisMonth = (await context.OfficeSpendings
        .Where(x => x.Month.Month == month).ToListAsync())
        .GroupBy(x => x.CouncilorId)
        .Select(x => x.ToArray());

    Dictionary<Guid, (int Attendences, int Absences, decimal OfficeSpending)> variablesToCalculate = [];

    foreach (var i in attendencesGroupedByCouncilors)
    {
        if (variablesToCalculate.ContainsKey(i.First().CouncilorId))
            continue;

        Guid councilorId = i.First().CouncilorId;
        int attendences = i.Where(x => x.Status == "PRESENTE").Count();
        int absences = i.Where(x => x.Status != "PRESENTE").Count();

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

    cache.Set(CachingKey, response, TimeSpan.FromHours(6));

    return Results.Ok(response);
})
.WithTags("Destaques do mês");

app.MapGet("/councilors:ranking-dropdown", async (PostgresDbContext context, IMemoryCache cache) =>
{
    var dates = await GetMonthForRankingProcessing(context, cache);

    CouncilorsRankingDropdown response = new()
    {
        Items = dates
            .OrderByDescending(x => x)
            .Select(dateOnly => new CouncilorsRankingDropdownItem(dateOnly))
            .ToList()
    };

    return Results.Ok(response);
})
.WithTags("Destaques do mês");
#endregion

#region Services methods
async Task<IList<DateOnly>> GetMonthForRankingProcessing(PostgresDbContext context, IMemoryCache cache)
{
    const string CachingKey = "MonthsForRankingProcessing";

    cache.TryGetValue(CachingKey, out IList<DateOnly>? cachedMonths);
    if (cachedMonths is not null) return cachedMonths;

    int lastYear = DateTime.UtcNow.Year - 1;

    var attendencesProcessedThisYear = await context.Attendences
        .Where(x => x.Month.Year == lastYear)
        .Select(x => x.Month.Month)
        .Distinct()
        .OrderBy(x => x)
        .ToListAsync();

    var officesSpendingsProcessedThisYear = await context.OfficeSpendings
        .Where(x => x.Month.Year == lastYear)
        .Select(x => x.Month.Month)
        .Distinct()
        .OrderBy(x => x)
        .ToListAsync();

    // Pode acontecer de ter meses processados nas sessões, mas não nos gastos de gabinete - e vice versa.
    bool hasMoreAttendencesThanSpendings = attendencesProcessedThisYear.Count > officesSpendingsProcessedThisYear.Count;
    IList<DateOnly> dates = [];

    if (hasMoreAttendencesThanSpendings)
    {
        dates = await context.OfficeSpendings
            .Where(x => x.Month.Year == lastYear)
            .Select(x => new DateOnly(x.Month.Year, x.Month.Month, 1))
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();
    }
    else
    {
        dates = await context.Attendences
            .Where(x => x.Month.Year == lastYear)
            .Select(x => new DateOnly(x.Month.Year, x.Month.Month, 1))
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();
    }

    cache.Set(CachingKey, dates, TimeSpan.FromHours(12));

    return dates;
}
#endregion

app.Run();
