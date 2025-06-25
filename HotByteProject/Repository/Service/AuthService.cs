using HotByteProject.DTO;
using HotByteProject.Models;
using System.Security.Claims;
using System.Text;
using System;
using HotByteProject.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Globalization;
using HotByteProject.Services.Implementations;

namespace HotByteProject.Repository.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<string?> RegisterAsync(RegisterDTO model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                return null;

            string normalizedRole = model.Role?.Trim().ToLower();
            string finalRole = normalizedRole switch
            {
                "admin" => "Admin",
                "restaurant" => "Restaurant",
                "user" or "customer" => "User",
                _ => "User"
            };

            var hashedPassword = HashPassword(model.Password);

            //user Registration
            if (finalRole == "User")
            {
                if (string.IsNullOrWhiteSpace(model.UserName) ||
                    string.IsNullOrWhiteSpace(model.UserAddress) ||
                    string.IsNullOrWhiteSpace(model.UserContact))
                {
                    throw new ArgumentException("User Name, Address, and Contact are required.");
                }

                var user = new User
                {
                    Name = model.UserName,
                    Email = model.Email,
                    Address = model.UserAddress,
                    Role = finalRole,
                    PasswordHash = hashedPassword
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return GenerateToken(user);
            }

            //Restaurant Registration
            if (finalRole == "Restaurant")
            {
                if (string.IsNullOrWhiteSpace(model.RestaurantName) ||
                    string.IsNullOrWhiteSpace(model.RestaurantAddress) ||
                    string.IsNullOrWhiteSpace(model.RestaurantContact))
                {
                    throw new ArgumentException("Restaurant Name, Address, and Contact are required.");
                }

                var user = new User
                {
                    Name = model.RestaurantName, 
                    Email = model.Email,
                    Address = model.RestaurantAddress,
                    Role = finalRole,
                    PasswordHash = hashedPassword
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var restaurant = new Restaurant
                {
                    UserId = user.UserId,
                    RestaurantName = model.RestaurantName,
                    Location = model.RestaurantAddress,
                    ContactNumber = model.RestaurantContact
                };

                _context.Restaurants.Add(restaurant);
                await _context.SaveChangesAsync();

                return GenerateToken(user);
            }

            //Admin Registration
            var defaultUser = new User
            {
                Name = "Admin",
                Email = model.Email,
                Address = "N/A",
                Role = finalRole,
                PasswordHash = hashedPassword
            };

            _context.Users.Add(defaultUser);
            await _context.SaveChangesAsync();

            return GenerateToken(defaultUser);
        }


        public async Task<string?> LoginAsync(LoginDTO model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
                return null;

            return GenerateToken(user);
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Base claims  
            var claims = new List<Claim>
           {
               new Claim(ClaimTypes.Name, user.Email),
               new Claim(ClaimTypes.Role, CultureInfo.CurrentCulture.TextInfo.ToTitleCase(user.Role)),
               new Claim("UserId", user.UserId.ToString())
           };

            // Add RestaurantId if the user is a Restaurant  
            if (user.Role == "Restaurant")
            {
                var restaurant = _context.Restaurants.FirstOrDefault(r => r.UserId == user.UserId);
                if (restaurant != null)
                {
                    claims.Add(new Claim("RestaurantId", restaurant.RestaurantId.ToString()));
                }
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(5),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            return HashPassword(inputPassword) == storedHash;
        }
    }
}
