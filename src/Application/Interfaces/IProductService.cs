using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(int pageNumber, int pageSize);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, string username);
        Task<bool> UpdateProductAsync(int id, UpdateProductDto updateProductDto, string username);
        Task<bool> DeleteProductAsync(int id);
    }
}
