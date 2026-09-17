using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Utilities;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _repository;

    public UserController(IUserRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Utils.UserResponse>>> GetUsers()
    {
        var users = await _repository.GetUsersAsync();
        return users
            .Select(u => new Utils.UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                LastName = u.LastName,
                Mail = u.Mail
            })
            .ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<Utils.UserResponse>>> GetUser(int id)
    {
        if (id <= 0)
            return BadRequest(new { Error = "The ID must be a positive integer" });
   
        var user = await _repository.GetUserByIdAsync(id);

        if(user is null)
        {
            return NotFound(new { Error = $"User with id {id} doesn't exist." });
        }        

        return Ok(new Utils.UserResponse
        {
            Id = user.Id,
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
            Mail = newUser.Mail,
            Id = newUser.Id
        };

        if (!await _repository.CreateUserAsync(user))
        {
            return BadRequest(new { Error = $"The user with ID {newUser.Id} already exists." });
        }

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

        var user = await _repository.GetUserByIdAsync(id);
        if (user is null)
        {
            return NotFound(new { Error = $"User with ID {id} doesn't exist." });
        }

        if (!Utils.ValidateUser(updatedUser, out var error))
            return BadRequest(new { Error = error });

        var updatedUserObj = new Utils.User
        {
            Name = updatedUser.Name,
            LastName = updatedUser.LastName,
            Mail = updatedUser.Mail,
            Id = id
        };

        if (!await _repository.UpdateUserAsync(id, updatedUserObj, user)) {
            return Conflict(new { Error = $"The user with ID {id} was modified by someone else." });
        };

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

        if (!await _repository.DeleteUserAsync(id))
        {
            return NotFound(new { Error = $"User with ID {id} doesn't exist." });
        }

        return NoContent();
    }
}
