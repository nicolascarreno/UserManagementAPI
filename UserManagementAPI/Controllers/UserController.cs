using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Utilities;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private static readonly ConcurrentDictionary<int, Utils.User> users = new()
    {
        [1] = new Utils.User { Name = "Ana", LastName = "García", Mail = "ana@example.com" },
        [2] = new Utils.User { Name = "Luis", LastName = "Pérez", Mail = "luis@example.com" }
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Utils.UserResponse>>> GetUsers()
    {
        return users
            .Select(u => new Utils.UserResponse
            {
                Id = u.Key,
                Name = u.Value.Name,
                LastName = u.Value.LastName,
                Mail = u.Value.Mail
            })
            .ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<Utils.UserResponse>>> GetUser(int id)
    {
        if (id <= 0)
            return BadRequest(new { Error = "The ID must be a positive integer" });

        if (!users.TryGetValue(id, out var user))
            return NotFound(new { Error = $"User with id {id} doesn't exist." });

        return Ok(new Utils.UserResponse
        {
            Id = id,
            Name = user.Name,
            LastName = user.LastName,
            Mail = user.Mail
        });
    }

    [HttpPost]
    public async Task<ActionResult<Utils.UserResponse>> PostUser([FromBody] Utils.UserInput newUser)
    {
        if (!Utils.ValidateUser(newUser, out var error))
            return BadRequest(new { Error = error });

        var user = new Utils.User
        {
            Name = newUser.Name,
            LastName = newUser.LastName,
            Mail = newUser.Mail
        };

        if (!users.TryAdd(newUser.Id, user))
            return BadRequest(new { Error = $"The user with ID {newUser.Id} already exists." });

        var createdUser = new Utils.UserResponse
        {
            Id = newUser.Id,
            Name = newUser.Name,
            LastName = newUser.LastName,
            Mail = newUser.Mail
        };

        return Created($"/api/user/{newUser.Id}", createdUser);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Utils.UserResponse>> PutUser(int id, [FromBody] Utils.UserInput updatedUser) 
    {
        if (id <= 0)
            return BadRequest(new { Error = "ID must be a positive integer" });

        if (!users.TryGetValue(id, out var existingUser))
            return NotFound(new { Error = $"User with ID {id} doesn't exist." });

        if (!Utils.ValidateUser(updatedUser, out var error))
            return BadRequest(new { Error = error });

        var updatedUserObj = new Utils.User
        {
            Name = updatedUser.Name,
            LastName = updatedUser.LastName,
            Mail = updatedUser.Mail
        };

        if (!users.TryUpdate(id, updatedUserObj, existingUser))
            return Conflict(new { Error = $"The user with ID {id} was modified by someone else." });

        return Ok(new Utils.UserResponse
        {
            Id = id,
            Name = updatedUser.Name,
            LastName = updatedUser.LastName,
            Mail = updatedUser.Mail
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        if (id <= 0)
            return BadRequest(new { Error = "ID must be a positive integer" });

        if (!users.TryRemove(id, out _))
            return NotFound(new { Error = $"User with ID {id} doesn't exist." });

        return NoContent();
    }
}
