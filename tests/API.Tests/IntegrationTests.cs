using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.DTOs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.Tests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private SqliteConnection? _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the application's original ApplicationDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Create open connection to SQLite in-memory database
                _connection = new SqliteConnection("Filename=:memory:");
                _connection.Open();

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                // Build service provider and seed schema
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class IntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public IntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task RegistrationAndLoginFlow_Succeeds()
        {
            // Register a user
            var regRequest = new RegisterRequest 
            { 
                Username = "alice", 
                Password = "password123", 
                Role = "User" 
            };
            var regResponse = await _client.PostAsJsonAsync("api/v1/auth/register", regRequest);
            Assert.Equal(HttpStatusCode.OK, regResponse.StatusCode);

            // Log in with the credentials
            var loginRequest = new LoginRequest 
            { 
                Username = "alice", 
                Password = "password123" 
            };
            var loginResponse = await _client.PostAsJsonAsync("api/v1/auth/login", loginRequest);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.NotNull(tokens);
            Assert.NotEmpty(tokens.AccessToken);
            Assert.NotEmpty(tokens.RefreshToken);
        }

        [Fact]
        public async Task GetProducts_Unauthenticated_ReturnsOkForAnonymous()
        {
            // Act: Products endpoint supports AllowAnonymous for GET
            var response = await _client.GetAsync("api/v1/products");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateProduct_WithoutJwt_ReturnsUnauthorized()
        {
            // Act: attempt post without header
            var response = await _client.PostAsJsonAsync("api/v1/products", new CreateProductDto 
            { 
                ProductName = "Forbidden Product" 
            });

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateProduct_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange: authenticate a user
            var regRequest = new RegisterRequest 
            { 
                Username = "adminuser", 
                Password = "adminpassword", 
                Role = "Admin" 
            };
            await _client.PostAsJsonAsync("api/v1/auth/register", regRequest);
            
            var loginResponse = await _client.PostAsJsonAsync("api/v1/auth/login", new LoginRequest 
            { 
                Username = "adminuser", 
                Password = "adminpassword" 
            });
            var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
            
            var authClient = _factory.CreateClient();
            authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

            // Act: attempt to post with empty ProductName (violates FluentValidation rule)
            var response = await authClient.PostAsJsonAsync("api/v1/products", new CreateProductDto 
            { 
                ProductName = "" 
            });

            // Assert: ValidationFilter intercepts and middleware handles it
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
