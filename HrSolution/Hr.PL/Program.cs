using Hr.BLL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddBllDependencies(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
