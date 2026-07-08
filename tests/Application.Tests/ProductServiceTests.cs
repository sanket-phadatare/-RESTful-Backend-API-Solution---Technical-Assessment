using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _service = new ProductService(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ReturnsProductDto()
        {
            // Arrange
            int productId = 1;
            var product = new Product { Id = productId, ProductName = "Test Product" };
            var productDto = new ProductDto { Id = productId, ProductName = "Test Product" };

            _mockUow.Setup(u => u.Products.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockMapper.Setup(m => m.Map<ProductDto>(product))
                .Returns(productDto);

            // Act
            var result = await _service.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("Test Product", result.ProductName);
        }

        [Fact]
        public async Task CreateProductAsync_WithValidDto_SavesProductAndReturnsDto()
        {
            // Arrange
            var createDto = new CreateProductDto { ProductName = "New Product" };
            var product = new Product { ProductName = "New Product" };
            var productDto = new ProductDto { Id = 1, ProductName = "New Product" };
            string username = "testuser";

            _mockMapper.Setup(m => m.Map<Product>(createDto))
                .Returns(product);
            _mockUow.Setup(u => u.Products.AddAsync(product))
                .Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<ProductDto>(product))
                .Returns(productDto);

            // Act
            var result = await _service.CreateProductAsync(createDto, username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("New Product", result.ProductName);
            _mockUow.Verify(u => u.Products.AddAsync(product), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}
