using Tools.AspNet.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAdvancedSecurity(so =>
{
    so.EnableCors(co =>
    {
        co.AddOrigins(new Uri("https://localhost:7228"))
          .WithMethods(Methods.Get)
          .AddHeaders(new HeaderInfo("X-User-Agent", value => value is "BlazorApp"), new HeaderInfo("Y-X-MyCustomHeader", (value) => Guid.TryParse(value, out _)));
    });

    so.EnableRate(ro => ro.RateLimitPerMinute = 10);
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAdvancedSecurity();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
