using DartTournamentPlaner.API.Services;
using DartTournamentPlaner.API.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Add API services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Dart Tournament Planner API", 
        Version = "v1",
        Description = "REST API für die Dart Tournament Planner Anwendung"
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()  
              .AllowAnyHeader();
    });
});

// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Add Entity Framework (In-Memory for API storage)
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseInMemoryDatabase("TournamentApiDb"));

// Register API services
builder.Services.AddScoped<ITournamentApiService, TournamentApiService>();
builder.Services.AddScoped<IMatchApiService, MatchApiService>();
builder.Services.AddSingleton<ITournamentSyncService, TournamentSyncService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dart Tournament Planner API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
    });
}

// Serve static files (for demo page)
app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.MapHub<TournamentHub>("/tournamentHub");

// Health check endpoint
app.MapGet("/health", () => "Dart Tournament Planner API is running!");

// Demo page endpoint
app.MapGet("/demo", () => Results.Redirect("/demo.html"));

app.Run();