using AutoMapper;
using HotByteProject.DTO;
using HotByteProject.Models;

namespace HotByteProject.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Menu, MenuDTO>().ReverseMap();

            CreateMap<Menu, MenuDetailsDTO>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant.RestaurantName));

            CreateMap<CartItem, CartItemResponseDTO>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Menu.ItemName))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Menu.Price))
                .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Menu.Restaurant.RestaurantName))
                .ForMember(dest => dest.MenuId, opt => opt.MapFrom(src => src.MenuId));
        }
    }
}
