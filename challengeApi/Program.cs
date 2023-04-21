using AspNetCoreRateLimit;
using challengeApi.Data;
using challengeApi.Models;
using ChallengeApi.Models;
using EasyCaching.Core;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddRateLimiter(options =>
{
  
});
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;

    options.RealIpHeader = "X-Real-IP";   

    options.GeneralRules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Endpoint = "*",
                Period = "10s",
                Limit = 2
            }
        };
});
builder.Services.AddEasyCaching(options =>
{
    options.UseInMemory("inMemoryCache");
});

var app = builder.Build();
app.UseIpRateLimiting();
app.UseMetricServer(); // see http://localhost:49557/metrics
app.UseHttpMetrics();
app.UseSerilogRequestLogging();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var _logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

string logtemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3} {Username} {Message:lj}{Exception}{NewLine}";
    

app.MapGet("/todoitems", async (TodoDb db) =>
  {
       var output= await db.Todos.ToListAsync();

      _logger.Information(logtemplate, new LogModel() { Direction = 1, Rout = "/todoitems", HttpVerb = "get",Message= JsonSerializer.Serialize( db.Todos) });
      
      return output;
  });

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
{
    _logger.Information(logtemplate, new LogModel() { Direction = 0, Rout = "/todoitems/" + id.ToString(), HttpVerb = "get",Message=id.ToString() });

    var output = await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound();
    
    _logger.Information(logtemplate, new LogModel() { Direction = 1, Rout = "/todoitems/" + id.ToString(), HttpVerb = "get" , Message = JsonSerializer.Serialize(output) });

    return output;
}
);

app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    _logger.Information(logtemplate, new LogModel() { Direction = 0, Rout = "/todoitems/" , HttpVerb = "post" , Message = JsonSerializer.Serialize( todo)});

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    var output=Results.Created($"/todoitems/{todo.Id}", todo);
    _logger.Information(logtemplate, new LogModel() { Direction = 0, Rout = "/todoitems/", HttpVerb = "post", Message = JsonSerializer.Serialize( output) });

    return output;
});

app.MapGet("/httpbin", async (IEasyCachingProvider _provider) =>
{
    
    using var client = new HttpClient();

    var result = await client.GetAsync("http://httpbin.org/get");
    _logger.Information(logtemplate, new LogModel() { Direction = 1, Rout = "/todoitems/", HttpVerb = "post" , Message = JsonSerializer.Serialize(result) });

    await _provider.SetAsync("httpbinCache", result, TimeSpan.FromMinutes(60));

    return result;
});

app.Run();


