using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            var products = await _unitOfWork.Products.GetPagedAsync(pageNumber, pageSize);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, string username)
        {
            var product = _mapper.Map<Product>(createProductDto);
            product.CreatedBy = username;
            product.CreatedOn = DateTime.UtcNow;

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> UpdateProductAsync(int id, UpdateProductDto updateProductDto, string username)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return false;

            _mapper.Map(updateProductDto, product);
            product.ModifiedBy = username;
            product.ModifiedOn = DateTime.UtcNow;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return false;

            _unitOfWork.Products.Delete(product);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
