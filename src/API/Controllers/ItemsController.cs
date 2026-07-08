using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/products/{productId}/[controller]")]
    [Authorize]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;

        public ItemsController(IItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ItemDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetItemsByProductId(int productId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;

            var items = await _itemService.GetItemsByProductIdAsync(productId, pageNumber, pageSize);
            return Ok(items);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ItemDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int productId, int id)
        {
            var item = await _itemService.GetItemByIdAsync(id);
            if (item == null || item.ProductId != productId)
            {
                return NotFound(new { message = $"Item with ID {id} was not found under product {productId}." });
            }
            return Ok(item);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ItemDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(int productId, [FromBody] CreateItemDto createItemDto)
        {
            var item = await _itemService.CreateItemAsync(productId, createItemDto);
            return CreatedAtAction(nameof(GetById), new { productId = productId, id = item.Id }, item);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int productId, int id, [FromBody] UpdateItemDto updateItemDto)
        {
            // Verify item exists and matches productId
            var item = await _itemService.GetItemByIdAsync(id);
            if (item == null || item.ProductId != productId)
            {
                return NotFound(new { message = $"Item with ID {id} was not found under product {productId}." });
            }

            await _itemService.UpdateItemAsync(id, updateItemDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int productId, int id)
        {
            // Verify item exists and matches productId
            var item = await _itemService.GetItemByIdAsync(id);
            if (item == null || item.ProductId != productId)
            {
                return NotFound(new { message = $"Item with ID {id} was not found under product {productId}." });
            }

            await _itemService.DeleteItemAsync(id);
            return NoContent();
        }
    }
}
