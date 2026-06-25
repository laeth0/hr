using Hr.BLL;
using Hr.PL.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBllDependencies(builder.Configuration);
builder.Services.AddControllers(options =>
{
    // Global validation filter — runs for every action automatically
    options.Filters.Add<GlobalValidationFilter>();
});
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
