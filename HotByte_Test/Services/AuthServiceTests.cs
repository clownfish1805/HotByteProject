using HotByteProject.Context;
using HotByteProject.DTO;
using HotByteProject.Models;
using HotByteProject.Repository.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotByteProject.Tests
{
    public class AuthServiceTests
    {
        private AuthService _authService;
        private AppDbContext _context;
        private IConfiguration _config;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "AuthTestDb")
                .Options;
            _context = new AppDbContext(options);

            var configData = new Dictionary<string, string>
            {
               { "Jwt:Key", "12345678901234567890123456789012" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };
            _config = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

            _authService = new AuthService(_context, _config);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task RegisterAsync_ShouldRegisterUser()
        {
            var dto = new RegisterDTO
            {
                Email = "user@test.com",
                Password = "password",
                Role = "User",
                UserName = "Test User",
                UserAddress = "Test Address",
                UserContact = "1234567890"
            };

            var token = await _authService.RegisterAsync(dto);
            Assert.IsNotNull(token);
        }

        [Test]
        public async Task RegisterAsync_ShouldRegisterRestaurant()
        {
            var dto = new RegisterDTO
            {
                Email = "rest@test.com",
                Password = "password",
                Role = "Restaurant",
                RestaurantName = "Test Resto",
                RestaurantAddress = "Resto Address",
                RestaurantContact = "9876543210"
            };

            var token = await _authService.RegisterAsync(dto);
            Assert.IsNotNull(token);
        }

        [Test]
        public async Task RegisterAsync_ShouldRegisterAdmin()
        {
            var dto = new RegisterDTO
            {
                Email = "admin@test.com",
                Password = "password",
                Role = "Admin"
            };

            var token = await _authService.RegisterAsync(dto);
            Assert.IsNotNull(token);
        }

        [Test]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsValid()
        {
            var dto = new RegisterDTO
            {
                Email = "login@test.com",
                Password = "password",
                Role = "User",
                UserName = "Login Test",
                UserAddress = "Address",
                UserContact = "1112223333"
            };

            await _authService.RegisterAsync(dto);

            var loginDto = new LoginDTO
            {
                Email = dto.Email,
                Password = dto.Password
            };

            var token = await _authService.LoginAsync(loginDto);
            Assert.IsNotNull(token);
        }

        [Test]
        public async Task LoginAsync_ShouldReturnNull_WhenCredentialsInvalid()
        {
            var loginDto = new LoginDTO
            {
                Email = "nonexistent@test.com",
                Password = "wrongpass"
            };

            var token = await _authService.LoginAsync(loginDto);
            Assert.IsNull(token);
        }
    }
}
