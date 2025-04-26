using JobDescriptionAgent.Services;
using JobDescriptionAgent.Models;
using JobDescriptionAgent.Data;
using Microsoft.EntityFrameworkCore;
using MediatR;

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

// Register MediatR
builder.Services.AddMediatR(typeof(Program));

// Register Swagger with separate docs for Commands and Queries
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("commands", new() { Title = "Commands API", Version = "v1" });
    c.SwaggerDoc("queries", new() { Title = "Queries API", Version = "v1" });
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (docName == "commands")
            return apiDesc.GroupName == "commands";
        if (docName == "queries")
            return apiDesc.GroupName == "queries";
        return false;
    });
    
    // Add XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

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

// Configure Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/commands/swagger.json", "Commands API v1");
    c.SwaggerEndpoint("/swagger/queries/swagger.json", "Queries API v1");
});

// Only use HTTPS redirection in production
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

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
