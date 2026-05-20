using WebApplication1.Services.AiAgent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register your AI agent service
builder.Services.AddSingleton<AiAgentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // app.UseSwaggerUI(); // disable auto-open
}

// Serve index.html and other static files
app.UseDefaultFiles();
app.UseStaticFiles();

// Optional: disable HTTPS redirection for local testing
// app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();
app.Run();
