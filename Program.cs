using JobDescriptionAgent.Services;
using JobDescriptionAgent.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure strongly typed settings
builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddSingleton<LanguageModelService>();
builder.Services.AddSingleton<JDOrchestrator>();
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
app.UseAuthorization();
app.MapControllers();
app.Run();
