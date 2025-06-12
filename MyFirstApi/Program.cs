var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpLogging();
//builder.Services.AddControllers();

builder.Services.AddScoped<IMyService, MyService>();


//builder.Services.AddHttpLogging((logging) => { });
//app.Use(async (context, next) =>
//{
//    Console.WriteLine("Middleware 1: Before next middleware");
//    await next.Invoke(); // Call the next middleware in the pipeline
//    Console.WriteLine("Middleware 1: After next middleware");
//});


//if(app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//   // app.UseSession
//}
var app = builder.Build();
//app.UseHttpLogging();

//app.UseHttpsRedirection();
//app.UseAuthorization();

app.Use(async (context, next) =>
{
    var myService = context.RequestServices.GetRequiredService<IMyService>();
    myService.LogCreation("First MiddleWare");
    await next.Invoke();
});

app.Use(async (context, next) =>
{
    var myService = context.RequestServices.GetRequiredService<IMyService>();
    myService.LogCreation("Second MiddleWare");
    await next.Invoke();
});

app.MapGet("/", (IMyService myService) =>
{
    myService.LogCreation("Hello, World!");
    return Results.Ok("Check the console for service creation logs");
});

//app.MapControllers();

app.Run();










public interface IMyService
{
    void LogCreation(string message);
}

public class MyService : IMyService
{
    private readonly int _serviceId;
    public MyService()
    {
        _serviceId = new Random().Next(100000, 999999);
    }

    public void LogCreation(string message)
    {
        Console.WriteLine($"Service {_serviceId} created with message: {message}");
    }
}




