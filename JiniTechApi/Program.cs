using JiniTechApi.Services;
using JiniTechApi.Helpers;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

// Swagger setup
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JiniTechApi",
        Version = "1.0.0",
        Description = "API for Jini Tech AI Image and Video Creator"
    });

    c.OperationFilter<SwaggerFileOperationFilter>();
    c.EnableAnnotations();
});

// Enable CORS for any origin (mobile devices)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register HttpClient
builder.Services.AddHttpClient();

// Register custom services
builder.Services.AddSingleton<ApiKeyStore>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    return new ApiKeyStore(env);
});
builder.Services.AddSingleton<ApiKeyService>();
builder.Services.AddSingleton<FirebaseService>(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    var cfg = sp.GetRequiredService<IConfiguration>();
    return new FirebaseService(http, cfg);
});
builder.Services.AddSingleton<AiProviderService>(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    var cfg = sp.GetRequiredService<IConfiguration>();
    return new AiProviderService(http, cfg);
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "JiniTechApi 1.0.0");
    });
}

// Production-ready HTTPS redirection (optional)
app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

// Run API on all network interfaces for LAN / cloud access
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
// You can change port if needed
