using System.Text.Json;
using System.Xml.Serialization;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var samplePerson = new Person { UserName = "Thareesha98", UserAge = 30 };

app.MapGet("/", () => "Hello World!   I am the Root");

app.MapGet("/manual-json", () =>
{
    var jsonString = JsonSerializer.Serialize(samplePerson);
    return TypedResults.Text(jsonString, "application/json");
});

app.MapGet("/custom-serializer", () =>
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    var customJsonString = JsonSerializer.Serialize(samplePerson, options);
    return TypedResults.Text(customJsonString, "application/json");
});

app.MapGet("/json", () => {
    return TypedResults.Json(samplePerson);
});

app.MapGet("/auto", () =>
{
    return samplePerson;
});

app.MapGet("/xml", () =>
{
    var xmlSerializer = new XmlSerializer(typeof(Person));
    var stringWriter = new StringWriter();
    xmlSerializer.Serialize(stringWriter, samplePerson);
    var xmlOutput = stringWriter.ToString();
    return TypedResults.Text(xmlOutput, "application/xml");
});

app.MapPost("/submit", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();

    var person = JsonSerializer.Deserialize<Person>(body);

    if (person == null)
        return Results.BadRequest("Invalid JSON data.");

    return Results.Ok($"[Manual] Username = {person.UserName}, Age = {person.UserAge}");

});

app.Run();

public class Person
{
    public required string UserName { get; set; }
    public required int UserAge { get; set; }
}
