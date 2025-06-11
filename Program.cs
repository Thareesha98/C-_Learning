var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers();


var app = builder.Build();

app.UseHttpLogging();

builder.Services.AddHttpLogging((logging) => { });
app.Use(async (context, next) =>
{
    Console.WriteLine("Middleware 1: Before next middleware");
    await next.Invoke(); // Call the next middleware in the pipeline
    Console.WriteLine("Middleware 1: After next middleware");
});

app.MapGet("/", () => "Root.. Helloo World");

//app.MapControllers();

app.Run();




