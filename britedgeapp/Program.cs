using System;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "BritEdge demo running");

// Health check that uses the App Service connection string named "SqlConnection"
app.MapGet("/healthz", async (IConfiguration cfg) =>
{
    var cs = cfg.GetConnectionString("SqlConnection");
    if (string.IsNullOrWhiteSpace(cs))
        return Results.Problem("Missing connection string 'SqlConnection'.");

    await using var conn = new SqlConnection(cs);
    await conn.OpenAsync();

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT 1";

    var result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
    return result == 1 ? Results.Ok("OK") : Results.StatusCode(500);
});

app.Run();

