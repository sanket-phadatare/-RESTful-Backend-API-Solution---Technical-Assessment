using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ItemDto?> GetItemByIdAsync(int id)
        {
            var item = await _unitOfWork.Items.GetByIdAsync(id);
            return _mapper.Map<ItemDto>(item);
        }

        public async Task<IEnumerable<ItemDto>> GetItemsByProductIdAsync(int productId, int pageNumber, int pageSize)
        {
            // Verify if product exists
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} was not found.");
            }

            var items = await _unitOfWork.Items.FindPagedAsync(i => i.ProductId == productId, pageNumber, pageSize);
            return _mapper.Map<IEnumerable<ItemDto>>(items);
        }

        public async Task<ItemDto> CreateItemAsync(int productId, CreateItemDto createItemDto)
        {
            // Verify product exists
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} was not found.");
            }

            var item = _mapper.Map<Item>(createItemDto);
            item.ProductId = productId;

            await _unitOfWork.Items.AddAsync(item);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ItemDto>(item);
        }

        public async Task<bool> UpdateItemAsync(int id, UpdateItemDto updateItemDto)
        {
            var item = await _unitOfWork.Items.GetByIdAsync(id);
            if (item == null)
                return false;

            _mapper.Map(updateItemDto, item);

            _unitOfWork.Items.Update(item);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var item = await _unitOfWork.Items.GetByIdAsync(id);
            if (item == null)
                return false;

            _unitOfWork.Items.Delete(item);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
