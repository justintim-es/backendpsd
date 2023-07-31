using AutoMapper;
using backend.Models;
using backend.Models.Dtos;

namespace latest;

class AProfile : Profile {
    public AProfile()
    {
        CreateMap<ShopDto, Shop>()
        .ForMember(sd => sd.UserName, opt => opt.MapFrom(s => s.Email));
    }
}