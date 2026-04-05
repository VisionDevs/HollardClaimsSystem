using HollardClaimsSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

// Create sample data directory
var dataPath = Path.Combine(app.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(dataPath);
Directory.CreateDirectory(Path.Combine(dataPath, "Documents"));
Directory.CreateDirectory(Path.Combine(dataPath, "Metadata"));

app.Run();