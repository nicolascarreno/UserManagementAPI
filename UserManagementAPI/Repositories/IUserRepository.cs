using UserManagementAPI.Utilities;

public interface IUserRepository
{
    Task<IEnumerable<Utils.User>> GetUsersAsync();
    Task<Utils.User?> GetUserByIdAsync(int id);
    Task<bool> CreateUserAsync(Utils.User user);
    Task<bool> UpdateUserAsync(int id, Utils.User updatedUser);
    Task<bool> DeleteUserAsync(int id);
}