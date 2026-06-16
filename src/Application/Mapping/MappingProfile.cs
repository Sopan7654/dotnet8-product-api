using Application.DTOs.Item;
using Application.DTOs.Product;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product
        CreateMap<Product, ProductResponse>();
        CreateMap<Product, ProductWithItemsResponse>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        CreateMap<CreateProductRequest, Product>()
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(_ => DateTime.UtcNow));

        // Item
        CreateMap<Item, ItemResponse>();
        CreateMap<Item, ItemSummary>();
        CreateMap<CreateItemRequest, Item>();
    }
}
