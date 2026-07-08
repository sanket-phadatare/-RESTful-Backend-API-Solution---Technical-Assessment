using AutoMapper;
using Domain.Entities;
using Application.DTOs;

namespace Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>();
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();

            CreateMap<Item, ItemDto>();
            CreateMap<CreateItemDto, Item>();
            CreateMap<UpdateItemDto, Item>();

            CreateMap<User, UserDto>();
        }
    }
}
