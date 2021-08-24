using AutoMapper;
using HubEntities.Database;
using HubEntities.Dto;
using System.Linq;

namespace API {
    public class AutoMapping : Profile {
        public AutoMapping() {
            CreateMap<User, UserDto>();
            CreateMap<User, TeamMemberDto>();
            CreateMap<Team, TeamDto>();
            CreateMap<Team, TeamInfoDto>();
            CreateMap<EditUserDto, User>();
            CreateMap<TeamJoinRequest, TeamJoinRequestDto>();
        }
    }
}