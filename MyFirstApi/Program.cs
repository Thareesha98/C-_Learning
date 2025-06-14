using Microsoft.AspNetCore.OpenApi;
using Swashbuckle.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();


var blogs = new List<Blog>
{
    new Blog { Title = "First Blog", Body = "This is the content of the first blog." },
    new Blog { Title = "Second Blog", Body = "This is the content of the second blog." }
};

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    Console.WriteLine(context.Request.Path);
    await next.Invoke();
    Console.WriteLine(context.Response.StatusCode);

});


app.UseWhen(
    context => context.Request.Method != "GET",
    appBuilder => appBuilder.Use(async (context, next) =>
    {
        var extractedPassword = context.Request.Headers["X-Api-Key"];
        if (extractedPassword != "12345")
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Unauthorized access. Invalid Password-key.");
        }
        else
        {
            context.Response.StatusCode = 200;
            await next.Invoke();
        }
    }
    )
);


app.MapGet("/", () => "Welcome to the Blog API!");

app.MapGet("/blogs", () => Results.Ok(blogs));

app.MapGet("/blogs/{id}", (int id) => 
{
    if (id < 0 || id >= blogs.Count)
    {
        return Results.NotFound();
    }
    return Results.Ok(blogs[id]);
}).WithOpenApi(operation =>
{
    operation.Parameters[0].Description = "The ID of the blog to retrieve";
    operation.Summary = "Get a specific blog by ID";
    operation.Description = "Returns a single blog";
    return operation;
});

app.MapPost("/blogs", (Blog blog) => 
{
    blogs.Add(blog);
    return Results.Created($"/blogs/{blogs.Count - 1}", blog);
});

app.MapPut("/blogs/{id}", (int id, Blog blog) => 
{
    if (id < 0 || id >= blogs.Count)
    {
        return Results.NotFound();
    }
    blogs[id] = blog;
    return Results.Ok(blog);
});

app.MapDelete("/blogs/{id}", (int id) => 
{
    if (id < 0 || id >= blogs.Count)
    {
        return Results.NotFound();
    }
    blogs.RemoveAt(id);
    return Results.NoContent();
});


app.Run();





public class Blog
{
    public required string Title { get; set; }
    public required string Body { get; set; }
}