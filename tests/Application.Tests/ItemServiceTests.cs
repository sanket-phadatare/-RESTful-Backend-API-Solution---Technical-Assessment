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
    public class ItemServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ItemService _service;

        public ItemServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _service = new ItemService(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task CreateItemAsync_ProductNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            int productId = 99;
            var createDto = new CreateItemDto { Quantity = 5 };

            _mockUow.Setup(u => u.Products.GetByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreateItemAsync(productId, createDto));
        }

        [Fact]
        public async Task CreateItemAsync_ProductExists_SavesAndReturnsItemDto()
        {
            // Arrange
            int productId = 1;
            var product = new Product { Id = productId, ProductName = "Product" };
            var createDto = new CreateItemDto { Quantity = 5 };
            var item = new Item { Quantity = 5, ProductId = productId };
            var itemDto = new ItemDto { Id = 1, ProductId = productId, Quantity = 5 };

            _mockUow.Setup(u => u.Products.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockMapper.Setup(m => m.Map<Item>(createDto))
                .Returns(item);
            _mockUow.Setup(u => u.Items.AddAsync(item))
                .Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<ItemDto>(item))
                .Returns(itemDto);

            // Act
            var result = await _service.CreateItemAsync(productId, createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(5, result.Quantity);
            _mockUow.Verify(u => u.Items.AddAsync(item), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}
