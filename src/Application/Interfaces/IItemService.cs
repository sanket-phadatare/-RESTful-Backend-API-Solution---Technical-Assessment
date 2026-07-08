using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IItemService
    {
        Task<ItemDto?> GetItemByIdAsync(int id);
        Task<IEnumerable<ItemDto>> GetItemsByProductIdAsync(int productId, int pageNumber, int pageSize);
        Task<ItemDto> CreateItemAsync(int productId, CreateItemDto createItemDto);
        Task<bool> UpdateItemAsync(int id, UpdateItemDto updateItemDto);
        Task<bool> DeleteItemAsync(int id);
    }
}
