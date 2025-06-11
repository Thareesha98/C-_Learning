var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpLogging();
builder.Services.AddControllers();



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
app.UseHttpLogging();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapGet("/", () => "Root.. Helloo World");

app.MapControllers();

app.Run();




