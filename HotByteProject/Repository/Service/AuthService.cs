using HotByteProject.Context;
using HotByteProject.DTO;
using HotByteProject.Models;
using HotByteProject.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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

        // REGISTER METHOD
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

            string hashedPassword = HashPassword(model.Password);

            // Register as User
            if (finalRole == "User")
            {
                if (string.IsNullOrWhiteSpace(model.UserName) ||
                    string.IsNullOrWhiteSpace(model.UserAddress) ||
                    string.IsNullOrWhiteSpace(model.UserContact))
                    throw new ArgumentException("User Name, Address, and Contact are required.");

                var user = new User
                {
                    Name = model.UserName,
                    Email = model.Email,
                    Address = model.UserAddress,
                    Contact = model.UserContact,
                    Role = finalRole,
                    PasswordHash = hashedPassword
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return GenerateToken(user);
            }

            // Register as Restaurant
            if (finalRole == "Restaurant")
            {
                if (string.IsNullOrWhiteSpace(model.RestaurantName) ||
                    string.IsNullOrWhiteSpace(model.RestaurantAddress) ||
                    string.IsNullOrWhiteSpace(model.RestaurantContact))
                    throw new ArgumentException("Restaurant Name, Address, and Contact are required.");

                var user = new User
                {
                    Name = model.RestaurantName,
                    Email = model.Email,
                    Address = model.RestaurantAddress,
                    Contact = model.RestaurantContact,
                    ImageUrl = model.ImageUrl,

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

            // Register as Admin
            var adminUser = new User
            {
                Name = "Admin",
                Email = model.Email,
                Address = "N/A",
                Role = finalRole,
                PasswordHash = hashedPassword
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            return GenerateToken(adminUser);
        }

        // LOGIN METHOD
        public async Task<AuthResponseDTO?> LoginAsync(LoginDTO model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
                return null;

            if (!VerifyPassword(model.Password, user.PasswordHash))
                return null;

            var token = GenerateToken(user);
            var role = user.Role;

            var response = new AuthResponseDTO
            {
                Token = token,
                Role = role
            };

            if (role == "Restaurant")
            {
                var restaurant = await _context.Restaurants
                    .FirstOrDefaultAsync(r => r.UserId == user.UserId);

                if (restaurant != null)
                    response.RestaurantId = restaurant.RestaurantId;
            }
            else if (role == "User")
            {
                response.UserId = user.UserId;
            }

            return response;
        }


        // GENERATE JWT
        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, CultureInfo.CurrentCulture.TextInfo.ToTitleCase(user.Role)),
                new Claim("UserId", user.UserId.ToString())
            };

            // Include RestaurantId for restaurant role
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

        // HASH PASSWORD (SHA256)
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // VERIFY PASSWORD
        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            var hashedInput = HashPassword(inputPassword);
            return hashedInput == storedHash;
        }
    }
}
