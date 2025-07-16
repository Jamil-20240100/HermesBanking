using AutoMapper;
using HermesBanking.Core.Application.DTOs.User;
using HermesBanking.Core.Application.ViewModels.Loan;
using HermesBanking.Core.Application.ViewModels.User;

namespace HermesBanking.Core.Application.Mappings.DTOsAndViewModels
{
    public class UserDtoMappingProfile : Profile
    {
        public UserDtoMappingProfile()
        {
            CreateMap<UserDto, ClientSelectionViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CurrentDebtAmount, opt => opt.Ignore()) 
                .ForMember(dest => dest.IsSelected, opt => opt.Ignore());


            CreateMap<UserDto, UserViewModel>()
                .ReverseMap();

            CreateMap<UserDto, DeleteUserViewModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ReverseMap()
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.InitialAmount, opt => opt.Ignore());


            CreateMap<UserDto, UpdateUserViewModel>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<SaveUserDto, UpdateUserViewModel>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<SaveUserDto, CreateUserViewModel>()
                .ReverseMap();

            CreateMap<SaveUserDto, RegisterUserViewModel>()
                .ReverseMap();

            CreateMap<LoginDto, LoginViewModel>()
                .ReverseMap();
        }
    }
}