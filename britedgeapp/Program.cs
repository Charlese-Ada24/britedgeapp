using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging; // <- for ILogger

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", (ILogger<Program> log) =>
{
    log.LogInformation("Home '/' hit at {UtcTime}", DateTimeOffset.UtcNow);
    return Results.Ok("BritEdge demo running");
});

app.MapGet("/healthz", async (IConfiguration cfg, ILogger<Program> log) =>
{
    var cs = cfg.GetConnectionString("SqlConnection");
    if (string.IsNullOrWhiteSpace(cs))
    {
        log.LogWarning("Healthz called but SqlConnection is missing");
        return Results.Problem("Missing SqlConnection.");
    }

    try
    {
        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        var ok = Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 1;

        log.LogInformation("Healthz DB check succeeded: {Ok}", ok);
        return ok ? Results.Ok("OK") : Results.StatusCode(500);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Healthz DB check failed");
        return Results.StatusCode(500);
    }
});

app.Run();


