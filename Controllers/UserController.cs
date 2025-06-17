using Microsoft.AspNetCore.Mvc;

// User model
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpGet("{id}")]
    public ActionResult<User> GetUser(int id)
    {
        return new User { Id = id, Name = $"User {id}" };
    }
}
