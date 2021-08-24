using AutoMapper;
using HubEntities.Database;
using HubEntities.Dto;
using System.Linq;

namespace API {
    public class AutoMapping : Profile {
        public AutoMapping() {
            CreateMap<User, UserDto>();
            CreateMap<Team, TeamDto>();
            CreateMap<EditUserDto, User>();
        }
    }
}