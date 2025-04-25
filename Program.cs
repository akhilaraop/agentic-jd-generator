using JobDescriptionAgent.Services;
using JobDescriptionAgent.Models;
using JobDescriptionAgent.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure strongly typed settings
builder.Services.Configure<AppSettings>(builder.Configuration);

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Razor Pages support
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddControllers();

// Register services
builder.Services.AddSingleton<LanguageModelService>();
builder.Services.AddScoped<IAgentPromptService, AgentPromptService>();

// Register agents
builder.Services.AddScoped<ClarifierAgent>();
builder.Services.AddScoped<GeneratorAgent>();
builder.Services.AddScoped<CritiqueAgent>();
builder.Services.AddScoped<ComplianceAgent>();
builder.Services.AddScoped<RewriterAgent>();
builder.Services.AddScoped<FinalizerAgent>();

// Register orchestrator
builder.Services.AddScoped<IAgenticWorkflowService, JDOrchestrator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

var app = builder.Build();

// Use CORS
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "JD Agent API v1");
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
