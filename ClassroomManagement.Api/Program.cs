using ClassroomManagement.Api.Repositories;
using ClassroomManagement.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "University Classroom Management Web API",
        Version = "v1",
        Description = "API для керування університетськими аудиторіями та їх бронюванням (ЛР №2 з МРПЗ)"
    });
});

// Register singletons for in-memory persistence across HTTP requests
builder.Services.AddSingleton<IClassroomRepository, InMemoryClassroomRepository>();
builder.Services.AddSingleton<ClassroomValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
