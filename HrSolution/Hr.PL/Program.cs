using Hr.BLL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBllDependencies(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
