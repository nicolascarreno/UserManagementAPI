using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Data;
using UserManagementAPI.Utilities;

public class EfUserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public EfUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Utils.User>> GetUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<Utils.User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<bool> CreateUserAsync(Utils.User user)
    {
        _context.Users.Add(user);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateUserAsync(int id, Utils.User updatedUser)
    {
         var existing = await _context.Users.FindAsync(id);
        if (existing is null) return false;

        existing.Name = updatedUser.Name;
        existing.LastName = updatedUser.LastName;
        existing.Mail = updatedUser.Mail;

        return await _context.SaveChangesAsync() > 0;
    }

     public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return false;

        _context.Users.Remove(user);
        return await _context.SaveChangesAsync() > 0;
    }
}