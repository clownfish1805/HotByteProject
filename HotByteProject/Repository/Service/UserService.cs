using System.Security.Cryptography;
using System.Text;
using HotByteProject.Context;
using HotByteProject.DTO;
using HotByteProject.Models;
using HotByteProject.Services;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public async Task<UpdateUserDTO?> GetUserDtoByIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        return new UpdateUserDTO
        {
            Email = user.Email,
            Password = "", 
            UserName = user.Name,
            UserAddress = user.Address,
            UserContact = user.Contact
        };
    }

    public async Task<bool> UpdateUserAsync(int userId, UpdateUserDTO dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.Email = dto.Email;
        user.Name = dto.UserName;
        user.Address = dto.UserAddress;

        // ❌ BAD: this stores the raw password
        // user.PasswordHash = dto.Password;

        // ✅ GOOD: hash the password before saving
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            // ✅ Hash the new password only if provided
            user.PasswordHash = HashPassword(dto.Password);
        }


        user.Contact = dto.UserContact;

        await _context.SaveChangesAsync();
        return true;
    }

}
