using System.Collections.Concurrent;
using UserManagementAPI.Utilities;

public class InMemoryUserRepository : IUserRepository
{
    private static readonly ConcurrentDictionary<int, Utils.User> users = new ()
    {
        [1] = new Utils.User { Name = "Ana", LastName = "García", Mail = "ana@example.com", Id = 1 },
        [2] = new Utils.User { Name = "Luis", LastName = "Pérez", Mail = "luis@example.com", Id = 2 }
    };

    public Task<IEnumerable<Utils.User>> GetUsersAsync()
    {
        return Task.FromResult(users.Values.AsEnumerable());
    }

    public Task<Utils.User?> GetUserByIdAsync(int id)
    {
        users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<bool> CreateUserAsync(Utils.User user)
    {
        return Task.FromResult(users.TryAdd(user.Id, user));
    }

    public Task<bool> UpdateUserAsync(int id, Utils.User updatedUser, Utils.User existingUser)
    {
        return Task.FromResult(users.TryUpdate(id, updatedUser, existingUser));
    }

    public Task<bool> DeleteUserAsync(int id)
    {
        return Task.FromResult(users.TryRemove(id, out _));
    }
}