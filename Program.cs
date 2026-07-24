using BlazingPizza;
using Microsoft.Data.Sqlite;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Combine(builder.Environment.ContentRootPath, "pizza.db");

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSqlite<PizzaStoreContext>($"Data Source={dbPath}");
builder.Services.AddScoped<OrderState>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Initialize the database
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PizzaStoreContext>();

    var needsRecreate = false;
    if (File.Exists(dbPath))
    {
        using var sqliteConnection = new SqliteConnection($"Data Source={dbPath}");
        sqliteConnection.Open();
        using var command = sqliteConnection.CreateCommand();
        command.CommandText = "SELECT count(name) FROM sqlite_master WHERE type='table' AND name='Orders';";
        var ordersTableCount = (long)command.ExecuteScalar();
        if (ordersTableCount == 0)
        {
            needsRecreate = true;
        }
    }

    if (needsRecreate)
    {
        db.Database.EnsureDeleted();
    }

    if (db.Database.EnsureCreated())
    {
        SeedData.Initialize(db);
    }
    else if (!db.Specials.Any())
    {
        SeedData.Initialize(db);
    }
}

app.Run();

