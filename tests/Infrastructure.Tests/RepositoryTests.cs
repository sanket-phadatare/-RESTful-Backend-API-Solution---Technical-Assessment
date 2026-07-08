using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests
{
    public class RepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ApplicationDbContext> _contextOptions;
        private readonly ApplicationDbContext _context;
        private readonly GenericRepository<Product> _repository;

        public RepositoryTests()
        {
            // Create and open a connection to the SQLite database
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(_contextOptions);
            _context.Database.EnsureCreated();

            _repository = new GenericRepository<Product>(_context);
        }

        [Fact]
        public async Task AddAsync_SavesProductToDatabase()
        {
            // Arrange
            var product = new Product 
            { 
                ProductName = "Test Product", 
                CreatedBy = "Admin", 
                CreatedOn = DateTime.UtcNow 
            };

            // Act
            await _repository.AddAsync(product);
            await _context.SaveChangesAsync();

            // Assert
            var savedProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductName == "Test Product");
            Assert.NotNull(savedProduct);
            Assert.Equal("Test Product", savedProduct.ProductName);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsCorrectPageOfData()
        {
            // Arrange
            for (int i = 1; i <= 5; i++)
            {
                _context.Products.Add(new Product 
                { 
                    ProductName = $"Product {i}", 
                    CreatedBy = "Admin", 
                    CreatedOn = DateTime.UtcNow 
                });
            }
            await _context.SaveChangesAsync();

            // Act
            var page = await _repository.GetPagedAsync(pageNumber: 2, pageSize: 2);

            // Assert
            var list = page.ToList();
            Assert.Equal(2, list.Count);
            Assert.Equal("Product 3", list[0].ProductName);
            Assert.Equal("Product 4", list[1].ProductName);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Close();
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
